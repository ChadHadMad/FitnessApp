using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitnessApp.Models;
using System.Reflection;

namespace FitnessApp.Services
{
    public static class FoodDatabase
    {
        public static List<FoodItem> Foods { get; private set; } = new();
        private static bool _isInitialized = false;

        public static async Task InitializeAsync()
        {
            if (_isInitialized)
                return; // Already loaded

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "FitnessApp.Data.foundation_foods.csv"; // Updated filename

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    // Fallback to default foods if CSV not found
                    LoadDefaultFoods();
                    _isInitialized = true;
                    return;
                }

                using var reader = new StreamReader(stream, Encoding.UTF8);

                // Skip header line
                await reader.ReadLineAsync();

                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    // Simple split approach - your CSV has quotes around names
                    int lastCommaIndex = line.LastIndexOf(',');
                    if (lastCommaIndex > 0)
                    {
                        string name = line.Substring(0, lastCommaIndex).Trim().Trim('"');
                        string calorieString = line.Substring(lastCommaIndex + 1).Trim();

                        // Debug output - remove after fixing
                        System.Diagnostics.Debug.WriteLine($"Line: {line}");
                        System.Diagnostics.Debug.WriteLine($"Name: '{name}', Calories: '{calorieString}'");

                        if (!string.IsNullOrWhiteSpace(name) &&
                            double.TryParse(calorieString, System.Globalization.NumberStyles.Float,
                                          System.Globalization.CultureInfo.InvariantCulture, out double calories))
                        {
                            System.Diagnostics.Debug.WriteLine($"Successfully parsed: {name} = {calories} kcal");
                            Foods.Add(new FoodItem
                            {
                                Name = name,
                                Calories = calories
                            });
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to parse: name='{name}', calorieString='{calorieString}'");
                        }
                    }
                }

                _isInitialized = true;
                System.Diagnostics.Debug.WriteLine($"Loaded {Foods.Count} food items from CSV");
            }
            catch (Exception ex)
            {
                // Log the exception and load default foods as fallback
                System.Diagnostics.Debug.WriteLine($"Error loading food database: {ex.Message}");
                LoadDefaultFoods();
                _isInitialized = true;
            }
        }

        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    // Don't add the quote character to the result
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString().Trim());
            return result.ToArray();
        }

        private static void LoadDefaultFoods()
        {
            Foods.AddRange(new List<FoodItem>
            {
                new() { Name = "Apple", Calories = 95 },
                new() { Name = "Banana", Calories = 105 },
                new() { Name = "Orange", Calories = 62 },
                new() { Name = "Chicken Breast (100g)", Calories = 165 },
                new() { Name = "Rice (1 cup)", Calories = 205 },
                new() { Name = "Bread (1 slice)", Calories = 79 },
                new() { Name = "Milk (1 cup)", Calories = 149 },
                new() { Name = "Eggs (1 large)", Calories = 70 }
            });
        }

        public static void AddFood(string name, double calories)
        {
            if (!string.IsNullOrWhiteSpace(name) && calories >= 0)
            {
                Foods.Add(new FoodItem { Name = name.Trim(), Calories = calories });
            }
        }

        public static List<FoodItem> SearchFoods(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Foods.ToList();

            return Foods.Where(f => f.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                       .ToList();
        }
    }
}