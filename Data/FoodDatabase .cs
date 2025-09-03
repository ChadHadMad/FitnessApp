using System.Text;
using FitnessApp.Models;
using System.Reflection;
using FitnessApp.Data;
namespace FitnessApp.Services
{
    public static class FoodDatabase
    {
        public static List<FoodItem> Foods { get; private set; } = new();
        private static bool _isInitialized = false;

        public static async Task InitializeAsync()
        {
            if (_isInitialized) return;

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "FitnessApp.Data.cleaned_nutrition_dataset_per100g.csv";

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    LoadDefaultFoods();
                }
                else
                {
                    using var reader = new StreamReader(stream, Encoding.UTF8);

                    await reader.ReadLineAsync(); 

                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var parts = line.Split(',');

                        if (parts.Length >= 12 &&
                            double.TryParse(parts[7], System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture, out double kcal) &&
                            double.TryParse(parts[11], System.Globalization.NumberStyles.Float,
                                System.Globalization.CultureInfo.InvariantCulture, out double protein))
                        {
                            Foods.Add(new FoodItem
                            {
                                Name = parts[5].Trim('"'),
                                CaloriesPer100g = kcal,
                                ProteinPer100g = protein
                            });
                        }
                    }
                }

                try
                {
                    var customFoods = await FoodCsvService.LoadCustomAsync();
                    if (customFoods.Count > 0)
                    {
                        var existing = new HashSet<string>(
                            Foods.Select(f => f.Name),
                            StringComparer.OrdinalIgnoreCase);

                        foreach (var cf in customFoods)
                            if (!existing.Contains(cf.Name))
                                Foods.Add(cf);
                    }
                }
                catch {}

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading food database: {ex.Message}");
                LoadDefaultFoods();
                _isInitialized = true;
            }
        }

        private static void LoadDefaultFoods()
        {
            Foods.AddRange(new List<FoodItem>
            {
                new() { Name = "Apple", CaloriesPer100g = 52, ProteinPer100g = 0.3 },
                new() { Name = "Banana", CaloriesPer100g = 89, ProteinPer100g = 1.1 },
                new() { Name = "Chicken Breast", CaloriesPer100g = 165, ProteinPer100g = 31 },
                new() { Name = "Rice (Cooked)", CaloriesPer100g = 130, ProteinPer100g = 2.7 },
                new() { Name = "Bread (White)", CaloriesPer100g = 265, ProteinPer100g = 9 }
            });
        }

        public static async Task AddFoodAsync(string name, double kcal, double protein, bool persist = true)
        {
            if (string.IsNullOrWhiteSpace(name)) return;

            var item = new FoodItem
            {
                Name = name.Trim(),
                CaloriesPer100g = kcal,
                ProteinPer100g = protein
            };

            Foods.Add(item);

            if (persist)
            {
                try { await FoodCsvService.AppendCustomAsync(item); }
                catch {  }
            }
        }

        public static void AddFood(string name, double kcal, double protein)
            => AddFoodAsync(name, kcal, protein, persist: true).GetAwaiter().GetResult();

        public static void DeleteFood(FoodItem food) => Foods.Remove(food);

        public static void UpdateFood(FoodItem food, double newKcal, double newProtein)
        {
            food.CaloriesPer100g = newKcal;
            food.ProteinPer100g = newProtein;
        }

        public static List<FoodItem> SearchFoods(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Foods.ToList();

            return Foods.Where(f => f.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }
}