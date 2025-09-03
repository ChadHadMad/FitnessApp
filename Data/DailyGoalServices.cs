using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using FitnessApp.Models;

namespace FitnessApp.Services
{
    public static class DailyGoalService
    {
        public static async Task UpsertTodayGoalFromProfileAsync()
        {
            var profile = await UserProfileService.LoadProfileAsync();
            if (profile == null) return;

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

            double dailyGoal = bmr * multiplier;

            if (profile.TargetWeightKg > 0 && Math.Abs(profile.TargetWeightKg - profile.WeightKg) >= 1)
                dailyGoal += profile.TargetWeightKg > profile.WeightKg ? 500 : -500;

            double burnedToday = Preferences.Get("CaloriesBurnedToday", 0.0);
            dailyGoal += burnedToday;

            var today = await FoodLogService.GetTodayAsync();
            today.DailyGoal = dailyGoal;

            today.Calories = today.Foods.Sum(f => f.TotalCalories);
            today.Protein = today.Foods.Sum(f => f.TotalProtein);

            await FoodLogService.SaveLogAsync(today);
        }
    }
}