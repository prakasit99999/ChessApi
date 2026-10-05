using System;
using System.Threading.Tasks;
using ChessApi.DTOs.Game;
using ChessApi.Services.Interfaces;
using ChessApi.Utilities.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace ChessApi.Controllers.Move
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoveController : ControllerBase
    {
        private readonly IMoveService _moveService;

        public MoveController(IMoveService moveService)
        {
            _moveService = moveService;
        }

        // 1️ Make Single Move
        [HttpPost]
        public async Task<IActionResult> MakeMove([FromBody] MoveDto.MoveRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                int? userId = UserClaimHelper.GetUserIdFromToken(User);

                var success = await _moveService.MakeMoveAsync(request, userId);

                if (!success)
                {
                    return BadRequest(new
                    {
                        Error = "Move not allowed. Invalid game, unauthorized, or game not in progress."
                    });
                }

                return Ok(new { Message = "Move recorded successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // 2️ Make Batch Moves local (AI Simulation)
        [HttpPost("batch")]
        public async Task<IActionResult> MakeMovesBatch([FromBody] MoveDto.MoveBatchRequest wrapper)
        {
            if (wrapper == null || wrapper.moves == null || wrapper.moves.Count == 0)
                return BadRequest("No moves provided.");

            int? userId = UserClaimHelper.GetUserIdFromToken(User);

            int successCount = 0;
            int failCount = 0;

            foreach (var req in wrapper.moves)
            {
                bool isSuccess = await _moveService.MakeMoveAsync(req, userId);

                if (isSuccess) successCount++;
                else failCount++;
            }

            return Ok(new
            {
                Message = "Batch process completed",
                Total = wrapper.moves.Count,
                Success = successCount,
                Failed = failCount
            });
        }

        // 3️ Get Latest Move (Polling Support)
        [HttpGet("latest/{gameId}")]
        public async Task<IActionResult> GetLatestMove(int gameId)
        {
            var result = await _moveService.GetLatestMoveAsync(gameId);

            if (result == null)
                return Ok(null); // เกมเพิ่งเริ่ม ไม่มี move

            return Ok(result);
        }

        // 4️ Get All Moves for Game (รายละเอียด Move ทั้งหมดในเกม)
        [HttpGet("game/{gameId:int}")]
        [HttpGet("list/{gameId:int}")]
        public async Task<IActionResult> GetGameMoves([FromRoute] int gameId)
        {
            if (gameId <= 0)
                return BadRequest(new { Error = "Invalid game ID." });

            var moves = await _moveService.GetGameMovesAsync(gameId);
            return Ok(moves);
        }

        // 5️ Get Single Move Detail (รายละเอียด Move เดี่ยว)
        [HttpGet("{moveId:int}")]
        [HttpGet("detail/{moveId:int}")]
        public async Task<IActionResult> GetMoveDetail([FromRoute] int moveId)
        {
            if (moveId <= 0)
                return BadRequest(new { Error = "Invalid move ID." });

            var move = await _moveService.GetMoveByIdAsync(moveId);
            if (move == null)
                return NotFound(new { Error = "Move not found." });

            return Ok(move);
        }
    }
}
