using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Models;
using UserService.Domain.Repositories;

namespace UserService.Domain.Seeders;

public class UserDbSeeder (UserDataContext context) : IUserDbSeeder
{
    
    public async Task Seed()
    {
        var roles = new List<Role>
        {
            new Role { Name = "Admin" },
            new Role { Name = "Employee" },
            new Role { Name = "Client" }
        };

        context.Roles.AddRange(roles);
        context.SaveChanges();

        var adminRole = context.Roles.First(r => r.Name == "Admin");    
        var employeeRole = context.Roles.First(r => r.Name == "Employee");
        var clientRole = context.Roles.First(r => r.Name == "Client");

        if (!context.Users.Any())
        {
            var user1 = new User
            {
                Username = "admin",
                Email = "admin@comicestore.pl",
                Password = "haslo",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Roles = new List<Role> { adminRole, employeeRole, clientRole }
            };

            var user2 = new User
            {
                Username = "tomek",
                Email = "tomek@comicestore.pl",
                Password = "tomek",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Roles = new List<Role> { employeeRole, clientRole }
            };

            var user3 = new User
            {
                Username = "bartek",
                Email = "bartek@comicestore.pl",
                Password = "bartek",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Roles = new List<Role> { clientRole }
            };

            context.Users.AddRange(user1, user2, user3);

            context.SaveChanges();
        }
    }
}
