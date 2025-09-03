using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitnessApp.Models;
using Microsoft.Maui.Storage;

using System.Text.Json;
using Microsoft.Maui.Storage;

namespace FitnessApp.Services
{
    public static class WorkoutLogService
    {
        private const string LogsKey = "WorkoutLogs";
        private const string LastDateKey = "LastWorkoutDate";
        private const string BurnedKey = "CaloriesBurnedToday";

        public static event Action? LogChanged;

        public static async Task EnsureDailyResetAsync()
        {
            var todayStr = DateTime.Today.ToString("yyyy-MM-dd");
            var last = Preferences.Get(LastDateKey, "");

            if (last == todayStr) return;

            var logs = await LoadAllAsync();

            var today = logs.FirstOrDefault(l => l.Date == todayStr);
            if (today == null)
            {
                today = new WorkoutLogData
                {
                    Date = todayStr,
                    Workouts = new List<LoggedWorkout>()
                };
                logs.Add(today);
            }
            else
            {
                today.Workouts.Clear();
            }

            await SaveAllAsync(logs);

            Preferences.Set(LastDateKey, todayStr);
            Preferences.Set(BurnedKey, 0.0);

            LogChanged?.Invoke();
        }

        public static async Task<WorkoutLogData> GetTodayAsync()
        {
            var todayStr = DateTime.Today.ToString("yyyy-MM-dd");
            var logs = await LoadAllAsync();

            var today = logs.FirstOrDefault(l => l.Date == todayStr);
            if (today == null)
            {
                today = new WorkoutLogData
                {
                    Date = todayStr,
                    Workouts = new List<LoggedWorkout>()
                };
                logs.Add(today);
                await SaveAllAsync(logs);
                Preferences.Set(LastDateKey, todayStr);
                Preferences.Set(BurnedKey, 0.0);
            }

            return today;
        }

        public static async Task AddLoggedWorkoutAsync(string dateStr, LoggedWorkout entry)
        {
            var logs = await LoadAllAsync();
            var day = logs.FirstOrDefault(l => l.Date == dateStr)
                      ?? new WorkoutLogData { Date = dateStr, Workouts = new List<LoggedWorkout>() };

            if (!logs.Contains(day)) logs.Add(day);

            day.Workouts.Add(entry);

            await SaveAllAsync(logs);
            Preferences.Set(BurnedKey, day.TotalCalories);
            LogChanged?.Invoke();
        }

        public static async Task UpdateLoggedWorkoutAsync(string dateStr, LoggedWorkout entry)
        {
            var logs = await LoadAllAsync();
            var day = logs.FirstOrDefault(l => l.Date == dateStr);
            if (day == null) return;

            var idx = day.Workouts.FindIndex(w => w.Id == entry.Id);
            if (idx >= 0) day.Workouts[idx] = entry;

            await SaveAllAsync(logs);
            Preferences.Set(BurnedKey, day.TotalCalories);
            LogChanged?.Invoke();
        }

        public static async Task DeleteLoggedWorkoutAsync(string dateStr, Guid entryId)
        {
            var logs = await LoadAllAsync();
            var day = logs.FirstOrDefault(l => l.Date == dateStr);
            if (day == null) return;

            day.Workouts.RemoveAll(w => w.Id == entryId);

            await SaveAllAsync(logs);
            Preferences.Set(BurnedKey, day.TotalCalories);
            LogChanged?.Invoke();
        }

        private static Task<List<WorkoutLogData>> LoadAllAsync()
        {
            var json = Preferences.Get(LogsKey, "");
            if (string.IsNullOrWhiteSpace(json))
                return Task.FromResult(new List<WorkoutLogData>());

            return Task.FromResult(
                JsonSerializer.Deserialize<List<WorkoutLogData>>(json) ?? new List<WorkoutLogData>());
        }

        private static Task SaveAllAsync(List<WorkoutLogData> logs)
        {
            var json = JsonSerializer.Serialize(logs);
            Preferences.Set(LogsKey, json);
            return Task.CompletedTask;
        }
    }
}