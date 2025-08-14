using System;
using Microsoft.Maui.Controls;

namespace FitnessApp.Pages
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();

            // Load data - placeholder
            //LoadWorkoutSummary();
        }

        /*private void LoadWorkoutSummary()
        {
            // You can later replace this with data from a database or service
            WorkoutSummaryLabel.Text = "30 min cardio, 15 push-ups, 10 squats.";
        }*/

        private async void OnLogWorkoutClicked(object sender, EventArgs e)
        {
            // Navigate to the Log page
            await Shell.Current.GoToAsync("//User");
        }
    }
}