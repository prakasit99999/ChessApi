﻿using ChessApi.DTOs.Game;
using ChessApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
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
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                // TODO: SECURITY CHECK (ดักจับเรื่อง Token )
                // ดึง User ID จาก Token (ถ้ามี)
                int? userIdFromToken = null;
                if (User.Identity.IsAuthenticated)
                {
                    var idClaim = User.FindFirst("id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(idClaim, out int id))
                    {
                        userIdFromToken = id;
                    }
                }

                // กรณี 1: ถ้าเป็น Online Multiplayer -> บังคับต้องมี Token
                if (dto.GameType == "online_multiplayer")
                {
                    if (userIdFromToken == null)
                    {
                        return Unauthorized(new { Error = "Online mode requires login (Token is missing or invalid)." });
                    }

                    // Auto-fill ID จาก Token เพื่อความชัวร์ และป้องกันการสวมรอย
                    dto.WhitePlayerId = userIdFromToken;
                }
                // กรณี 2: ถ้าเป็น Single Player / Local -> ถ้ามี Token ก็ใส่ ID ให้ ถ้าไม่มีก็เป็น null
                else
                {

                    // ถ้าไม่ได้ล็อกอิน -> เล่นแบบ Guest
                    dto.WhitePlayerId = null;
                    // โหมด Offline ไม่ต้องมี BlackPlayerId (AI หรือ Local)
                    dto.BlackPlayerId = null;
                }

                var gameId = await _gameService.CreateGameAsync(dto);

                return Ok(new
                {
                    Message = "Game started successfully",
                    GameId = gameId,
                    Mode = dto.GameType
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
                Status = gameResult.Result ?? "in_progress", // ถ้ายังไม่จบ Result จะเป็น null
                MoveCount = gameResult.MoveCount,
                CreatedAt = gameResult.CreatedAt
            });
        }
    }
}