using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Domain.Events;

public class UserLoggedEvent
{
    public int UserId { get; set; }
    public string Email { get; set; }
}
