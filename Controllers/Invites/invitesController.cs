using System.Security.Claims;
using ChessApi.DTOs.Invites;
using ChessApi.Services.Interfaces;
using ChessApi.Utilities.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChessApi.Controllers.Invites
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class InvitesController : ControllerBase
    {
        private readonly IInviteService _inviteService;

        public InvitesController(IInviteService inviteService)
        {
            _inviteService = inviteService;
        }

        // CREATE INVITE
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InviteRequestDto dto)
        {
            var userId = UserClaimHelper.GetUserIdFromToken(User);
            if (userId == null)
                return Unauthorized();

            dto.FromUserId = userId.Value;

            var result = await _inviteService.CreateAsync(dto);
            return Ok(result);
        }

        // INBOX
        [HttpGet("inbox")]
        public async Task<IActionResult> Inbox()
        {
            var userId = UserClaimHelper.GetUserIdFromToken(User);
            if (userId == null)
                return Unauthorized();

            var result = await _inviteService.GetInboxAsync(userId.Value);
            return Ok(result);
        }

        // SENT
        [HttpGet("sent")]
        public async Task<IActionResult> Sent()
        {
            var userId = UserClaimHelper.GetUserIdFromToken(User);
            if (userId == null)
                return Unauthorized();

            var result = await _inviteService.GetSentAsync(userId.Value);
            return Ok(result);
        }

        // ACCEPT
        [HttpPost("{inviteId}/accept")]
        public async Task<IActionResult> Accept(string inviteId)
        {
            var userId = UserClaimHelper.GetUserIdFromToken(User);
            if (userId == null)
                return Unauthorized();

            var result = await _inviteService.AcceptAsync(inviteId, userId.Value);
            if (result == null)
                return BadRequest(new { Error = "Invalid invite." });

            return Ok(result);
        }

        // DECLINE
        [HttpPost("{inviteId}/decline")]
        public async Task<IActionResult> Decline(string inviteId)
        {
            var userId = UserClaimHelper.GetUserIdFromToken(User);
            if (userId == null)
                return Unauthorized();

            var result = await _inviteService.DeclineAsync(inviteId, userId.Value);
            if (result == null)
                return BadRequest(new { Error = "Invalid invite." });

            return Ok(result);
        }

        // CANCEL
        [HttpPost("{inviteId}/cancel")]
        public async Task<IActionResult> Cancel(string inviteId)
        {
            var userId = UserClaimHelper.GetUserIdFromToken(User);
            if (userId == null)
                return Unauthorized();

            var result = await _inviteService.CancelAsync(inviteId, userId.Value);
            if (result == null)
                return BadRequest(new { Error = "Invalid invite." });

            return Ok(result);
        }

    }
}
