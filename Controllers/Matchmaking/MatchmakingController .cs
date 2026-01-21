using ChessApi.DTOs.Matchmaking;
using ChessApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChessApi.Controllers.Matchmaking
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchmakingController : ControllerBase
    {
        private readonly IMatchmakingService _matchmakingService;

        public MatchmakingController(IMatchmakingService matchmakingService)
        {
            _matchmakingService = matchmakingService;
        }

        [HttpGet("join")]
        public async Task<IActionResult> JoinQueue([FromQuery] string username, [FromQuery] int minRating, [FromQuery] int maxRating)
        {
            var request = new JoinQueueDTOs
            {
                Username = username,
                MinRating = minRating,
                MaxRating = maxRating,
            };

            // รับผลลัพธ์ทันที (เผื่อจับคู่ได้เลย)
            var matchResult = await _matchmakingService.JoinQueueAsync(request);

            if (matchResult != null)
            {
                return Ok(new { message = "Match found immediately!", matchDetails = matchResult });
            }

            return Ok(new { message = "Joined matchmaking queue. Please poll /check to find a match." });
        }

        [HttpGet("check")]
        public async Task<IActionResult> CheckForMatch([FromQuery] string username)
        {
            var matchFound = await _matchmakingService.CheckForMatchAsync(username);
            
            if (matchFound != null)
            {
                return Ok(new { message = "Match found!", matchDetails = matchFound });
            }
            return NotFound(new { message = "Still searching..." });
        }

        [HttpGet("cancel")]
        public async Task<IActionResult> CancelQueue([FromQuery] string username)
        {
            await _matchmakingService.CancelQueueAsync(new CancelQueueDTOs { Username = username });
            return Ok(new { message = "Cancelled matchmaking queue successfully." });
        }
    }
}