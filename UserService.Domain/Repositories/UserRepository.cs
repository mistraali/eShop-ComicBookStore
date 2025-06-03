using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace UserService.Domain.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDataContext _context;

    public UserRepository(UserDataContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users.Include(u => u.Roles).ToListAsync();
    }

    public async Task<User> GetUserByIdAsync(int userId)
    {
        return await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User> GetUserByUsernameAsync(string username)
    {
        return await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User> AddUserAsync(User user)
    {
        foreach (var role in user.Roles)
        {
            _context.Attach(role); 
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }
}
