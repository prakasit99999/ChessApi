using Xunit;
using Microsoft.EntityFrameworkCore;
using ChessApi.Controllers;
using ChessApi.DbContext;
using ChessApi.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChessApi.Tests
{
    public class GameInvitationsControllerTests
    {
        private ChessDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ChessDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ChessDbContext(options);
        }

        [Fact]
        public async Task CreateInvitation_ReturnsCreatedAtAction()
        {
            // Arrange
            var context = GetDbContext();
            var controller = new GameInvitationsController(context);
            var dto = new CreateInvitationDto
            {
                from_user_id = 1,
                to_user_id = 2,
                time_control_minutes = 15,
                message = "Let's play!"
            };

            // Act
            var result = await controller.CreateInvitation(dto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var invitation = Assert.IsType<game_invitation>(createdAtActionResult.Value);
            Assert.Equal(1, invitation.from_user_id);
            Assert.Equal(2, invitation.to_user_id);
            Assert.Equal("pending", invitation.status);
        }

        [Fact]
        public async Task GetReceivedInvitations_ReturnsOnlyPendingForUser()
        {
            // Arrange
            var context = GetDbContext();
            context.game_invitations.AddRange(new List<game_invitation>
            {
                new game_invitation { invitation_id = 1, from_user_id = 1, to_user_id = 2, status = "pending" },
                new game_invitation { invitation_id = 2, from_user_id = 3, to_user_id = 2, status = "pending" },
                new game_invitation { invitation_id = 3, from_user_id = 1, to_user_id = 2, status = "accepted" },
                new game_invitation { invitation_id = 4, from_user_id = 2, to_user_id = 1, status = "pending" }
            });
            await context.SaveChangesAsync();

            var controller = new GameInvitationsController(context);

            // Act
            var result = await controller.GetReceivedInvitations(2);

            // Assert
            var invitations = Assert.IsAssignableFrom<IEnumerable<game_invitation>>(result.Value);
            Assert.Equal(2, invitations.Count());
            Assert.All(invitations, i => Assert.Equal(2, i.to_user_id));
            Assert.All(invitations, i => Assert.Equal("pending", i.status));
        }

        [Fact]
        public async Task AcceptInvitation_UpdatesStatusAndCreatesGame()
        {
            // Arrange
            var context = GetDbContext();
            var invitation = new game_invitation
            {
                invitation_id = 1,
                from_user_id = 1,
                to_user_id = 2,
                status = "pending",
                time_control_minutes = 10
            };
            context.game_invitations.Add(invitation);
            await context.SaveChangesAsync();

            var controller = new GameInvitationsController(context);

            // Act
            var result = await controller.AcceptInvitation(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Check invitation status
            var updatedInvitation = await context.game_invitations.FindAsync(1);
            Assert.Equal("accepted", updatedInvitation.status);
            Assert.NotNull(updatedInvitation.responded_at);

            // Check if game was created
            var game = await context.games.FirstOrDefaultAsync(g => g.white_player_id == 1 && g.black_player_id == 2);
            Assert.NotNull(game);
            Assert.Equal("in_progress", game.game_status);
            Assert.Equal(10, game.time_control_minutes);
        }

        [Fact]
        public async Task DeclineInvitation_UpdatesStatus()
        {
            // Arrange
            var context = GetDbContext();
            var invitation = new game_invitation { invitation_id = 1, from_user_id = 1, to_user_id = 2, status = "pending" };
            context.game_invitations.Add(invitation);
            await context.SaveChangesAsync();

            var controller = new GameInvitationsController(context);

            // Act
            var result = await controller.DeclineInvitation(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedInvitation = await context.game_invitations.FindAsync(1);
            Assert.Equal("declined", updatedInvitation.status);
            Assert.NotNull(updatedInvitation.responded_at);
        }
    }
}
