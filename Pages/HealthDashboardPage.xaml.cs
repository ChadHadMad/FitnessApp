using FitnessApp.Models;
using FitnessApp.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using System.Collections.ObjectModel;

namespace FitnessApp.Pages
{
    public partial class HealthDashboardPage : ContentPage
    {
        public ObservableCollection<ISeries> WeeklyCaloriesSeries { get; set; }
        public Axis[] XAxes { get; set; }

        public HealthDashboardPage()
        {
            InitializeComponent();

            WeeklyCaloriesSeries = new ObservableCollection<ISeries>();
            XAxes = new Axis[] { new Axis { Labels = new List<string>() } };

            WeeklyCaloriesChart.Series = WeeklyCaloriesSeries;
            WeeklyCaloriesChart.XAxes = XAxes;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await FoodLogService.EnsureDailyResetAsync();

            await LoadDashboardData();
            await LoadWeeklyCaloriesChart();
            await LoadMonthlyCaloriesChart();

            await RefreshCaloriesConsistencyPieAsync();
        }

        private async Task LoadDashboardData()
        {
            var profile = await UserProfileService.LoadProfileAsync();
            if (profile == null || profile.HeightCm <= 0 || profile.WeightKg <= 0 || profile.Age <= 0)
            {
                WeightLabel.Text = "Please complete your profile.";
                return;
            }

            HeightLabel.Text = $"{profile.HeightCm:F1} cm";
            WeightLabel.Text = $"{profile.WeightKg:F1} kg";
            WeightProgressBar.Progress = Math.Min(profile.WeightKg / 150.0, 1.0);

            double heightMeters = profile.HeightCm / 100.0;
            double bmi = profile.WeightKg / (heightMeters * heightMeters);
            BMILabel.Text = $"{bmi:F1}";
            BMIProgressBar.Progress = Math.Min(bmi / 40.0, 1.0);

            double bmr = (profile.Gender == "Male")
                ? 10 * profile.TargetWeightKg + 6.25 * profile.HeightCm - 5 * profile.Age + 5
                : 10 * profile.TargetWeightKg + 6.25 * profile.HeightCm - 5 * profile.Age - 161;

            double multiplier = profile.ActivityLevel switch
            {
                "Sedentary" => 1.2,
                "Lightly Active" => 1.375,
                "Moderately Active" => 1.55,
                "Very Active" => 1.725,
                "Super Active" => 1.9,
                _ => 1.2
            };

            double dailyCalories = bmr * multiplier;
            if (profile.TargetWeightKg > 0 && Math.Abs(profile.TargetWeightKg - profile.WeightKg) >= 1)
                dailyCalories += profile.TargetWeightKg > profile.WeightKg ? 500 : -500;

            CaloriesLabel.Text = $"{dailyCalories:F0} kcal";

            double proteinNeeded = profile.WeightKg * 1.6;
            ProteinLabel.Text = $"{proteinNeeded:F0} g";
        }

        private async Task LoadWeeklyCaloriesChart()
        {
            var logs = await FoodLogService.GetLast7DaysAsync();
            var values = logs.Select(l => (double)l.Foods.Sum(f => f.TotalCalories)).ToArray();
            var labels = logs.Select(l => l.Date.ToString("ddd")).ToArray();

            if (values.Length == 0) { values = new double[] { 0 }; labels = new[] { "—" }; }

            WeeklyCaloriesChart.Series = new ISeries[]
            {
            new ColumnSeries<double> { Values = values }
            };
            WeeklyCaloriesChart.XAxes = new[] { new Axis { Labels = labels } };
        }

        private async Task LoadMonthlyCaloriesChart()
        {
            var logs = await FoodLogService.GetLast30DaysAsync();
            var values = logs.Select(l => (double)l.Foods.Sum(f => f.TotalCalories)).ToArray();
            var labels = logs.Select(l => l.Date.ToString("dd MMM")).ToArray();

            if (values.Length == 0) { values = new double[] { 0 }; labels = new[] { "—" }; }

            MonthlyCaloriesChart.Series = new ISeries[]
            {
            new LineSeries<double> { Values = values }
            };
            MonthlyCaloriesChart.XAxes = new[] { new Axis { Labels = labels } };
        }

        private async Task RefreshCaloriesConsistencyPieAsync()
        {
            var stats = await StatisticsService.GetStatsAsync(days: 7); // or 30

            ConsistencyPieChart.Series = new ISeries[]
            {
            new PieSeries<double>
            {
                Values = new[] { stats.ConsistencyPercent },
                Name = "On Track",
                DataLabelsSize = 14,
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle
            },
            new PieSeries<double>
            {
                Values = new[] { 100 - stats.ConsistencyPercent },
                Name = "Missed",
                DataLabelsSize = 14,
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle
            }
            };
        }
        private async void OnEditProfileClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("UserProfilePage");
    }
}