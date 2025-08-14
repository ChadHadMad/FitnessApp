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

        public CalorieIntakePage()
        {
            InitializeComponent();
            LoadProfileData();
            LoadFoodList();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadProfileData();
        }

        private void LoadProfileData()
        {
            string gender = Preferences.Get("Gender", "");
            double heightCm = Preferences.Get("HeightCm", 0.0);
            double weightKg = Preferences.Get("WeightKg", 0.0);
            double targetWeightKg = Preferences.Get("TargetWeightKg", 0.0);
            int age = Preferences.Get("Age", 0);
            string activityLevel = Preferences.Get("ActivityLevel", "");

            if (string.IsNullOrEmpty(gender) || heightCm <= 0 || weightKg <= 0 || age <= 0)
            {
                RemainingCaloriesLabel.Text = "Please complete your profile first.";
                return;
            }

  
            double bmr = (gender == "Male")
                ? 10 * targetWeightKg + 6.25 * heightCm - 5 * age + 5
                : 10 * targetWeightKg + 6.25 * heightCm - 5 * age - 161;

         
            double multiplier = activityLevel switch
            {
                "Sedentary" => 1.2,
                "Lightly Active" => 1.375,
                "Moderately Active" => 1.55,
                "Very Active" => 1.725,
                "Super Active" => 1.9,
                _ => 1.2
            };

            dailyCalorieNeed = bmr * multiplier;

           
            if (targetWeightKg > 0 && Math.Abs(targetWeightKg - weightKg) >= 1)
            {
                if (targetWeightKg > weightKg)
                    dailyCalorieNeed += 500; 
                else if (targetWeightKg < weightKg)
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
            string query = "";
            if (e.NewTextValue != null)
            {
                query = e.NewTextValue.ToLower();
            }

            filteredFoods = FoodDatabase.Foods
                .Where(f => f.Name.ToLower().Contains(query))
                .ToList();
            FoodListView.ItemsSource = filteredFoods;
        }

        private void OnFoodSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is FoodItem selectedFood)
            {
                eatenFoods.Add(selectedFood);
               
                caloriesConsumed += selectedFood.Calories;

                if (EatenListView != null)
                {
                    EatenListView.ItemsSource = null;
                    EatenListView.ItemsSource = eatenFoods;
                }
                UpdateRemainingCalories();
            }
        }

        private async void OnAddCustomFoodClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CustomFoodName.Text) ||
                !double.TryParse(CustomFoodCalories.Text, out double calories))
            {
                await DisplayAlert("Error", "Please enter valid food name and calories.", "OK");
                return;
            }

          
            FoodDatabase.AddFood(CustomFoodName.Text, calories);

           
            CustomFoodName.Text = "";
            CustomFoodCalories.Text = "";

            LoadFoodList();
        }

        private void UpdateRemainingCalories()
        {
            double caloriesRemaining = dailyCalorieNeed - caloriesConsumed;
            DailyGoalLabel.Text = $"Daily Goal: {dailyCalorieNeed:F0} kcal";
            CaloriesLabel.Text = $"Calories Consumed: {caloriesConsumed:F1} kcal";
            RemainingCaloriesLabel.Text = $"Remaining: {caloriesRemaining:F1} kcal";
        }
    }
}