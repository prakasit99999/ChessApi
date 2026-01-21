﻿using ChessApi.DTOs.Game;
using ChessApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static ChessApi.DTOs.Game.MoveDto;

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

        [HttpPost]
        public async Task<IActionResult> MakeMove([FromBody] MoveDto.MoveRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var success = await _moveService.MakeMoveAsync(request);

                if (!success)
                {
                    // ถ้าบันทึกไม่ได้ (เช่น GameId ผิด หรือเกมจบแล้ว) จะแจ้ง Error ชัดเจน
                    return BadRequest("Cannot make move. Game ID not found or game is not in progress.");
                }

                return Ok(new { Message = "Move recorded successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // ✅ แก้ไขฟังก์ชันนี้: เช็คผลลัพธ์ Success/Fail ตามจริง
        [HttpPost("batch")]
        public async Task<IActionResult> MakeMovesBatch([FromBody] MoveBatchRequest wrapper) // ✅ รับเป็น Wrapper
        {
            // แกะกล่องเอา moves ออกมาเช็ค
            if (wrapper == null || wrapper.moves == null || wrapper.moves.Count == 0)
            {
                return BadRequest("No moves provided.");
            }

            var requests = wrapper.moves; // ✅ เอา List ข้างในมาใช้
            int successCount = 0;
            int failCount = 0;

            foreach (var req in requests)
            {
                bool isSuccess = await _moveService.MakeMoveAsync(req);
                if (isSuccess) successCount++;
                else failCount++;
            }

            return Ok(new
            {
                Message = "Batch process completed",
                Total = requests.Count,
                Success = successCount,
                Failed = failCount
            });
        }

        [HttpGet("latest/{gameId}")]
        public async Task<IActionResult> GetLatestMove(int gameId)
        {
            var result = await _moveService.GetLatestMoveAsync(gameId);
            if (result == null)
            {
                return NotFound(new { Message = "No moves found for this game." });
            }
            return Ok(result);
        }
    }
}