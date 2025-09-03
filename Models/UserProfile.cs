using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FitnessApp.Models
{
    public class UserProfile
    {
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string Gender { get; set; } = "";
        public double HeightCm { get; set; }
        public double WeightKg { get; set; }
        public double TargetWeightKg { get; set; }
        public int Age { get; set; }
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string ActivityLevel { get; set; } = "";
        public bool HasCompletedOnboarding { get; set; }
    }
}