using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitnessApp.Models;
using Microsoft.Maui.Storage;
using System.Globalization;

namespace FitnessApp.Services
{
    public static class WorkoutCsvService
    {
        private static readonly string FilePath =
            Path.Combine(FileSystem.AppDataDirectory, "workouts_log.csv");

        private static readonly string Header =
            "Date,Name,DurationMinutes,CaloriesPerMinute,CaloriesBurned";

    
        public static async Task AppendAsync(Models.LoggedWorkout workout)
        {
            await EnsureHeaderAsync();

            var line = string.Join(",",
                workout.Date.ToString("yyyy-MM-dd"),
                CsvEscape(workout.Name),
                workout.DurationMinutes.ToString(CultureInfo.InvariantCulture),
                workout.CaloriesPerMinute.ToString(CultureInfo.InvariantCulture),
                workout.CaloriesBurned.ToString(CultureInfo.InvariantCulture));

            await File.AppendAllTextAsync(FilePath, line + Environment.NewLine, Encoding.UTF8);
        }

 
        public static async Task RewriteDayAsync(DateTime date, IEnumerable<Models.LoggedWorkout> dayEntries)
        {
            await EnsureHeaderAsync();

            var allLines = (await File.ReadAllLinesAsync(FilePath, Encoding.UTF8)).ToList();
            if (allLines.Count == 0) allLines.Add(Header);

            var datePrefix = date.ToString("yyyy-MM-dd") + ",";
            var kept = new List<string> { allLines[0] };
            kept.AddRange(allLines.Skip(1).Where(l => !l.StartsWith(datePrefix, StringComparison.Ordinal)));

            foreach (var w in dayEntries)
            {
                var line = string.Join(",",
                    w.Date.ToString("yyyy-MM-dd"),
                    CsvEscape(w.Name),
                    w.DurationMinutes.ToString(CultureInfo.InvariantCulture),
                    w.CaloriesPerMinute.ToString(CultureInfo.InvariantCulture),
                    w.CaloriesBurned.ToString(CultureInfo.InvariantCulture));
                kept.Add(line);
            }

            await File.WriteAllLinesAsync(FilePath, kept, Encoding.UTF8);
        }

        private static async Task EnsureHeaderAsync()
        {
            if (!File.Exists(FilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                await File.WriteAllTextAsync(FilePath, Header + Environment.NewLine, Encoding.UTF8);
            }
            else
            {
                var first = (await File.ReadAllLinesAsync(FilePath, Encoding.UTF8)).FirstOrDefault();
                if (first == null || !first.StartsWith("Date,"))
                {
                    await File.WriteAllTextAsync(FilePath, Header + Environment.NewLine, Encoding.UTF8);
                }
            }
        }

        private static string CsvEscape(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }
        public static async Task<List<WorkoutCsvRow>> ReadAllAsync()
        {
            var path = FilePath;
            var rows = new List<WorkoutCsvRow>();
            if (!File.Exists(path)) return rows;

            var lines = await File.ReadAllLinesAsync(path, Encoding.UTF8);
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = SplitCsv(line);
                if (parts.Length < 5) continue;

                if (!DateTime.TryParse(parts[0], out var date)) continue;
                var name = parts[1].Trim();
                _ = double.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var minutes);
                _ = double.TryParse(parts[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var cpm);
                _ = double.TryParse(parts[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var kcal);

                rows.Add(new WorkoutCsvRow
                {
                    Date = date,
                    Name = name,
                    DurationMinutes = minutes,
                    CaloriesPerMinute = cpm,
                    CaloriesBurned = kcal
                });
            }
            return rows;
        }

        public static async Task<List<WorkoutDailyTotal>> GetDailyTotalsAsync(DateTime start, DateTime end)
        {
            var all = await ReadAllAsync();
            return all
                .Where(r => r.Date.Date >= start.Date && r.Date.Date <= end.Date)
                .GroupBy(r => r.Date.Date)
                .OrderBy(g => g.Key)
                .Select(g => new WorkoutDailyTotal
                {
                    Date = g.Key,
                    CaloriesBurned = g.Sum(x => x.CaloriesBurned)
                })
                .ToList();
        }


        private static string[] SplitCsv(string line)
        {
            var inQuotes = false;
            var cur = new StringBuilder();
            var list = new List<string>();
            foreach (var ch in line)
            {
                if (ch == '"') { inQuotes = !inQuotes; continue; }
                if (ch == ',' && !inQuotes) { list.Add(cur.ToString()); cur.Clear(); }
                else cur.Append(ch);
            }
            list.Add(cur.ToString());
            return list.ToArray();
        }

        public class WorkoutCsvRow
        {
            public DateTime Date { get; set; }
            public string Name { get; set; } = "";
            public double DurationMinutes { get; set; }
            public double CaloriesPerMinute { get; set; }
            public double CaloriesBurned { get; set; }
        }

        public class WorkoutDailyTotal
        {
            public DateTime Date { get; set; }
            public double CaloriesBurned { get; set; }
        }
    }
}