using System;
using ChessApi.DTOs.Matchmaking;
using ChessApi.Services.Interfaces;
using ChessApi.Utilities.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChessApi.Controllers.Matchmaking
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MatchmakingController(IMatchmakingService matchmakingService) : ControllerBase
    {
        private readonly IMatchmakingService _matchmakingService = matchmakingService;

        private string GetUsernameFromToken()
        {
            var username = UserClaimHelper.GetUsernameFromToken(User);
            if (username == null)
                throw new UnauthorizedAccessException("User not authenticated");
            return username;
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinQueue([FromBody] JoinQueueDTOs request)
        {

            // รับผลลัพธ์ทันที (เผื่อจับคู่ได้เลย)
            var username = GetUsernameFromToken();
            var matchResult = await _matchmakingService.JoinQueueAsync(new JoinQueueDTOs { Username = username });
            if (matchResult != null)
            {
                return Ok(new { status = "matched", matchDetails = matchResult });
            }

            return Ok(new { status = "waiting", message = "Joined matchmaking queue. Please poll /check to find a match." });
        }

        [HttpGet("check")]
        public async Task<IActionResult> CheckForMatch()
        {
            var username = GetUsernameFromToken();
            var matchFound = await _matchmakingService.CheckForMatchAsync(username);

            if (matchFound != null)
            {
                return Ok(new { status = "matched", matchDetails = matchFound });
            }
            var status = await _matchmakingService.GetQueueStatusAsync(username);
            if (string.Equals(status, "expired", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { status = "expired", message = "Queue expired." });

            return NotFound(new { status = "waiting", message = "Still searching..." });
        }

        [HttpPut("cancel")]
        public async Task<IActionResult> CancelQueue()
        {
            var username = GetUsernameFromToken();
            await _matchmakingService.CancelQueueAsync(new CancelQueueDTOs { Username = username });
            return Ok(new { message = "Cancelled matchmaking queue successfully." });
        }
    }
}

