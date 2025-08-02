using ChessApi.DTOs.Game;
using ChessApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChessApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoveController : ControllerBase
    {
        private readonly IMoveService _moveService;

        public MoveController(IMoveService moveService)
        {
            _moveService = moveService;
        }

        [HttpPost("log")]
        public async Task<IActionResult> LogMove([FromBody] MoveDto moveDto)
        {
            if (moveDto == null) return BadRequest("Move data is required");

            var moveId = await _moveService.LogMoveAsync(moveDto);
            return Ok(new { MoveId = moveId });
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddMove([FromBody] MoveDto moveDto)
        {
            var result = await _moveService.AddMoveAsync(moveDto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var moves = await _moveService.GetAllMovesAsync();
            return Ok(moves);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var move = await _moveService.GetMoveByIdAsync(id);
            if (move == null) return NotFound();

            return Ok(move);
        }
    }
}
