using FitnessApp.Services;
using FitnessApp.Models;
//using FitnessApp.Services;
using System.Reflection;

namespace FitnessApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            Task.Run(async () =>
            {
                await FoodDatabase.InitializeAsync();
            });
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}