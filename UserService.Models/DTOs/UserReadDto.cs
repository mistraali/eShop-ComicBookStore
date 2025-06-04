using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Domain.DTO;

public class UserReadDto
{
    public int Id { get; set; }

    public string Username { get; set; }

    public string Email { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<string> Roles { get; set; } = new(); // tylko nazwy ról
}
