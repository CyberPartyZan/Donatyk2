using Donatyk2.Server.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Donatyk2.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        // Login endpoint
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            throw new NotImplementedException();

            // Placeholder logic for user authentication
            if (request.Username == "testuser" && request.Password == "password")
            {
                return Ok(new { Token = "fake-jwt-token" });
            }
            return Unauthorized();
        }

        // Registration endpoint
        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult Register([FromBody] LoginRequest request)
        {
            throw new NotImplementedException();
            // Placeholder logic for user registration
            return Ok(new { Message = "User registered successfully" });
        }
    }
}
