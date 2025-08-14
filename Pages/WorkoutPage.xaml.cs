using Microsoft.Maui.Storage;
using System.Text.Json;
using FitnessApp.Data;
using FitnessApp.Models;

namespace FitnessApp.Pages
{
    public partial class WorkoutPage : ContentPage
    {
        private List<WorkoutItem> recommendedWorkouts = new();
        private List<WorkoutItem> loggedWorkouts = new();
        private double caloriesBurnedToday;

        public WorkoutPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadRecommendedWorkouts();
            ResetIfNewDay();
            LoadWorkoutLog();
            UpdateCaloriesBurnedLabel();
        }

        private void LoadRecommendedWorkouts()
        {
            recommendedWorkouts = WorkoutDatabase.GetWorkouts();
            WorkoutListView.ItemsSource = recommendedWorkouts;
        }

        private void ResetIfNewDay()
        {
            string lastDate = Preferences.Get("LastWorkoutDate", "");
            string today = DateTime.Today.ToString("yyyy-MM-dd");

            if (lastDate != today)
            {
                caloriesBurnedToday = 0;
                loggedWorkouts.Clear();
                Preferences.Set("CaloriesBurnedToday", 0.0);
                SaveWorkoutLog();
                Preferences.Set("LastWorkoutDate", today);
            }
            else
            {
                caloriesBurnedToday = Preferences.Get("CaloriesBurnedToday", 0.0);
            }
        }

        private void SaveWorkoutLog()
        {
            var workoutLog = new WorkoutLogData
            {
                Date = DateTime.Today.ToString("yyyy-MM-dd"),
                Workouts = loggedWorkouts
            };

            string json = JsonSerializer.Serialize(workoutLog);
            Preferences.Set("WorkoutLog", json);
        }

        private void LoadWorkoutLog()
        {
            string json = Preferences.Get("WorkoutLog", "");
            if (!string.IsNullOrEmpty(json))
            {
                var workoutLog = JsonSerializer.Deserialize<WorkoutLogData>(json);

                if (workoutLog != null && workoutLog.Date == DateTime.Today.ToString("yyyy-MM-dd"))
                {
                    loggedWorkouts = workoutLog.Workouts;
                }
                else
                {
                    loggedWorkouts = new List<WorkoutItem>();
                    SaveWorkoutLog();
                }
            }
            else
            {
                loggedWorkouts = new List<WorkoutItem>();
            }

            LoggedWorkoutsListView.ItemsSource = null;
            LoggedWorkoutsListView.ItemsSource = loggedWorkouts;
        }

        private void OnWorkoutSearchChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue?.ToLower() ?? "";
            WorkoutListView.ItemsSource = string.IsNullOrWhiteSpace(searchText)
                ? recommendedWorkouts
                : recommendedWorkouts.Where(w => w.Name.ToLower().Contains(searchText)).ToList();
        }

        private void OnWorkoutSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is WorkoutItem selectedWorkout)
            {
                AddWorkoutToLog(selectedWorkout);
                WorkoutListView.SelectedItem = null;
            }
        }

        private void OnAddCustomWorkoutClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CustomWorkoutName.Text) ||
                !double.TryParse(CustomWorkoutCalories.Text, out double calories))
            {
                DisplayAlert("Error", "Please enter valid workout name and calories.", "OK");
                return;
            }

            var workout = new WorkoutItem
            {
                Name = CustomWorkoutName.Text,
                CaloriesBurned = calories
            };

            AddWorkoutToLog(workout);

            CustomWorkoutName.Text = "";
            CustomWorkoutCalories.Text = "";
        }

        private void AddWorkoutToLog(WorkoutItem workout)
        {
            loggedWorkouts.Add(workout);
            caloriesBurnedToday += workout.CaloriesBurned;

            Preferences.Set("CaloriesBurnedToday", caloriesBurnedToday);
            SaveWorkoutLog();

            LoggedWorkoutsListView.ItemsSource = null;
            LoggedWorkoutsListView.ItemsSource = loggedWorkouts;
            UpdateCaloriesBurnedLabel();
        }

        private void UpdateCaloriesBurnedLabel()
        {
            CaloriesBurnedLabel.Text = $"Calories Burned Today: {caloriesBurnedToday:F1} kcal";
        }
    }
}