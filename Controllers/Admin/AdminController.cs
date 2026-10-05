using ChessApi.DTOs.Auth;
using ChessApi.DTOs.User;
using ChessApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChessApi.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AdminController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        /// <summary>
        /// เข้าสู่ระบบเฉพาะสำหรับ Admin (POST /api/admin/login)
        /// รองรับการกรอก Username (เช่น "admin") หรือ Email (เช่น "admin@chess.com")
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
        {
            var response = await _authService.AdminLoginAsync(request);

            if (response == null)
                return StatusCode(500, new { message = "Unexpected error.", success = false });

            if (!response.Success)
            {
                if (response.Message.Contains("denied", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(403, response);

                if (response.Message.Contains("password", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized(response);

                if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(response);

                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// ตรวจสอบสถานะการยืนยันตัวตนของ Admin (GET /api/admin/verify)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("verify")]
        public IActionResult VerifyAdmin()
        {
            return Ok(new
            {
                success = true,
                message = "Admin authorization verified successfully.",
                username = User.FindFirst("username")?.Value,
                role = "Admin"
            });
        }

        /// <summary>
        /// ดึงข้อมูลรายละเอียดของ User ใดๆ ตาม ID สำหรับ Admin
        /// (GET /api/admin/users/{id} หรือ GET /api/admin/user/{id})
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("users/{id:int}")]
        [HttpGet("user/{id:int}")]
        public async Task<IActionResult> GetUserById([FromRoute] int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid user id", success = false });
            }

            var profile = await _userService.GetProfileByIdAsync(id);
            if (!profile.Success)
            {
                return NotFound(profile);
            }

            return Ok(profile);
        }

        /// <summary>
        /// Admin แก้ไขข้อมูล User ใดๆ ตาม ID
        /// (PUT /api/admin/users/{id} หรือ PUT /api/admin/user/{id})
        /// สามารถแก้ไข Username, Email, Password, Rating, Status
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("users/{id:int}")]
        [HttpPut("user/{id:int}")]
        public async Task<IActionResult> UpdateUser([FromRoute] int id, [FromBody] AdminUpdateUserRequest request)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid user id", success = false });
            }

            var result = await _userService.AdminUpdateUserAsync(id, request);
            if (!result.Success)
            {
                if (result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return NotFound(result);
                }

                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
