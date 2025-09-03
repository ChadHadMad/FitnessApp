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
                new WorkoutItem { Name = "Running", CaloriesPerMinute = 10 },   
                new WorkoutItem { Name = "Cycling", CaloriesPerMinute = 8.33 },  
                new WorkoutItem { Name = "Swimming", CaloriesPerMinute = 13.33 }, 
                new WorkoutItem { Name = "Jump Rope", CaloriesPerMinute = 13.33 },
                new WorkoutItem { Name = "Yoga", CaloriesPerMinute = 4 }          
            };
        }
    }
}