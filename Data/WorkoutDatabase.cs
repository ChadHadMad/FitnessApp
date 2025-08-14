using FitnessApp.Models;
using System.Collections.Generic;

namespace FitnessApp.Data
{
    public static class WorkoutDatabase
    {
        public static List<WorkoutItem> GetWorkouts()
        {
            return new List<WorkoutItem>
            {
                new WorkoutItem { Name = "Running (30 min)", CaloriesBurned = 300 },
                new WorkoutItem { Name = "Cycling (30 min)", CaloriesBurned = 250 },
                new WorkoutItem { Name = "Swimming (30 min)", CaloriesBurned = 400 },
                new WorkoutItem { Name = "Jump Rope (15 min)", CaloriesBurned = 200 },
                new WorkoutItem { Name = "Yoga (45 min)", CaloriesBurned = 180 }
            };
        }
    }
}