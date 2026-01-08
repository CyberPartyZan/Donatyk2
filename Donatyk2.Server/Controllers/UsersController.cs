using Donatyk2.Server.Data;
using Donatyk2.Server.Dto;
using Donatyk2.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var users = await _usersService.GetAll(search, page, pageSize);
            return Ok(users);
        }

        [HttpGet("{id}", Name = "GetUserById")]
        public async Task<IActionResult> Get(Guid id)
        {
            // TODO: Or I should use JwtRegisteredClaimNames.Sub to get UserId from claims?
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim is null || (!User.IsInRole("Admin") && userIdClaim != id.ToString()))
            {
                // Don't return Forbid() to avoid information leakage
                return NotFound();
            }

            var user = await _usersService.GetById(id);
            if (user is null) 
                return NotFound();

            return Ok(user);
        }

        [HttpGet("by-email")]
        public async Task<IActionResult> GetByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest("Email is required.");
            var user = await _usersService.GetByEmail(email);

            if (user is null) 
                return NotFound();

            // TODO: Or I should use JwtRegisteredClaimNames.Sub to get UserId from claims?
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && user.Id.ToString() != userIdClaim)
            {
                // Don't return Forbid() to avoid information leakage
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] UserDto user)
        {
            // TODO: Or I should use JwtRegisteredClaimNames.Sub to get UserId from claims?
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && id.ToString() != userIdClaim)
            {
                return BadRequest();
            }

            if (user is null) 
                return BadRequest();

            user.Id = id;
            await _usersService.Update(user);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            // TODO: Or I should use JwtRegisteredClaimNames.Sub to get UserId from claims?
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && id.ToString() != userIdClaim)
            {
                return BadRequest();
            }

            await _usersService.Delete(id);
            return NoContent();
        }
    }
}
