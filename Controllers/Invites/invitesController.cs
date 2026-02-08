using System.Threading.Tasks;
using ChessApi.DTOs.Invites;
using ChessApi.Services.Invites;
using Microsoft.AspNetCore.Mvc;

namespace ChessApi.Controllers.Invites
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvitesController : ControllerBase
    {
        private readonly IInviteService _inviteService;

        public InvitesController(IInviteService inviteService)
        {
            _inviteService = inviteService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InviteRequestDto dto)
        {
            var result = await _inviteService.CreateAsync(dto);
            return Ok(result);
        }

        [HttpGet("inbox/{userId}")]
        public async Task<IActionResult> Inbox(int userId)
        {
            var result = await _inviteService.GetInboxAsync(userId);
            return Ok(result);
        }

        [HttpGet("sent/{userId}")]
        public async Task<IActionResult> Sent(int userId)
        {
            var result = await _inviteService.GetSentAsync(userId);
            return Ok(result);
        }

        [HttpPost("{inviteId}/accept")]
        public async Task<IActionResult> Accept(string inviteId, [FromBody] InviteActionDto dto)
        {
            var result = await _inviteService.AcceptAsync(inviteId, dto.UserId);
            if (result == null) return BadRequest(new { Error = "Invalid invite." });
            return Ok(result);
        }

        [HttpPost("{inviteId}/decline")]
        public async Task<IActionResult> Decline(string inviteId, [FromBody] InviteActionDto dto)
        {
            var result = await _inviteService.DeclineAsync(inviteId, dto.UserId);
            if (result == null) return BadRequest(new { Error = "Invalid invite." });
            return Ok(result);
        }

        [HttpPost("{inviteId}/cancel")]
        public async Task<IActionResult> Cancel(string inviteId, [FromBody] InviteActionDto dto)
        {
            var result = await _inviteService.CancelAsync(inviteId, dto.UserId);
            if (result == null) return BadRequest(new { Error = "Invalid invite." });
            return Ok(result);
        }
    }
}
