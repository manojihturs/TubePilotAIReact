using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TubePilot.Domain.Entities;

namespace TubePilot.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(TubePilotDbContext db)
        {
            // Only seed demo user in Development environment via application startup seed.
            // This method assumes the caller checks the environment before calling in Program.cs.
            if (await db.Users.AnyAsync()) return;

            // Create a seeded admin user with a hashed password if an environment variable is provided.
            // For local development, set TUBEPILOT_SEED_ADMIN_PASSWORD env var to control the seeded password.
            var seedPassword = Environment.GetEnvironmentVariable("TUBEPILOT_SEED_ADMIN_PASSWORD");
            if (string.IsNullOrEmpty(seedPassword))
            {
                // Do not seed a user if no password was provided to avoid insecure defaults.
                return;
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@tubepilot.local",
                Password = HashPassword(seedPassword),
                Role = "Admin"
            };

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();
        }

        // Simple PBKDF2-based hashing for demo seeding. Not a replacement for ASP.NET Identity.
        private static string HashPassword(string password)
        {
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var salt = new byte[16];
            rng.GetBytes(salt);
            using var deriveBytes = new System.Security.Cryptography.Rfc2898DeriveBytes(password, salt, 10000, System.Security.Cryptography.HashAlgorithmName.SHA256);
            var hash = deriveBytes.GetBytes(32);
            var result = new byte[49]; // 16 salt + 32 hash + 1 prefix
            Buffer.BlockCopy(salt, 0, result, 1, 16);
            Buffer.BlockCopy(hash, 0, result, 17, 32);
            result[0] = 0x01; // version prefix
            return Convert.ToBase64String(result);
        }
    }
}
