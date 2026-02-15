﻿using ChessApi.DTOs.Game;
using ChessApi.Services.Interfaces;
using ChessApi.Utilities.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System;
using System.Threading.Tasks;

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

        // 1️ START GAME (ONLINE)
        [HttpPost("start/online")]
        public async Task<IActionResult> StartOnlineGame([FromBody] GameCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                int? userIdFromToken = UserClaimHelper.GetUserIdFromToken(User);
                if (userIdFromToken == null)
                    return Unauthorized(new { Error = "Online mode requires login." });

                dto.GameType = "online_multiplayer";
                dto.WhitePlayerId = userIdFromToken;

                int gameId = await _gameService.CreateGameAsync(dto);

                return Ok(new
                {
                    Message = "Game started successfully",
                    GameId = gameId,
                    Mode = dto.GameType,
                    MatchMode = dto.MatchMode == 1 ? "ranked" :
                                dto.MatchMode == 2 ? "normal" : null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // 2️ START GAME (OFFLINE)
        [HttpPost("start/offline")]
        public async Task<IActionResult> StartOfflineGame([FromBody] GameCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                if (string.Equals(dto.GameType, "online_multiplayer", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { Error = "Use online start for online games." });

                int? userIdFromToken = UserClaimHelper.GetUserIdFromToken(User);

                // Single / AI / Local
                dto.WhitePlayerId = userIdFromToken; // ถ้ามีก็เก็บ
                dto.MatchMode = null; // ไม่ใช้ ranked/normal

                int gameId = await _gameService.CreateGameAsync(dto);

                return Ok(new
                {
                    Message = "Game started successfully",
                    GameId = gameId,
                    Mode = dto.GameType,
                    MatchMode = 0 //  null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // 3️ START GAME (DEPRECATED)
        [HttpPost("start")]
        public IActionResult StartGameDeprecated()
        {
            return BadRequest(new
            {
                Error = "Use /api/Game/start/online or /api/Game/start/offline."
            });
        }

        // 4️ END GAME (ONLINE)
        [HttpPost("end/online")]
        public async Task<IActionResult> EndOnlineGame([FromBody] GameResultDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                int? userIdFromToken = UserClaimHelper.GetUserIdFromToken(User);
                if (userIdFromToken == null)
                    return Unauthorized(new { Error = "Online mode requires login." });

                var (found, gameType, gameStatus, whitePlayerId, blackPlayerId)
                    = await _gameService.GetEndGameInfoAsync(dto.GameId);

                if (!found)
                    return NotFound(new { Error = "Game not found." });

                if (!string.Equals(gameType, "online_multiplayer", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { Error = "Use offline end for non-online games." });

                if (gameStatus != "in_progress")
                    return BadRequest(new { Error = "Game not in progress." });

                if (whitePlayerId != userIdFromToken && blackPlayerId != userIdFromToken)
                    return Forbid();

                var (success, whiteRating, blackRating)
                    = await _gameService.FinalizeGameAsync(dto);

                if (!success)
                    return BadRequest(new { Error = "Game not found or already finished." });

                return Ok(new
                {
                    Message = "Game ended successfully",
                    WhiteRating = whiteRating,
                    BlackRating = blackRating
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // 5️ END GAME (OFFLINE)
        [HttpPost("end/offline")]
        public async Task<IActionResult> EndOfflineGame([FromBody] GameResultDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var (found, gameType, gameStatus, _, _)
                    = await _gameService.GetEndGameInfoAsync(dto.GameId);

                if (!found)
                    return NotFound(new { Error = "Game not found." });

                if (string.Equals(gameType, "online_multiplayer", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { Error = "Use online end for online games." });

                if (gameStatus != "in_progress")
                    return BadRequest(new { Error = "Game not in progress." });

                var (success, whiteRating, blackRating)
                    = await _gameService.FinalizeGameAsync(dto);

                if (!success)
                    return BadRequest(new { Error = "Game not found or already finished." });

                return Ok(new
                {
                    Message = "Game ended successfully",
                    WhiteRating = whiteRating,
                    BlackRating = blackRating
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // 6️ END GAME (DEPRECATED)
        [HttpPost("end")]
        public IActionResult EndGameDeprecated()
        {
            return BadRequest(new
            {
                Error = "Use /api/Game/end/online or /api/Game/end/offline."
            });
        }

        // 7️ RESIGN (ONLINE)
        [HttpPost("resign/online")]
        public async Task<IActionResult> ResignOnlineGame([FromBody] GameResignDto dto)
        {
            try
            {
                int? userIdFromToken = UserClaimHelper.GetUserIdFromToken(User);
                if (userIdFromToken == null)
                    return Unauthorized(new { Error = "Online mode requires login." });

                var (found, gameType, gameStatus, whitePlayerId, blackPlayerId)
                    = await _gameService.GetEndGameInfoAsync(dto.GameId);

                if (!found)
                    return NotFound(new { Error = "Game not found." });

                if (!string.Equals(gameType, "online_multiplayer", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { Error = "Use offline resign for non-online games." });

                if (gameStatus != "in_progress")
                    return BadRequest(new { Error = "Game not in progress." });

                if (whitePlayerId != userIdFromToken && blackPlayerId != userIdFromToken)
                    return Forbid();

                var (success, whiteRating, blackRating)
                    = await _gameService.ResignGameAsync(
                        dto.GameId,
                        userIdFromToken.Value,
                        dto.Reason
                    );

                if (!success)
                    return BadRequest(new { Error = "Resign failed." });

                return Ok(new
                {
                    Message = "Game resigned successfully",
                    WhiteRating = whiteRating,
                    BlackRating = blackRating
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // 8️ RESIGN (OFFLINE)
        [HttpPost("resign/offline")]
        public async Task<IActionResult> ResignOfflineGame([FromBody] GameResignDto dto)
        {
            try
            {
                var (found, gameType, gameStatus, _, _)
                    = await _gameService.GetEndGameInfoAsync(dto.GameId);

                if (!found)
                    return NotFound(new { Error = "Game not found." });

                if (string.Equals(gameType, "online_multiplayer", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { Error = "Use online resign for online games." });

                if (gameStatus != "in_progress")
                    return BadRequest(new { Error = "Game not in progress." });

                var (success, whiteRating, blackRating)
                    = await _gameService.ResignGameAsync(
                        dto.GameId,
                        dto.PlayerId,
                        dto.Reason
                    );

                if (!success)
                    return BadRequest(new { Error = "Resign failed." });

                return Ok(new
                {
                    Message = "Game resigned successfully",
                    WhiteRating = whiteRating,
                    BlackRating = blackRating
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // 9️ RESIGN (DEPRECATED)
        [HttpPost("resign")]
        public IActionResult ResignGameDeprecated()
        {
            return BadRequest(new
            {
                Error = "Use /api/Game/resign/online or /api/Game/resign/offline."
            });
        }

        // 10️ RESULT
        [HttpGet("result/{gameId}")]
        public async Task<IActionResult> GetGameResult(int gameId)
        {
            var result = await _gameService.GetGameResultAsync(gameId);

            if (result == null)
                return NotFound(new { Error = "Game not found." });

            return Ok(result);
        }

        // 11️ STATUS
        [HttpGet("status/{gameId}")]
        public async Task<IActionResult> GetGameStatus(int gameId)
        {
            var result = await _gameService.GetGameResultAsync(gameId);

            if (result == null)
                return NotFound(new { Error = "Game not found." });

            return Ok(new
            {
                GameId = result.GameId,
                GameType = result.GameType,
                MatchMode = result.MatchMode,
                Status = result.Result ?? "in_progress",
                MoveCount = result.MoveCount,
                CreatedAt = result.CreatedAt
            });
        }

    }

}
