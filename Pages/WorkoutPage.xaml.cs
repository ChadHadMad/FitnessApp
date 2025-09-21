using Microsoft.Maui.Storage;
using System.Text.Json;
using FitnessApp.Data;
using FitnessApp.Models;
using FitnessApp.Services;

namespace FitnessApp.Pages
{
    public partial class WorkoutPage : ContentPage
    {
        private List<WorkoutItem> recommendedWorkouts = new();
        private List<LoggedWorkout> loggedWorkouts = new();
        private WorkoutLogData todayLog = new();

        public WorkoutPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            LoadRecommendedWorkouts();                     

            await WorkoutLogService.EnsureDailyResetAsync();

            var today = await WorkoutLogService.GetTodayAsync();
            todayLog = today;
            loggedWorkouts = today.Workouts;

            LoggedWorkoutsListView.ItemsSource = null;
            LoggedWorkoutsListView.ItemsSource = loggedWorkouts;

            RefreshLoggedListAndTotals();                  
        }

        private void LoadRecommendedWorkouts()
        {
            recommendedWorkouts = WorkoutDatabase.GetWorkouts();
            WorkoutListView.ItemsSource = recommendedWorkouts;
        }

        private void OnWorkoutSearchChanged(object sender, TextChangedEventArgs e)
        {
            var q = e.NewTextValue?.ToLower() ?? "";
            WorkoutListView.ItemsSource =
                string.IsNullOrWhiteSpace(q)
                    ? recommendedWorkouts
                    : recommendedWorkouts.Where(w => w.Name.ToLower().Contains(q)).ToList();
        }

        private async void OnWorkoutSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not WorkoutItem selected) return;
            WorkoutListView.SelectedItem = null;

            var minutesText = await DisplayPromptAsync("Duration", $"Minutes for {selected.Name}:", keyboard: Keyboard.Numeric);
            if (!double.TryParse(minutesText, out double minutes) || minutes <= 0) return;

            var entry = new LoggedWorkout
            {
                Name = selected.Name,
                CaloriesPerMinute = selected.CaloriesPerMinute,
                DurationMinutes = minutes,
                Date = DateTime.Today
            };

            await WorkoutLogService.AddLoggedWorkoutAsync(DateTime.Today.ToString("yyyy-MM-dd"), entry);
            await WorkoutCsvService.AppendAsync(entry); 

            todayLog.Workouts.Add(entry);
            RefreshLoggedListAndTotals();
        }

        private async void OnAddCustomWorkoutClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CustomWorkoutName.Text)
                || !double.TryParse(CustomWorkoutKcalPerMin.Text, out double cpm)
                || cpm <= 0)
            {
                await DisplayAlert("Error", "Enter a name and a valid 'calories per minute'.", "OK");
                return;
            }

            var minutesText = await DisplayPromptAsync("Duration",
                $"Minutes for {CustomWorkoutName.Text}:",
                keyboard: Keyboard.Numeric);

            if (!double.TryParse(minutesText, out double minutes) || minutes <= 0) return;

            var entry = new LoggedWorkout
            {
                Name = CustomWorkoutName.Text.Trim(),
                CaloriesPerMinute = cpm,
                DurationMinutes = minutes,
                Date = DateTime.Today
            };

            await WorkoutLogService.AddLoggedWorkoutAsync(DateTime.Today.ToString("yyyy-MM-dd"), entry);
            await WorkoutCsvService.AppendAsync(entry); 

            if (!recommendedWorkouts.Any(w => w.Name.Equals(entry.Name, StringComparison.OrdinalIgnoreCase)))
                recommendedWorkouts.Add(new WorkoutItem { Name = entry.Name, CaloriesPerMinute = cpm });

            CustomWorkoutName.Text = "";
            CustomWorkoutKcalPerMin.Text = "";

            RefreshLoggedListAndTotals();
        }

        private async void OnEditLoggedWorkout(object sender, EventArgs e)
        {
            if (sender is not SwipeItem swipe || swipe.BindingContext is not LoggedWorkout entry) return;

            var newMinutesText = await DisplayPromptAsync("Edit Duration",
                $"Minutes for {entry.Name}:",
                initialValue: entry.DurationMinutes.ToString(),
                keyboard: Keyboard.Numeric);

            if (!double.TryParse(newMinutesText, out double newMinutes) || newMinutes <= 0) return;

            entry.DurationMinutes = newMinutes;

            await WorkoutLogService.UpdateLoggedWorkoutAsync(DateTime.Today.ToString("yyyy-MM-dd"), entry);

            await WorkoutCsvService.RewriteDayAsync(DateTime.Today, todayLog.Workouts);

            RefreshLoggedListAndTotals();
        }

        private async void OnDeleteLoggedWorkout(object sender, EventArgs e)
        {
            if (sender is not SwipeItem swipe || swipe.BindingContext is not LoggedWorkout entry) return;

            bool ok = await DisplayAlert("Delete", $"Delete {entry.Name}?", "Yes", "No");
            if (!ok) return;

            await WorkoutLogService.DeleteLoggedWorkoutAsync(DateTime.Today.ToString("yyyy-MM-dd"), entry.Id);

            todayLog.Workouts.RemoveAll(w => w.Id == entry.Id);

            await WorkoutCsvService.RewriteDayAsync(DateTime.Today, todayLog.Workouts);

            RefreshLoggedListAndTotals();
        }

        private void RefreshLoggedListAndTotals()
        {
            LoggedWorkoutsListView.ItemsSource = null;
            LoggedWorkoutsListView.ItemsSource = todayLog.Workouts;

            var total = todayLog.Workouts.Sum(w => w.CaloriesPerMinute * w.DurationMinutes);

            Preferences.Set("CaloriesBurnedToday", total);

            CaloriesBurnedLabel.Text = $"Calories Burned Today: {total:F1} kcal";
        }

        private void UpdateCaloriesBurnedLabel()
        {
            CaloriesBurnedLabel.Text = $"Calories Burned Today: {todayLog.TotalCalories:F1} kcal";
        }
    }
}