using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChessApi.DTOs.User;
using ChessApi.Services.Interfaces;

namespace ChessApi.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // ต้องมี token
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var idClaim = User.FindFirst("id"); // claim ที่เราใส่ตอนสร้าง JWT
            if (idClaim == null)
            {
                return Unauthorized(new { message = "Invalid token", success = false });
            }

            if (!int.TryParse(idClaim.Value, out var userId))
            {
                return BadRequest(new { message = "Invalid user id", success = false });
            }

            var request = new ProfileRequest { UserId = userId };
            var profile = await _userService.GetProfileAsync(request);

            if (profile == null)
            {
                return StatusCode(500, new { message = "Unexpected error.", success = false });
            }

            if (!profile.Success)
            {
                if (profile.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(profile);
                }
                return BadRequest(profile);
            }

            return Ok(profile);
        }
    }
}
