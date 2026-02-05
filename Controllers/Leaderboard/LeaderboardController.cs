using ChessApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChessApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLeaderboard()
        {
            var leaderboard = await _leaderboardService.GetLeaderboardAsync();
            return Ok(leaderboard);
        }
    }
}
