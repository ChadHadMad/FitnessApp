using FitnessApp.Pages;
using Microsoft.Maui.Storage;

namespace FitnessApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();

            if (Preferences.Get("HasCompletedOnboarding", false))
            {
                NavigateToMainApp();
            }
            else
            {
                NavigateToHome();
            }
        }

        private void RegisterRoutes()
        {
            Routing.RegisterRoute("UserProfilePage", typeof(UserProfilePage));

            Routing.RegisterRoute("HealthDashboardPage", typeof(HealthDashboardPage));
            Routing.RegisterRoute("CalorieIntakePage", typeof(CalorieIntakePage));
            Routing.RegisterRoute("WorkoutPage", typeof(WorkoutPage));
            Routing.RegisterRoute("HomePage", typeof(HomePage));
        }

        public void NavigateToMainApp()
        {
            Dispatcher.Dispatch(() =>
            {
                Preferences.Set("HasCompletedOnboarding", true);
                MainTabs.IsVisible = true;
                HomePageItem.IsVisible = false;
                CurrentItem = MainTabs;
            });
        }

        public void NavigateToHome()
        {
            Dispatcher.Dispatch(() =>
            {
                MainTabs.IsVisible = false;
                HomePageItem.IsVisible = true;
                CurrentItem = HomePageItem;
            });
        }

    }
}