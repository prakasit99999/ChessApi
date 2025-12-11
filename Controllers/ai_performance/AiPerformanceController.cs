using ChessApi.DTOs.AiPerformance;
using ChessApi.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ChessApi.Controllers.ai_performance
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiPerformanceController : ControllerBase
    {
        private readonly IAiPerformance _aiPerformanceService;
        public AiPerformanceController(IAiPerformance aiPerformanceService)
        {
            _aiPerformanceService = aiPerformanceService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AiPerformanceCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newId = await _aiPerformanceService.CreateAiPerformanceAsync(dto);

            return Ok(new
            {
                Message = "AI performance record created successfully",
                PerformanceId = newId
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var aiPerformance = await _aiPerformanceService.GetAiPerformanceAsync(id);
            if (aiPerformance == null)
                return NotFound(new { Message = "AI performance record not found" });
            return Ok(aiPerformance);
        }
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] AiPerformanceUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                await _aiPerformanceService.UpdateAiPerformanceAsync(dto);
                return Ok(new { Message = "AI performance record updated successfully" });
            }
            catch (System.Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }
    }
}