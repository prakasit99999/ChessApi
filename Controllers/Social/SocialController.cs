using ChessApi.DTOs.Social;
using ChessApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChessApi.Controllers.Social
{
    [Route("api/[controller]")]
    [ApiController]
    public class SocialController : ControllerBase
    {
        private readonly ISocialService _socialService;

        public SocialController(ISocialService socialService)
        {
            _socialService = socialService;
        }

        [HttpPost("friend/add")]
        public async Task<IActionResult> AddFriend([FromBody] FriendRequestDto request)
        {
            var success = await _socialService.SendFriendRequestAsync(request.SenderId, request.TargetUsername);
            if (!success)
            {
                return BadRequest("Cannot send friend request. User may not exist or request already sent.");
            }
            return Ok(new { Message = "Friend request sent successfully." });
        }

        [HttpPost("friend/accept")]
        public async Task<IActionResult> AcceptFriend([FromBody] AcceptFriendRequestDto request)
        {
            var success = await _socialService.AcceptFriendRequestAsync(request.UserId, request.RequesterId);
            if (!success) return BadRequest("Friend request not found or already processed.");
            return Ok(new { Message = "Friend request accepted." });
        }

        [HttpGet("friend/list/{userId}")]
        public async Task<IActionResult> GetFriendList(int userId)
        {
            var friends = await _socialService.GetFriendListAsync(userId);
            return Ok(friends);
        }
    }
}
