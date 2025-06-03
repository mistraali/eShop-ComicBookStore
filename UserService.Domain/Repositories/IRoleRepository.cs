using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Domain.Models;

namespace UserService.Domain.Repositories;

public interface IRoleRepository
{
    Task<List<Role>> GetAllRolesAsync();
    Task<Role> GetByNameAsync(string name);
    Task<Role> GetByIdAsync(int id);
    Task<Role> AddRoleAsync(Role role);
    Task DeleteRoleAsync(Role role);
}
