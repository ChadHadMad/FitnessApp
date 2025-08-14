using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessApp.Models
{
    public class WorkoutLogData
    {
        public string Date { get; set; } = string.Empty;
        public List<WorkoutItem> Workouts { get; set; }
    }
}