using FitnessApp.Models;
using FitnessApp.Services;
using Microsoft.Maui.Storage;
using SkiaSharp.Views.Maui.Controls.Hosting;
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
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp();
            MainPage = new AppShell();
        }
    }
}