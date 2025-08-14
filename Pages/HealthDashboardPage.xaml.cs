using Microsoft.Maui.Storage;

namespace FitnessApp.Pages
{
    public partial class HealthDashboardPage : ContentPage
    {
        public HealthDashboardPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            string gender = Preferences.Get("Gender", "");
            double heightCm = Preferences.Get("HeightCm", 0.0);
            double weightKg = Preferences.Get("WeightKg", 0.0);
            int age = Preferences.Get("Age", 0);
            string activityLevel = Preferences.Get("ActivityLevel", "");

            if (heightCm <= 0 || weightKg <= 0 || age <= 0)
            {
                WeightLabel.Text = "Please complete your profile.";
                return;
            }

            // Height
            HeightLabel.Text = $"{heightCm:F1} cm";

            // Weight
            WeightLabel.Text = $"{weightKg:F1} kg";
            WeightProgressBar.Progress = Math.Min(weightKg / 150.0, 1.0); 

            // BMI
            double heightMeters = heightCm / 100.0;
            double bmi = weightKg / (heightMeters * heightMeters);
            BMILabel.Text = $"{bmi:F1}";
            BMIProgressBar.Progress = Math.Min(bmi / 40.0, 1.0); 

            // Calories
            double bmr = (gender == "Male")
                ? 10 * weightKg + 6.25 * heightCm - 5 * age + 5
                : 10 * weightKg + 6.25 * heightCm - 5 * age - 161;

            double multiplier = activityLevel switch
            {
                "Sedentary" => 1.2,
                "Lightly Active" => 1.375,
                "Moderately Active" => 1.55,
                "Very Active" => 1.725,
                "Super Active" => 1.9,
                _ => 1.2
            };

            double dailyCalories = bmr * multiplier;
            CaloriesLabel.Text = $"{dailyCalories:F0} kcal";

            double proteinNeeded = weightKg * 1.6;
            ProteinLabel.Text = $"{proteinNeeded:F0} g";
        }
    }
}