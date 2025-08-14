using Microsoft.Maui.Storage; 

namespace FitnessApp.Pages
{
    public partial class UserProfilePage : ContentPage
    {
        public UserProfilePage()
        {
            InitializeComponent();
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


            Preferences.Set("TargetWeightKg", targetWeightKg);

            string gender = GenderPicker.SelectedItem.ToString() ?? "";
            string activityLevel = ActivityLevelPicker.SelectedItem.ToString() ?? "";

            double heightMeters = heightCm / 100.0;
            double heightInches = heightCm / 2.54;

            double bmi = weightKg / (heightMeters * heightMeters);

            double idealWeightKg;
            if (gender == "Male")
                idealWeightKg = 50 + 2.3 * (heightInches - 60);
            else
                idealWeightKg = 45.5 + 2.3 * (heightInches - 60);

            IdealWeightLabel.Text = $"Ideal Weight: {idealWeightKg:F1} kg";
            BmiLabel.Text = $"BMI: {bmi:F1}";

            Preferences.Set("Gender", gender);
            Preferences.Set("HeightCm", heightCm);
            Preferences.Set("WeightKg", weightKg);
            Preferences.Set("TargetWeightKg", targetWeightKg);
            Preferences.Set("Age", age);
            Preferences.Set("ActivityLevel", activityLevel);

            await DisplayAlert(
                "Profile Saved",
                $"Ideal Weight: {idealWeightKg:F1} kg\nBMI: {bmi:F1}\nTarget Weight: {targetWeightKg:F1} kg",
                "OK"
            );
            }
        }
    }