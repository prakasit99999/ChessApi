using ChessApi.DTOs.Game;

namespace ChessApi.Services.Interfaces
{
    public interface IGameService
    {
        Task<int> CreateGameAsync(GameCreateDto dto);
        Task<bool> FinalizeGameAsync(GameResultDto dto); // รวม EndGame + อัปเดตสถิติ
        Task<bool> ResignGameAsync(int gameId, int playerId, string reason);
        Task<GameResultDto?> GetGameResultAsync(int gameId);
    }
}
