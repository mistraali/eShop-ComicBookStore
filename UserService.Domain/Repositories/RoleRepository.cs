using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Models;

namespace UserService.Domain.Repositories;

public class RoleRepository :IRoleRepository
{
    private readonly UserDataContext _context;

    public RoleRepository(UserDataContext context)
    {
        _context = context;
    }

    public async Task<List<Role>> GetAllRolesAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    public async Task<Role> GetByNameAsync(string name)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<Role> GetByIdAsync(int id)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Role> AddRoleAsync(Role role)
    {
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task DeleteRoleAsync(Role role)
    {
        _context.Roles.Remove(role);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // unhandled
        }
    }

}
