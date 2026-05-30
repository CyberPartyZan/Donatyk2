using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Marketplace.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryPreferencesController : ControllerBase
    {
        private readonly IDeliveryPreferencesService _deliveryPreferencesService;

        public DeliveryPreferencesController(IDeliveryPreferencesService deliveryPreferencesService)
        {
            _deliveryPreferencesService = deliveryPreferencesService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyPreferences()
        {
            var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrWhiteSpace(sub))
                return Unauthorized();

            var userId = Guid.Parse(sub);
            var preferences = await _deliveryPreferencesService.GetByUserId(userId);

            return Ok(preferences);
        }
    }
}