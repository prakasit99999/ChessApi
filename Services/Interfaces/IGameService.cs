using ChessApi.DTOs.Game;

namespace ChessApi.Services.Interfaces
{
    public interface IGameService
    {
        void StartGame();
        void EndGame(int gameId, string result, string? reason = null);
        Task<GameResultDto?> GetGameResultAsync(int gameId);
    }
}
