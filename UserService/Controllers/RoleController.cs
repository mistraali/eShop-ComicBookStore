using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Services;
using UserService.Domain.Repositories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private IRoleService _roleService;
        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }


        // GET: api/<RoleController>
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Get()
        {
            var result = await _roleService.GetAllRolesAsync();

            return Ok(result);
        }

        // GET api/<RoleController>/5
        [HttpGet("by-id/{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _roleService.GetRoleByIdAsync(id);

            if (result == null)
            {
                return NotFound($"Rola o {id} nie została znaleziona.");
            }

            return Ok(result);
        }

        // GET api/<RoleController>/<by-name>
        [HttpGet("by-name/{name}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Get(string name)
        {
            var result = await _roleService.GetRoleByNameAsync(name);

            if (result == null)
            {
                return NotFound($"Rola {name} nie została znaleziona.");
            }

            return Ok(result);
        }

        // POST api/<RoleController>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Post(string roleName)
        {
            var result = await _roleService.AddRoleAsync(roleName);

            return Ok(result);
        }

        // DELETE api/<RoleController>/5
        [HttpDelete("by-name/{name}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(string name)
        {
            var result = await _roleService.DeleteRoleAsync(name);

            if (!result)
            {
                return NotFound($"Rola {name} nie została znaleziona.");
            }

            return Ok($"Rola {name} została usunięta.");
        }
    }
}
