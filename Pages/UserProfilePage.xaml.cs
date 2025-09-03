using FitnessApp.Services;
using Microsoft.Maui.Storage;
using FitnessApp.Models;
namespace FitnessApp.Pages
{
    public partial class UserProfilePage : ContentPage
    {
        public UserProfilePage()
        {
            InitializeComponent();
            LoadProfileData(); 
        }

        private void LoadProfileData()
        {
            if (Preferences.ContainsKey("Gender"))
                GenderPicker.SelectedItem = Preferences.Get("Gender", "");

            if (Preferences.ContainsKey("HeightCm"))
                HeightEntry.Text = Preferences.Get("HeightCm", 0.0).ToString();

            if (Preferences.ContainsKey("Age"))
                AgeEntry.Text = Preferences.Get("Age", "Pick");

            if (Preferences.ContainsKey("WeightKg"))
                WeightEntry.Text = Preferences.Get("WeightKg", 0.0).ToString();

            if (Preferences.ContainsKey("TargetWeightKg"))
                TargetWeightEntry.Text = Preferences.Get("TargetWeightKg", 0.0).ToString();

            if (Preferences.ContainsKey("ActivityLevel"))
                ActivityLevelPicker.SelectedItem = Preferences.Get("ActivityLevel", "");

            double heightCm = Preferences.Get("HeightCm", 0.0);
            double weightKg = Preferences.Get("WeightKg", 0.0);
            string gender = Preferences.Get("Gender", "");

            if (heightCm > 0 && weightKg > 0 && !string.IsNullOrEmpty(gender))
            {
                double heightMeters = heightCm / 100.0;
                double heightInches = heightCm / 2.54;
                double bmi = weightKg / (heightMeters * heightMeters);

                double idealWeightKg = gender == "Male"
                    ? 50 + 2.3 * (heightInches - 60)
                    : 45.5 + 2.3 * (heightInches - 60);

                IdealWeightLabel.Text = $"Ideal Weight: {idealWeightKg:F1} kg";
                BmiLabel.Text = $"BMI: {bmi:F1}";
            }
        }

        private async void OnSaveProfileClicked(object sender, EventArgs e)
        {
            if (GenderPicker.SelectedItem == null ||
                !double.TryParse(HeightEntry.Text, out double heightCm) ||
                !int.TryParse(AgeEntry.Text, out int age) ||
                ActivityLevelPicker.SelectedItem == null ||
                !double.TryParse(WeightEntry.Text ?? "", out double weightKg) ||
                !double.TryParse(TargetWeightEntry.Text ?? "", out double targetWeightKg))
            {
                await DisplayAlert("Error", "Please fill in all fields correctly.", "OK");
                return;
            }

            var profile = new UserProfile
            {
                Gender = GenderPicker.SelectedItem.ToString() ?? "",
                HeightCm = heightCm,
                WeightKg = weightKg,
                TargetWeightKg = targetWeightKg,
                Age = age,
                ActivityLevel = ActivityLevelPicker.SelectedItem.ToString() ?? "",
                HasCompletedOnboarding = true 
            };

            await UserProfileService.SaveProfileAsync(profile);
            await DisplayAlert("Saved", "Your profile has been saved.", "OK");

            await Shell.Current.GoToAsync("..");

            if (Application.Current?.MainPage is AppShell shell)
            {
                shell.NavigateToMainApp();
            }
            else
            {
                await Shell.Current.GoToAsync("//HealthDashboardPage");
            }
        }
    }
}
