using ChessApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;


namespace ChessApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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

        [HttpGet("admin/dashboard")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDashboardOverview()
        {
            var overview = await _leaderboardService.GetDashboardOverviewAsync();
            return Ok(overview);
        }
    }
}
