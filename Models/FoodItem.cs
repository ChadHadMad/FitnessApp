using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace FitnessApp.Models
{
    public class FoodItem
    {
        public string Name { get; set; } = "";
        public double CaloriesPer100g { get; set; }
        public double ProteinPer100g { get; set; }
        public double GramsConsumed { get; set; }

        public double TotalCalories => (CaloriesPer100g / 100.0) * GramsConsumed;
        public double TotalProtein => (ProteinPer100g / 100.0) * GramsConsumed;
    }
}