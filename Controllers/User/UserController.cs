using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChessApi.DTOs.User;
using ChessApi.Services.Interfaces;
using System.Diagnostics;

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
            var idClaim = User.FindFirst("id");
            if (idClaim == null)
            {
                return Unauthorized(new { message = "Invalid token", success = false });
            }

            if (!int.TryParse(idClaim.Value, out var userId))
            {
                return BadRequest(new { message = "Invalid user id", success = false });
            }

            var request = new ProfileRequest { UserId = userId };
            if (request.UserId <= 0)
            {
                return BadRequest(new { message = "Invalid user id", success = false });
            }
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
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllPlayersAsync();
            return Ok(users);
        }


        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                var allUsers = await _userService.GetAllPlayersAsync();
                return Ok(allUsers);
            }
            var users = await _userService.SearchPlayersAsync(query);

            return Ok(users);
        }

        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
        {
            var idClaim = User.FindFirst("id");
            if (idClaim == null || !int.TryParse(idClaim.Value, out var userId))
                return Unauthorized(new { message = "Invalid token" });

            var success = await _userService.UpdateUserStatusAsync(userId, request.Status);
            if (!success)
                return StatusCode(500, new { message = "Unexpected error.", success = false });

            return Ok(new UpdateStatusResponse { Message = "Success", Success = true });
        }




    }
}