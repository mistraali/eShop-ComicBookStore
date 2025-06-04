using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Application.Services;

public interface IJwtTokenService
{
    string GenerateToken(int userId, List<string> roles);
}
