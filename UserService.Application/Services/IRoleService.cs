using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Domain.DTOs;
using UserService.Domain.Models;

namespace UserService.Application.Services;

public interface IRoleService
{
    Task<Role> GetRoleByIdAsync(int id);
    Task<Role> GetRoleByNameAsync(string name);
    Task<Role> AddRoleAsync(string roleName);
    Task<List<Role>> GetAllRolesAsync();
    Task<bool> DeleteRoleAsync(string roleName);
}
