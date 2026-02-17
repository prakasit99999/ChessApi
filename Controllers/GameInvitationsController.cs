using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChessApi.DbContext;
using ChessApi.Models;

namespace ChessApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameInvitationsController : ControllerBase
    {
        private readonly ChessDbContext _context;

        public GameInvitationsController(ChessDbContext context)
        {
            _context = context;
        }

        // POST: api/GameInvitations
        [HttpPost]
        public async Task<ActionResult<game_invitation>> CreateInvitation(CreateInvitationDto dto)
        {
            var invitation = new game_invitation
            {
                from_user_id = dto.from_user_id,
                to_user_id = dto.to_user_id,
                time_control_minutes = dto.time_control_minutes,
                message = dto.message,
                status = "pending",
                created_at = DateTime.UtcNow,
                expires_at = DateTime.UtcNow.AddHours(24) // Default expiry
            };

            _context.game_invitations.Add(invitation);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetInvitation), new { id = invitation.invitation_id }, invitation);
        }

        // GET: api/GameInvitations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<game_invitation>> GetInvitation(int id)
        {
            var invitation = await _context.game_invitations.FindAsync(id);

            if (invitation == null)
            {
                return NotFound();
            }

            return invitation;
        }

        // GET: api/GameInvitations/received/5
        [HttpGet("received/{userId}")]
        public async Task<ActionResult<IEnumerable<game_invitation>>> GetReceivedInvitations(int userId)
        {
            return await _context.game_invitations
                .Where(i => i.to_user_id == userId && i.status == "pending")
                .ToListAsync();
        }

        // POST: api/GameInvitations/5/accept
        [HttpPost("{id}/accept")]
        public async Task<IActionResult> AcceptInvitation(int id)
        {
            var invitation = await _context.game_invitations.FindAsync(id);

            if (invitation == null)
            {
                return NotFound();
            }

            if (invitation.status != "pending")
            {
                return BadRequest("Invitation is already " + invitation.status);
            }

            invitation.status = "accepted";
            invitation.responded_at = DateTime.UtcNow;

            // Create a new game
            var game = new game
            {
                white_player_id = invitation.from_user_id,
                black_player_id = invitation.to_user_id,
                game_type = "online_multiplayer", // Default for invitations
                white_player_type = "human",
                black_player_type = "human",
                game_status = "in_progress",
                current_turn = "white",
                time_control_minutes = invitation.time_control_minutes ?? 10,
                is_rated = true,
                created_at = DateTime.UtcNow,
                started_at = DateTime.UtcNow,
                move_count = 0
            };

            _context.games.Add(game);
            await _context.SaveChangesAsync();

            return Ok(new { invitation, game });
        }

        // POST: api/GameInvitations/5/decline
        [HttpPost("{id}/decline")]
        public async Task<IActionResult> DeclineInvitation(int id)
        {
            var invitation = await _context.game_invitations.FindAsync(id);

            if (invitation == null)
            {
                return NotFound();
            }

            if (invitation.status != "pending")
            {
                return BadRequest("Invitation is already " + invitation.status);
            }

            invitation.status = "declined";
            invitation.responded_at = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(invitation);
        }
    }

    public class CreateInvitationDto
    {
        public int from_user_id { get; set; }
        public int to_user_id { get; set; }
        public int? time_control_minutes { get; set; }
        public string? message { get; set; }
    }
}
