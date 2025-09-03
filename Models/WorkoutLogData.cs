using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace FitnessApp.Models
{
    public class WorkoutLogData
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Date { get; set; } = string.Empty;          
        public List<LoggedWorkout> Workouts { get; set; } = new();

        public double TotalCalories =>
            Workouts?.Sum(w => w.CaloriesPerMinute * w.DurationMinutes) ?? 0;
    }

}