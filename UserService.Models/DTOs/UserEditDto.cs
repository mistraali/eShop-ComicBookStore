using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Domain.Models;

namespace UserService.Domain.DTO;

public class UserEditDto
{
    [MaxLength(100)]
    public string NewUsername { get; set; }

    [MaxLength(255)]
    public string NewEmail { get; set; }
}
