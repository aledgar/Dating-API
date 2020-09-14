using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
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
        public static void SeedUsers(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            if (!userManager.Users.Any())
            {
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<User>>(userData);

                var roles = new List<Role>
                {
                    new Role {Name = "Member"},
                    new Role {Name = "Admin"},
                    new Role {Name = "Moderator"},
                    new Role {Name = "VIP"}
                };

                roles.ForEach(r => roleManager.CreateAsync(r).Wait());

                // foreach (var role in roles)
                // {
                //     roleManager.CreateAsync(role);
                // }
                var cont = 0;
                foreach (var user in users)
                {
                    //.Photos.SingleOrDefault().IsMain = true;
                    user.UserName = "null"+cont;
                    userManager.CreateAsync(user, "password").Wait();
                    userManager.AddToRoleAsync(user, "Member").Wait();
                    cont++;
                }

                var adminUser = new User
                {
                    Email = "admin@datingapp.com",
                    UserName = "null"+cont
                };

                var result = userManager.CreateAsync(adminUser, "password").Result;

                if (result.Succeeded)
                {
                    var admin = userManager.FindByEmailAsync("admin@datingapp.com").Result;
                    userManager.AddToRolesAsync(admin, new [] {"Admin", "Moderator"}).Wait();
                }
            }
        }

        // public static void createPasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        // {
        //     using (var hmac = new System.Security.Cryptography.HMACSHA512())
        //     {
        //         passwordSalt = hmac.Key;
        //         passwordHash = hmac.ComputeHash((System.Text.Encoding.UTF8.GetBytes(password)));
        //     }
        // }
    }
}
