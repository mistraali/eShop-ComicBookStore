using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Domain.Models;

namespace UserService.Domain.Repositories;

public interface IUserRepository
{
    #region User
    Task<User> GetUserByIdAsync(int id);
    Task<User> GetUserByUsernameAsync(string username);
    Task<User> AddUserAsync(User user);
    Task<User> UpdateUserAsync(User user);
    Task<List<User>> GetAllUsersAsync();
    #endregion
}
