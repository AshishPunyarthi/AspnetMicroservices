using Discount.API.Entities;
using Discount.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Discount.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DiscountController : Controller
    {
        private readonly IDiscountRepository _discountRepository;
        private readonly ILogger<DiscountController> _logger;

        public DiscountController(IDiscountRepository basketRepository, ILogger<DiscountController> logger)
        {
            _discountRepository = basketRepository ?? throw new ArgumentNullException(nameof(basketRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("{productName}", Name = "GetDiscount")]
        [ProducesResponseType(typeof(Coupon), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDiscount(string productName)
        {
            Coupon coupon = await _discountRepository.GetDiscountAsync(productName);
            return Ok(coupon);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Coupon), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateDiscount([FromBody] Coupon coupon)
        {
            _ = await _discountRepository.CreateDiscountAsync(coupon);

            IActionResult createdCoupon = await GetDiscount(coupon.ProductName);
            return CreatedAtRoute("GetDiscount", new { productName = coupon.ProductName }, createdCoupon);
        }

        [HttpPut]
        [ProducesResponseType(typeof(Coupon), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateDiscount([FromBody] Coupon coupon)
        {
            return Ok(await _discountRepository.UpdateDiscountAsync(coupon));
        }

        [HttpDelete("{productName}", Name = "DeleteDiscount")]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteDiscount(string productName)
        {
            return Ok(await _discountRepository.DeleteDiscountAsync(productName));
        }
    }
}
