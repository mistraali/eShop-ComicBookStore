using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Domain.DTO;

public class UserUpdateDto
{
    [MaxLength(100)]
    public string NewUsername { get; set; }

    [MaxLength(255)]
    public string NewEmail { get; set; }

    public bool NewIsActive { get; set; }

    public List<int> NewRoleIds { get; set; } = new();

    public string NewPassword { get; set; } 
}
