namespace FitnessApp.Pages
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private async void OnLogWorkoutClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("UserProfilePage");
        }
    }
}