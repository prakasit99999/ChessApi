using ChessApi.DTOs.Game;
using ChessApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
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

        //  1. Start Game: เพิ่มการ catch Error เพื่อส่ง Message จาก Service กลับไป
        [HttpPost("start")]
        public async Task<IActionResult> StartGame([FromBody] GameCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // เรียก Service สร้างเกม
                var gameId = await _gameService.CreateGameAsync(dto);

                // ส่ง gameId กลับไปเป็น JSON
                return Ok(new 
                { 
                    Message = "Game started successfully", 
                    GameId = gameId 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        //  2. End Game: ปรับ Response ให้เป็น JSON มาตรฐาน
        [HttpPost("end")]
        public async Task<IActionResult> EndGame([FromBody] GameResultDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var success = await _gameService.FinalizeGameAsync(dto);

                if (!success)
                {
                    return BadRequest(new { Error = "Game ID not found or update failed." });
                }

                return Ok(new { Message = "Game ended and stats updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
       
        //  3. Resign / Abort: ปรับ Response ให้เป็น JSON มาตรฐาน
        [HttpPost("resign")]
        public async Task<IActionResult> ResignGame([FromBody] GameResignDto dto)
        {
            try
            {
                var result = await _gameService.ResignGameAsync(dto.GameId, dto.PlayerId, dto.Reason);
                
                if (result)
                {
                    // ✅ ปรับเป็น JSON เพื่อให้ Unity อ่านง่าย
                    return Ok(new { Message = "Game resigned/aborted successfully." });
                }
                else
                {
                    return BadRequest(new { Error = "Failed to resign game. Game may not exist or is already finished." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        //  3. Get Result: ดึงผลสรุปเกม
        [HttpGet("result/{gameId}")]
        public async Task<IActionResult> GetGameResult(int gameId)
        {
            var gameResult = await _gameService.GetGameResultAsync(gameId);
            
            if (gameResult == null)
            {
                return NotFound(new { Error = "Game result not found." });
            }

            return Ok(gameResult);
        }

        //  4. Get Status: (เผื่อใช้เช็คสถานะระหว่างเกม)
        [HttpGet("status/{gameId}")]
        public async Task<IActionResult> GetGameStatus(int gameId)
        {
            var gameResult = await _gameService.GetGameResultAsync(gameId);
            
            if (gameResult == null)
            {
                return NotFound(new { Error = "Game status not found." });
            }

            return Ok(new
            {
                GameId = gameResult.GameId,
                GameType = gameResult.GameType,
                Status = gameResult.Result ?? "In Progress", // ถ้ายังไม่จบ Result จะเป็น null
                MoveCount = gameResult.MoveCount,
                CreatedAt = gameResult.CreatedAt
            });
        }
    }
}