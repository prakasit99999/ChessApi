using Microsoft.AspNetCore.Mvc;
using ChessApi.DTOs.Auth;
using ChessApi.Services.Interfaces;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ChessApi.Controllers.Auth
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);

            if (response == null)
                return StatusCode(500, new { message = "Unexpected error.", success = false });

            if (!response.Success)
            {
                if (response.Message.Contains("password", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(response);

                if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var response = await _authService.RegisterAsync(request);

            if (response == null)
                return StatusCode(500, new { message = "Unexpected error.", success = false });

            if (!response.Success)
            {
                if (response.Message.Contains("exists", StringComparison.OrdinalIgnoreCase))
                    return Conflict(response);

                return BadRequest(response);
            }

            return CreatedAtAction(nameof(Register), new { id = response.Success }, response);
        }

    }
}
