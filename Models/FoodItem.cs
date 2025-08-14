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
        public string Name { get; set; } = string.Empty; 
        public double Calories { get; set; }
    }
}