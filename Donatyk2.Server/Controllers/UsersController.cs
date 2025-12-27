using Donatyk2.Server.Data;
using Donatyk2.Server.Dto;
using Donatyk2.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Donatyk2.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _usersService;

        public UsersController(IUsersService usersService)
        {
            _usersService = usersService;
        }

        // GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var users = await _usersService.GetAll(search, page, pageSize);
            return Ok(users);
        }

        // GET: api/users/{id}
        [HttpGet("{id}", Name = "GetUserById")]
        public async Task<IActionResult> Get(Guid id)
        {
            var user = await _usersService.GetById(id);
            if (user is null) return NotFound();
            return Ok(user);
        }

        // GET: api/users/by-email?email=...
        [HttpGet("by-email")]
        public async Task<IActionResult> GetByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest("Email is required.");
            var user = await _usersService.GetByEmail(email);
            if (user is null) return NotFound();
            return Ok(user);
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] UserDto user)
        {
            if (user is null) return BadRequest();
            user.Id = id;
            await _usersService.Update(user);
            return NoContent();
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _usersService.Delete(id);
            return NoContent();
        }
    }
}
