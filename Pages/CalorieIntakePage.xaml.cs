using FitnessApp.Models;
using Microsoft.Maui.Storage;
using FitnessApp.Services;

namespace FitnessApp.Pages
{
    public partial class CalorieIntakePage : ContentPage
    {
        private List<FoodItem> filteredFoods = new();
        private List<FoodItem> eatenFoods = new();

        private double dailyCalorieNeed;
        private double caloriesConsumed;
        private double proteinConsumed;

        private FoodItem? selectedFood;

        public CalorieIntakePage()
        {
            InitializeComponent();
            

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await FoodDatabase.InitializeAsync();
            LoadFoodList();
            await LoadProfileData();

            var todayLog = await FoodLogService.GetTodayAsync();
            eatenFoods = todayLog.Foods;

            caloriesConsumed = todayLog.Calories;
            proteinConsumed = todayLog.Protein;

            EatenListView.ItemsSource = null;
            EatenListView.ItemsSource = eatenFoods;
            await DailyGoalService.UpsertTodayGoalFromProfileAsync();

            UpdateRemainingCalories();
        }

        private async Task LoadProfileData()
        {
            var profile = await UserProfileService.LoadProfileAsync();

            if (profile == null || profile.HeightCm <= 0 || profile.WeightKg <= 0 || profile.Age <= 0)
            {
                RemainingCaloriesLabel.Text = "Please complete your profile first.";
                return;
            }

            double bmr = (profile.Gender == "Male")
                ? 10 * profile.TargetWeightKg + 6.25 * profile.HeightCm - 5 * profile.Age + 5
                : 10 * profile.TargetWeightKg + 6.25 * profile.HeightCm - 5 * profile.Age - 161;

            double multiplier = profile.ActivityLevel switch
            {
                "Sedentary" => 1.2,
                "Lightly Active" => 1.375,
                "Moderately Active" => 1.55,
                "Very Active" => 1.725,
                "Super Active" => 1.9,
                _ => 1.2
            };

            dailyCalorieNeed = bmr * multiplier;

            if (profile.TargetWeightKg > 0 && Math.Abs(profile.TargetWeightKg - profile.WeightKg) >= 1)
            {
                if (profile.TargetWeightKg > profile.WeightKg)
                    dailyCalorieNeed += 500;
                else if (profile.TargetWeightKg < profile.WeightKg)
                    dailyCalorieNeed -= 500;
            }

            string storedDate = Preferences.Get("CaloriesBurnedDate", "");
            if (storedDate != DateTime.Today.ToString("yyyy-MM-dd"))
            {
                Preferences.Set("CaloriesBurnedToday", 0.0);
                Preferences.Set("CaloriesBurnedDate", DateTime.Today.ToString("yyyy-MM-dd"));
            }

            double caloriesBurnedToday = Preferences.Get("CaloriesBurnedToday", 0.0);
            dailyCalorieNeed += caloriesBurnedToday;

            UpdateRemainingCalories();
        }

        private void LoadFoodList()
        {
            filteredFoods = FoodDatabase.Foods.ToList();
            FoodListView.ItemsSource = filteredFoods;
        }

        private void OnFoodSearchChanged(object sender, TextChangedEventArgs e)
        {
            string query = e.NewTextValue?.ToLower() ?? "";
            filteredFoods = FoodDatabase.Foods
                .Where(f => f.Name.ToLower().Contains(query))
                .ToList();
            FoodListView.ItemsSource = filteredFoods;
        }

        private void OnFoodSelected(object sender, SelectionChangedEventArgs e)
        {
            selectedFood = e.CurrentSelection.FirstOrDefault() as FoodItem;
        }
        private async void OnAddFoodClicked(object sender, EventArgs e)
        {
            var query = FoodSearchBar.Text ?? "";
            var results = FoodDatabase.SearchFoods(query);

            if (results.Any())
            {
                var baseItem = results.First();
                var eaten = new FoodItem
                {
                    Name = baseItem.Name,
                    CaloriesPer100g = baseItem.CaloriesPer100g,
                    ProteinPer100g = baseItem.ProteinPer100g,
                    GramsConsumed = 100
                };

                eatenFoods.Add(eaten);
                caloriesConsumed += eaten.TotalCalories;
                proteinConsumed += eaten.TotalProtein;

                EatenListView.ItemsSource = null;
                EatenListView.ItemsSource = eatenFoods;

                UpdateRemainingCalories();
                await SaveTodayLogAsync(); 

                await DisplayAlert("Added", $"{eaten.Name} - {eaten.TotalCalories:F0} kcal", "OK");
            }
            else
            {
                await DisplayAlert("Not Found", "Food not in database", "OK");
            }
        }
        private async void OnAddFoodToLogClicked(object sender, EventArgs e)
        {

            if (selectedFood == null)
            {
                await DisplayAlert("Error", "Please select a food first.", "OK");
                return;
            }

            if (!double.TryParse(GramsEntry.Text, out double grams) || grams <= 0)
            {
                await DisplayAlert("Error", "Enter grams consumed.", "OK");
                return;
            }

            var eaten = new FoodItem
            {
                Name = selectedFood.Name,
                CaloriesPer100g = selectedFood.CaloriesPer100g,
                ProteinPer100g = selectedFood.ProteinPer100g,
                GramsConsumed = grams
            };

            eatenFoods.Add(eaten);
            caloriesConsumed += eaten.TotalCalories;
            proteinConsumed += eaten.TotalProtein;
            EatenListView.ItemsSource = null;
            EatenListView.ItemsSource = eatenFoods;

            UpdateRemainingCalories();
            await SaveTodayLogAsync();
        }
        private async void OnDeleteFoodClicked(object sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.BindingContext is FoodItem foodToRemove)
            {
                eatenFoods.Remove(foodToRemove);

                caloriesConsumed -= foodToRemove.TotalCalories;
                proteinConsumed -= foodToRemove.TotalProtein;

                EatenListView.ItemsSource = null;
                EatenListView.ItemsSource = eatenFoods;
                UpdateRemainingCalories();
                await SaveTodayLogAsync();
            }
        }

        private async void OnEditFoodClicked(object sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.BindingContext is FoodItem foodToEdit)
            {
                string result = await DisplayPromptAsync(
                    "Edit Food",
                    $"Enter new grams for {foodToEdit.Name}:",
                    initialValue: foodToEdit.GramsConsumed.ToString(),
                    keyboard: Keyboard.Numeric);

                if (double.TryParse(result, out double newGrams) && newGrams > 0)
                {
                    caloriesConsumed -= foodToEdit.TotalCalories;
                    proteinConsumed -= foodToEdit.TotalProtein;

                    foodToEdit.GramsConsumed = newGrams;

                    caloriesConsumed += foodToEdit.TotalCalories;
                    proteinConsumed += foodToEdit.TotalProtein;

                    EatenListView.ItemsSource = null;
                    EatenListView.ItemsSource = eatenFoods;

                    UpdateRemainingCalories();
                    await SaveTodayLogAsync();
                }
                else
                {
                    await DisplayAlert("Error", "Invalid number of grams entered.", "OK");
                }
            }
        }

        private async void OnAddCustomFoodClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CustomFoodName.Text) ||
                !double.TryParse(CustomFoodCalories.Text, out double kcal) ||
                !double.TryParse(CustomFoodProtein.Text, out double protein))
            {
                await DisplayAlert("Error", "Enter valid name, kcal/100g, and protein/100g.", "OK");
                return;
            }

            FoodDatabase.AddFood(CustomFoodName.Text, kcal, protein);

            await FoodCsvService.AppendCustomAsync(new FoodItem
            {
                Name = CustomFoodName.Text.Trim(),
                CaloriesPer100g = kcal,
                ProteinPer100g = protein
            });

            CustomFoodName.Text = "";
            CustomFoodCalories.Text = "";
            CustomFoodProtein.Text = "";

            LoadFoodList();
        }


        private void UpdateRemainingCalories()
        {
            double caloriesRemaining =Math.Round(dailyCalorieNeed - caloriesConsumed);
            DailyGoalLabel.Text = $"Daily Goal: {dailyCalorieNeed:F0} kcal";
            CaloriesLabel.Text = $"Calories Consumed: {caloriesConsumed:F1} kcal";
            ProteinLabel.Text = $"Protein Consumed: {proteinConsumed:F1} g";
            RemainingCaloriesLabel.Text = $"Remaining: {caloriesRemaining:F0} kcal";
        }
        private async Task SaveTodayLogAsync()
        {
            var todayLog = new DailyLog
            {
                Date = DateTime.Today,
                Foods = eatenFoods,
                Calories = eatenFoods.Sum(f => f.TotalCalories),
                Protein = eatenFoods.Sum(f => f.TotalProtein),
                DailyGoal = dailyCalorieNeed
            };

            await FoodLogService.SaveLogAsync(todayLog);
        }
    }
}