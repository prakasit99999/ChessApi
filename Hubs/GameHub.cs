using Microsoft.AspNetCore.SignalR;

namespace ChessApi.Hubs
{
    public class GameHub: Hub
    {
        public async Task SendMove(string gameId, string move)
        {
            await Clients.Group(gameId).SendAsync("ReceiveMove", move);
        }

        public async Task JoinGame(string gameId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
            await Clients.Group(gameId).SendAsync("UserJoined", Context.ConnectionId);
        }

        public async Task LeaveGame(string gameId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);
            await Clients.Group(gameId).SendAsync("UserLeft", Context.ConnectionId);
        }
    }
}
