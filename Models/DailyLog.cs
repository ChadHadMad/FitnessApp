using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessApp.Models
{
        public class DailyLog
        {
            public DateTime Date { get; set; }

            public List<FoodItem> Foods { get; set; } = new();

            public double Calories { get; set; } 
            public double Protein { get; set; }  
            public double DailyGoal { get; set; } 
        }
}