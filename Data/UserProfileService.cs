using FitnessApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace FitnessApp.Services
{
    public static class UserProfileService
    {
        private static readonly string filePath =
            Path.Combine(FileSystem.AppDataDirectory, "userprofile.json");

        public static async Task SaveProfileAsync(UserProfile profile)
        {
            var json = JsonSerializer.Serialize(profile);
            string filePath = Path.Combine(FileSystem.AppDataDirectory, "userprofile.json");
            await File.WriteAllTextAsync(filePath, json);
        }

        public static async Task<UserProfile?> LoadProfileAsync()
        {
            if (!File.Exists(filePath))
                return null;

            string json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<UserProfile>(json);
        }

        public static void DeleteProfile()
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}