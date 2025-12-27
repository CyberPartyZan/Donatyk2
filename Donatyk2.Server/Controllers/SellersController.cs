using Donatyk2.Server.Dto;
using Donatyk2.Server.Models;
using Donatyk2.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Donatyk2.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SellersController : ControllerBase
    {
        private readonly ISellersService _sellersService;

        public SellersController(ISellersService sellersService)
        {
            _sellersService = sellersService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string search, [FromQuery] int page, [FromQuery] int pageSize)
        {
            var sellers = await _sellersService.GetAll(search, page, pageSize);

            return Ok(sellers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var seller = await _sellersService.GetById(id);

            if (seller is null)
            {
                return NotFound();
            }

            return Ok(seller);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SellerDto seller)
        {
            await _sellersService.Create(seller);

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromBody] SellerDto seller, Guid id)
        {
            await _sellersService.Update(id, seller);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _sellersService.Delete(id);

            return Ok();
        }
    }
}
