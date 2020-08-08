using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using DatingApp.API.Models;
using Microsoft.Extensions.Logging;

namespace DatingApp.API.Data
{
    public class Seed
    {
        // private static ILogger _logger;
        //
        // public Seed(ILogger<Seed> logger)
        // {
        //     _logger = logger;
        // }
        public static void SeedUsers(DataContext context)
        {
            if (!context.Users.Any())
            {
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<User>>(userData);

                foreach (var user in users)
                {
                    byte[] passwordHash, passwordSalt;
                    createPasswordHash("password", out passwordHash, out passwordSalt);

                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;
                    user.Email = user.Email.ToLower();
                    context.Users.Add(user);
                    //_logger.LogInformation(user.Introduction);
                }

                context.SaveChanges();
            }
        }

        public static void createPasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash((System.Text.Encoding.UTF8.GetBytes(password)));
            }
        }
    }
}
