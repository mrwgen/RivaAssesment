using Microsoft.AspNetCore.Mvc;
using RivaAssessment.Services;

namespace RivaAssessment.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : Controller
    {
        private readonly ICreditService _creditService;

        public HomeController(ICreditService creditService)
        {
            _creditService = creditService;
        }

        /// <summary>
        /// Gets the current credit balance for the user identified by the X-User-Id header.
        /// </summary>
        /// <returns></returns>
        [HttpGet("credits")]
        public async Task<IActionResult> GetCredits()
        {
            if (!Request.Headers.TryGetValue("X-User-Id", out var userIdValues))
            {
                return Unauthorized("Missing X-User-Id Header");
            }

            var userId = userIdValues.ToString();

            var credits = await _creditService.GetCredits(userId);

            return Ok(new
            {
                UserId = userId,
                Credits = credits
            }
            );
        }
    }
}
