using ChessApi.DTOs.Game;
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
        private readonly IMoveService _moveService;

        public GameController(IGameService gameService, IMoveService moveService)
        {
            _gameService = gameService;
            _moveService = moveService;
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

        // 12️ GET PAGED GAMES (ตาราง Game สำหรับแสดงผลใน Frontend)
        [HttpGet("paged")]
        [HttpGet("list")]
        [HttpGet]
        public async Task<IActionResult> GetPagedGames([FromQuery] GamePagedRequest request)
        {
            var result = await _gameService.GetPagedGamesAsync(request);
            return Ok(result);
        }

        // 13️ GET USER GAMES (ประวัติเกมของ User)
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetUserGames([FromRoute] int userId, [FromQuery] int limit = 20)
        {
            if (userId <= 0)
                return BadRequest(new { Error = "Invalid user ID." });

            var result = await _gameService.GetUserGamesAsync(userId, limit);
            return Ok(result);
        }

        // 14️ GET GAME DETAIL WITH MOVES (รายละเอียดเกมพร้อมตาเดินทั้งหมด)
        [HttpGet("{gameId:int}")]
        [HttpGet("detail/{gameId:int}")]
        public async Task<IActionResult> GetGameDetail([FromRoute] int gameId)
        {
            if (gameId <= 0)
                return BadRequest(new { Error = "Invalid game ID." });

            var result = await _gameService.GetGameDetailAsync(gameId);
            if (result == null)
                return NotFound(new { Error = "Game not found." });

            return Ok(result);
        }

        // 15️ GET GAME MOVES (รายละเอียด Move ทั้งหมดในเกม)
        [HttpGet("{gameId:int}/moves")]
        public async Task<IActionResult> GetGameMoves([FromRoute] int gameId)
        {
            if (gameId <= 0)
                return BadRequest(new { Error = "Invalid game ID." });

            var moves = await _moveService.GetGameMovesAsync(gameId);
            return Ok(moves);
        }

        // 16️ GET GAME COUNTS & STATS (API แยกต่างหากสำหรับสถิติตาราง Game)
        /// <summary>
        /// ดึงสถิติตาราง Game ทั้งหมด:
        /// - Total Games: จำนวนแถวทั้งหมดในตาราง game
        /// - Active Matches: สถานะ in_progress หรือ active
        /// - AI Matches: game_type เป็น single_player, ai_vs_ai หรือมีผู้เล่นเป็น ai
        /// - Ranked Games: match_mode == "ranked"
        /// (สามารถเรียกผ่าน REST API หรือผ่าน SignalR ที่ /gamehub หรือ /userhub Event "ReceiveGameCounts")
        /// </summary>
        [HttpGet("counts")]
        [HttpGet("stats")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> GetGameCounts()
        {
            var counts = await _gameService.GetGameCountsAsync();
            return Ok(counts);
        }

        // 17️ GET GAME CHART STATS (สถิติแยกตามวัน สำหรับแสดงกราฟเส้น)
        /// <summary>
        /// ดึงข้อมูลสถิติเกมแยกตามวัน สำหรับใช้วาดกราฟแสดงแนวโน้ม
        /// - AiGames: จำนวนเกมกับ AI ในวันนั้น (single_player, ai_vs_ai)
        /// - MultiplayerGames: จำนวนเกม PvP ในวันนั้น (online_multiplayer, local_multiplayer)
        /// - TotalGames: รวมทั้งหมดในวันนั้น
        /// Query Params: days=30 (ย้อนหลัง 30 วัน) หรือ startDate=2026-09-01&endDate=2026-10-04
        /// </summary>
        [HttpGet("chart-stats")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> GetChartStats([FromQuery] GameChartRequest request)
        {
            var data = await _gameService.GetChartStatsAsync(request);
            return Ok(data);
        }

        // 18️ GET GAME DETAIL ADMIN (รายละเอียดเกมแบบละเอียด พร้อม Player, AI Performance และ Moves สำหรับ Frontend)
        /// <summary>
        /// ดึงข้อมูลรายละเอียดของเกมสำหรับ Admin หรือหน้าแสดงรายละเอียดเกม:
        /// - ข้อมูลเกม (GameId, GameType, MatchMode, GameStatus, Result, ResultReason)
        /// - ผู้เล่น WhitePlayer & BlackPlayer (UserId, Username, Rating, PlayerType)
        /// - AiPerformance (AlgorithmType, EvaluationScore, AiDepthSearched, AiNodesEvaluated, AiMoveTimeMs)
        /// - Moves (รายการตาเดินทั้งหมดของเกม พร้อมรายละเอียดพิกัดและสถิติ AI)
        /// </summary>
        [HttpGet("admin/{gameId:int}")]
        [HttpGet("admin-detail/{gameId:int}")]
        public async Task<IActionResult> GetGameDetailAdmin([FromRoute] int gameId)
        {
            if (gameId <= 0)
                return BadRequest(new { Error = "Invalid game ID." });

            var result = await _gameService.GetGameDetailAdminAsync(gameId);
            if (result == null)
                return NotFound(new { Error = "Game not found." });

            return Ok(result);
        }
    }
}
