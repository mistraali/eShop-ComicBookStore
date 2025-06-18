using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using UserService.Application.Kafka;
using UserService.Domain.Events;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Application.Services;

public class LoginService : ILoginService
{
    protected IJwtTokenService _jwtTokenService;
    protected IUserRepository _userRepository;
    private readonly UserKafkaProducer _kafkaProducer;

    public LoginService(IJwtTokenService jwtTokenService, IUserRepository userRepository, UserKafkaProducer kafkaProducer)
    {
        _jwtTokenService = jwtTokenService;
        _userRepository = userRepository;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<string> Login(string username, string password)
    {
        var user = await _userRepository.GetUserByUsernameAsync(username);

        if (user == null || user.Password != password || user.IsActive != true)
        {
            throw new InvalidCredentialsException();
        }

        var roles = user.Roles.Select(r => r.Name).ToList();

        var token = _jwtTokenService.GenerateToken(user.Id, roles);

        Console.WriteLine($"[LoginService] Sending UserLoggedEvent for user: {user.Id}, {user.Email}");

        await _kafkaProducer.PublishUserLoggedAsync(new UserLoggedEvent
        {
            UserId = user.Id,
            Email = user.Email
        });

        Console.WriteLine("[LoginService] UserLoggedEvent sent.");

        return token;
    }
}
