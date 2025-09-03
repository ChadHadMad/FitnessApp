using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitnessApp.Models;

namespace FitnessApp.Services
{
    public static class StatisticsService
    {
        public static async Task<StatsResult> GetStatsAsync(int days = 7)
        {
            var allLogs = await FoodLogService.LoadLogAsync();

            if (days <= 0 || allLogs.Count == 0)
                return new StatsResult();

            var start = DateTime.Today.AddDays(-(days - 1)).Date;

            var recentLogs = allLogs
                .Where(l => l.Date.Date >= start && l.Date.Date <= DateTime.Today)
                .OrderBy(l => l.Date)
                .ToList();

            if (recentLogs.Count == 0)
                return new StatsResult();

            var calories = recentLogs.Select(l => l.Calories).ToList();
            var goals = recentLogs.Select(l => l.DailyGoal).ToList();

            double avgCalories = calories.Average();
            double avgGoal = goals.Where(g => g > 0).DefaultIfEmpty(0).Average();

            int onTrackDays = recentLogs.Count(l =>
                l.DailyGoal > 0 &&
                Math.Abs(l.Calories - l.DailyGoal) / l.DailyGoal <= 0.05
            );

            double consistency = 100.0 * onTrackDays / recentLogs.Count;

            return new StatsResult
            {
                AvgCalories = Math.Round(avgCalories, 1),
                AvgGoal = Math.Round(avgGoal, 1),
                ConsistencyPercent = Math.Round(consistency, 1),
                Logs = recentLogs
            };
        }
    }

    public class StatsResult
    {
        public double AvgCalories { get; set; }
        public double AvgGoal { get; set; }
        public double ConsistencyPercent { get; set; }
        public List<DailyLog> Logs { get; set; } = new();
    }
}