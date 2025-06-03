using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Domain.Models;
using UserService.Domain.Repositories;
using UserService.Domain.DTOs;
using Microsoft.EntityFrameworkCore;

namespace UserService.Application.Services;

public class UserService : IUserService
{
    private IUserRepository _userRepository;
    private IRoleRepository _roleRepository;
    public UserService(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<UserReadDto> AddUserAsync(UserRegisterDto userRegisterDto)
    {
        var defaultRole = await _roleRepository.GetByNameAsync("Client");
        if (defaultRole == null)
        {
            throw new InvalidOperationException("'Client' role does not exist.");
        }

        var newUser = new User
        {
            Username = userRegisterDto.Username,
            Email = userRegisterDto.Email,
            Password = userRegisterDto.Password,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Roles = new List<Role> { defaultRole }
        };

        var result = await _userRepository.AddUserAsync(newUser);

        // MAPPING: User -> UserReadDto
        var dto = new UserReadDto
        {
            Id = result.Id,
            Username = result.Username,
            Email = result.Email,
            CreatedAt = result.CreatedAt,
            Roles = result.Roles.Select(r => r.Name).ToList(),
        };
        return dto;

    }

    public async Task<List<AdminReadDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllUsersAsync();

        var userDtos = users.Select(u => new AdminReadDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            LastLoginAt = u.LastLoginAt,
            Roles = u.Roles.Select(r => r.Name).ToList(),
            Password = u.Password
        }).ToList();

        return userDtos;
    }

    public async Task<AdminReadDto> GetUserByIdAsync(int id)
    {
        var user  = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
        {
            throw new InvalidOperationException($"User with id: {id} does not exist.");
        }

        var dto = new AdminReadDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = user.Roles.Select(r => r.Name).ToList(),
            Password = user.Password 
        };

        return dto;
    }

    public async Task<UserReadDto> GetUserDtoByIdAsync(int id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
        {
            throw new InvalidOperationException($"User with id: {id} does not exist.");
        }

        var dto = new UserReadDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Roles = user.Roles.Select(r => r.Name).ToList(),
        };

        return dto;
    }

    public async Task<AdminReadDto> GetUserByUsernameAsync(string username)
    {
        var user = await _userRepository.GetUserByUsernameAsync(username);
        if (user == null)
        {
            throw new InvalidOperationException($"User with username: {username} does not exist.");
        }

        var dto = new AdminReadDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = user.Roles.Select(r => r.Name).ToList(),
            Password = user.Password
        };

        return dto;
    }

    public async Task<bool> ChangePasswordAsync(int userId, UserChangePasswordDto changePasswordDto)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);

        if (user.Password != changePasswordDto.OldPassword)
        {
            throw new InvalidOperationException("Old password is incorrect.");
        }
        else
        {
            user.Password = changePasswordDto.NewPassword;
            await _userRepository.UpdateUserAsync(user);

            return true;
        }
    }

    public async Task<bool> EditUserAccountAsync(int userId, UserEditDto editAccountDto)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException($"User with id: {userId} does not exist.");
        }
        else
        {
            user.Username = editAccountDto.NewUsername;
            user.Email = editAccountDto.NewEmail;
            await _userRepository.UpdateUserAsync(user);

            return true;
        }
    }

    public async Task<bool> AdminEditUserAccountAsync(int userId, UserUpdateDto userUpdateDto)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException($"User with id: {userId} does not exist.");
        }
        else
        {
            if (!string.IsNullOrEmpty(userUpdateDto.NewUsername))
            {
                user.Username = userUpdateDto.NewUsername;
            }
            if (!string.IsNullOrEmpty(userUpdateDto.NewEmail))
            {
                user.Email = userUpdateDto.NewEmail;
            }
            if (!string.IsNullOrEmpty(userUpdateDto.NewPassword) && userUpdateDto.NewPassword.Length >= 5)
            {
                user.Password = userUpdateDto.NewPassword;
            }
            else
            {
                throw new InvalidOperationException("New password must be at least 5 characters long.");
            }
            if (userUpdateDto.NewIsActive != null)
            {
                user.IsActive = userUpdateDto.NewIsActive;
            }
            
            //var newRoles = new List<Role> { };
            //user.Roles = newRoles;

            await _userRepository.UpdateUserAsync(user);

            return true;
        }
    }
}
