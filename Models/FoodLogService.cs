using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FitnessApp.Models;
using Microsoft.Maui.Storage;
using FitnessApp.Services;

namespace FitnessApp.Services
{
    public static class FoodLogService
    {
        private const string LogsKey = "DailyLogs";

        public static event Action? LogChanged;

        public static Task<List<DailyLog>> LoadLogAsync()
        {
            if (!Preferences.ContainsKey(LogsKey))
                return Task.FromResult(new List<DailyLog>());

            var json = Preferences.Get(LogsKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
                return Task.FromResult(new List<DailyLog>());

            return Task.FromResult(JsonSerializer.Deserialize<List<DailyLog>>(json) ?? new List<DailyLog>());
        }

        public static async Task SaveLogAsync(DailyLog logForDay)
        {
            var logs = await LoadLogAsync();
            logs.RemoveAll(l => l.Date.Date == logForDay.Date.Date);
            logs.Add(logForDay);
            Preferences.Set(LogsKey, JsonSerializer.Serialize(logs));

            var cutoff = DateTime.Today.AddDays(-60);
            logs = logs.Where(l => l.Date.Date >= cutoff).OrderBy(l => l.Date).ToList();

            Preferences.Set(LogsKey, JsonSerializer.Serialize(logs)); 

            LogChanged?.Invoke();
        }

        public static async Task<DailyLog> GetTodayAsync()
        {
            var logs = await LoadLogAsync();
            var today = logs.FirstOrDefault(l => l.Date.Date == DateTime.Today);
            if (today == null)
            {
                today = new DailyLog { Date = DateTime.Today, Foods = new List<FoodItem>(), Calories = 0, Protein = 0 };
                logs.Add(today);
                Preferences.Set(LogsKey, JsonSerializer.Serialize(logs));

                LogChanged?.Invoke();
            }
            return today;
        }

        public static async Task EnsureDailyResetAsync() => _ = await GetTodayAsync();

        public static async Task<List<DailyLog>> GetLast7DaysAsync()
        {
            var logs = await LoadLogAsync();
            var start = DateTime.Today.AddDays(-6);
            return logs.Where(l => l.Date.Date >= start).OrderBy(l => l.Date).ToList();
        }

        public static async Task<List<DailyLog>> GetLast30DaysAsync()
        {
            var logs = await LoadLogAsync();
            var start = DateTime.Today.AddDays(-29);
            return logs.Where(l => l.Date.Date >= start).OrderBy(l => l.Date).ToList();
        }
    }
}