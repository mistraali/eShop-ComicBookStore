using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Domain.Models;
using UserService.Domain.DTOs;

namespace UserService.Application.Services;

public interface IUserService
{
    Task<AdminReadDto> GetUserByIdAsync(int id);

    Task<UserReadDto> GetUserDtoByIdAsync(int id);
    Task<AdminReadDto> GetUserByUsernameAsync(string username);
    Task<UserReadDto> AddUserAsync(UserRegisterDto userRegisterDto);
    Task<bool> ChangePasswordAsync(int userId, UserChangePasswordDto changePasswordDto);
    Task<List<AdminReadDto>> GetAllUsersAsync();
    Task<bool> EditUserAccountAsync(int userId, UserEditDto userEditDto);
    Task<bool> AdminEditUserAccountAsync(int userId, UserUpdateDto userUpdateDto);
}
