using ChessApi.DTOs.Matchmaking;
using ChessApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ChessApi.Controllers.Matchmaking
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchmakingController : ControllerBase
    {
        private readonly IMatchmakingService _matchmakingService;
        [HttpGet("join")]
        public async Task<IActionResult> JoinQueue([FromQuery] string username, [FromQuery] int minRating, [FromQuery] int maxRating, [FromQuery] int preferredTimeControl)
        {
            var request = new JoinQueueRequest
            {
                Username = username,
                MinRating = minRating,
                MaxRating = maxRating,
                PreferredTimeControl = preferredTimeControl
            };

            await _matchmakingService.JoinQueueAsync(request);
            return Ok(new { message = "Joined matchmaking queue successfully." });
        }


        [HttpGet("cancel")]
        public async Task<IActionResult> CancelQueue([FromQuery] string username)
        {
            var request = new CancelQueueRequest
            {
                Username = username
            };
            // Call the matchmaking service to cancel the queue
            await _matchmakingService.CancelQueueAsync(request);
            return Ok(new { message = "Cancelled matchmaking queue successfully." });
        }

        [HttpGet("check")]
        public async Task<IActionResult> CheckForMatch([FromQuery] string username)
        {
            // Call the matchmaking service to check for a match
            var matchFound = _matchmakingService.CheckForMatchAsync(username).GetAwaiter().GetResult();
            if (matchFound != null)
            {
                return Ok(new { message = "Match found!", MatchDetails = matchFound });
            }
            return NotFound(new { message = "No match found." });
        }
    }
}
