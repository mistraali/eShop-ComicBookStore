using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Domain.DTO;

public class UserChangePasswordDto
{
    [Required]
    public string OldPassword { get; set; }

    [Required]
    [MinLength(5)]
    public string NewPassword { get; set; }
}
