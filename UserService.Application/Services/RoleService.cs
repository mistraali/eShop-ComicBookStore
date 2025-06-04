using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using UserService.Domain.DTOs;
using UserService.Domain.Models;
using UserService.Domain.Repositories;

namespace UserService.Application.Services;

public class RoleService : IRoleService
{
    private IRoleRepository _roleRepository;
    public RoleService(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }


    public async Task<Role> AddRoleAsync(string roleName)
    {
        var newRole = new Role
        {
            Name = roleName
        };
        
        var result = await _roleRepository.AddRoleAsync(newRole);

        return result;
    }

    public async Task<bool> DeleteRoleAsync(string roleName)
    {
        var deletedRole = await _roleRepository.GetByNameAsync(roleName);
        if (deletedRole == null)
            return false;

        await _roleRepository.DeleteRoleAsync(deletedRole);
        return true;
    }

    public async Task<List<Role>> GetAllRolesAsync()
    {
        var result = await _roleRepository.GetAllRolesAsync();

        return result;
    }

    public async Task<Role> GetRoleByIdAsync(int id)
    {
        var result = await _roleRepository.GetByIdAsync(id);

        return result;
    }

    public async Task<Role> GetRoleByNameAsync(string name)
    {
        var result = await _roleRepository.GetByNameAsync(name);

        return result;
    }
}
