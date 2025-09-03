using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessApp.Models
{
    public class LoggedWorkout
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public double CaloriesPerMinute { get; set; }
        public double DurationMinutes { get; set; }
        public DateTime Date { get; set; }

        public double CaloriesBurned => CaloriesPerMinute * DurationMinutes;
    }
}