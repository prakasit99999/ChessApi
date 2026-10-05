using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChessApi.DTOs.User;
using ChessApi.Services.Interfaces;
using ChessApi.Utilities.Helpers;

namespace ChessApi.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// ดึงข้อมูลโปรไฟล์ของ User ที่ Login อยู่ปัจจุบัน
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = UserClaimHelper.GetUserIdFromToken(User);
            if (userId == null || userId <= 0)
            {
                return Unauthorized(new { message = "Invalid token", success = false });
            }

            var profile = await _userService.GetProfileByIdAsync(userId.Value);
            if (profile == null)
            {
                return StatusCode(500, new { message = "Unexpected error.", success = false });
            }

            if (!profile.Success)
            {
                if (profile.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return NotFound(profile);
                }
                return BadRequest(profile);
            }

            return Ok(profile);
        }

        /// <summary>
        /// ดึงข้อมูลโปรไฟล์ตาม User ID (GET /api/user/{id} หรือ GET /api/user/profile/{id})
        /// </summary>
        [HttpGet("{id:int}")]
        [HttpGet("profile/{id:int}")]
        public async Task<IActionResult> GetProfileById([FromRoute] int id)
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
        /// แก้ไขข้อมูลโปรไฟล์ตนเอง (Username, Email, Password)
        /// </summary>
        [HttpPut("profile")]
        [HttpPut("edit")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = UserClaimHelper.GetUserIdFromToken(User);
            if (userId == null || userId <= 0)
            {
                return Unauthorized(new { message = "Invalid token", success = false });
            }

            var result = await _userService.UpdateProfileAsync(userId.Value, request);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Admin แก้ไขข้อมูล User ใดๆ ตาม ID (PUT /api/user/{id})
        /// เฉพาะ Admin เท่านั้น - สามารถแก้ไข Username, Email, Password (ไม่ต้องใส่รหัสผ่านเก่า), Rating, Status
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> AdminUpdateUser([FromRoute] int id, [FromBody] AdminUpdateUserRequest request)
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

        /// <summary>
        /// ดึงรายชื่อ User แบบแบ่งหน้า (Pagination) สำหรับตารางใน Frontend
        /// รองรับ pageNumber, pageSize และ search (ค้นหา username/email)
        /// </summary>
        [HttpGet("paged")]
        [HttpGet]
        public async Task<IActionResult> GetPagedUsers([FromQuery] UserPagedRequest request)
        {
            var pagedResult = await _userService.GetPagedUsersAsync(request);
            return Ok(pagedResult);
        }

        /// <summary>
        /// ดึงรายชื่อ User ทั้งหมด (ส่ง Username, email, rating, status)
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllPlayersAsync();
            return Ok(users);
        }

        /// <summary>
        /// ค้นหา User ตามคำค้นหา
        /// </summary>
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

        /// <summary>
        /// อัปเดตสถานะ User (online, offline, playing)
        /// </summary>
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
        {
            var userId = UserClaimHelper.GetUserIdFromToken(User);
            if (userId == null || userId <= 0)
            {
                return Unauthorized(new { message = "Invalid token", success = false });
            }

            var success = await _userService.UpdateUserStatusAsync(userId.Value, request.Status);
            if (!success)
            {
                return StatusCode(500, new { message = "Unexpected error.", success = false });
            }

            return Ok(new UpdateStatusResponse { Message = "Success", Success = true });
        }

        /// <summary>
        /// ดึงจำนวนผู้ใช้ทั้งหมด: Users ทั้งหมด, Online, Offline, และ Pending
        /// (สามารถเรียกผ่าน REST API หรือผ่าน SignalR ที่ /userhub ได้เช่นกัน)
        /// </summary>
        [HttpGet("counts")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserCounts()
        {
            var counts = await _userService.GetUserCountsAsync();
            return Ok(counts);
        }
    }
}