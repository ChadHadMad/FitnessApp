using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using FitnessApp.Models;
using Microsoft.Maui.Storage;

namespace FitnessApp.Services
{
    public static class FoodCsvService
    {
        private static readonly string FilePath =
            Path.Combine(FileSystem.AppDataDirectory, "custom_foods.csv");

        public static async Task AppendCustomAsync(FoodItem food)
        {
            bool fileExists = File.Exists(FilePath);
            var line = $"{CsvEscape(food.Name)},{food.CaloriesPer100g.ToString(CultureInfo.InvariantCulture)},{food.ProteinPer100g.ToString(CultureInfo.InvariantCulture)}";

            if (!fileExists)
            {
                var header = "Name,CaloriesPer100g,ProteinPer100g";
                await File.WriteAllTextAsync(FilePath, header + Environment.NewLine + line + Environment.NewLine, Encoding.UTF8);
            }
            else
            {
                await File.AppendAllTextAsync(FilePath, line + Environment.NewLine, Encoding.UTF8);
            }
        }

        public static async Task<List<FoodItem>> LoadCustomAsync()
        {
            var foods = new List<FoodItem>();
            if (!File.Exists(FilePath)) return foods;

            var lines = await File.ReadAllLinesAsync(FilePath, Encoding.UTF8);
            foreach (var line in lines.Skip(1)) 
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');
                if (parts.Length < 3) continue;

                if (double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var kcal) &&
                    double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var protein))
                {
                    foods.Add(new FoodItem
                    {
                        Name = parts[0],
                        CaloriesPer100g = kcal,
                        ProteinPer100g = protein
                    });
                }
            }
            return foods;
        }

        private static string CsvEscape(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}