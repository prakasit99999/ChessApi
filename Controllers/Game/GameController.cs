using ChessApi.DTOs.Game;
using ChessApi.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChessApi.Controllers.Game
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost("end")]
        public async Task<IActionResult> EndGame([FromBody] GameResultDto dto)
        {
            var result = await _gameService.FinalizeGameAsync(dto);
            return result ? Ok("Game ended and stats updated.") : BadRequest("Failed to end game.");
        }



        [HttpGet("result/{gameId}")]
        public async Task<IActionResult> GetGameResult(int gameId)
        {
            var gameResult = await _gameService.GetGameResultAsync(gameId);
            if (gameResult == null)
            {
                return NotFound("Game result not found.");
            }
            return Ok(gameResult);

        }

        [HttpGet("status/{gameId}")]
        public async Task<IActionResult> GetGameStatus(int gameId)
        {
            var gameResult = await _gameService.GetGameResultAsync(gameId);
            if (gameResult == null)
            {
                return NotFound("Game status not found.");
            }
            return Ok(new
            {
                GameId = gameResult.GameId,
                GameType = gameResult.GameType,
                Status = gameResult.Result ?? "In Progress",
                MoveCount = gameResult.MoveCount,
                IsRated = gameResult.IsRated
            });

        }
    }
}
