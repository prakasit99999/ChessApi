using ChessApi.DTOs.Game;

namespace ChessApi.Services.Interfaces
{
    public interface IGameService
    {
        Task<bool> FinalizeGameAsync(GameResultDto dto); // รวม EndGame + อัปเดตสถิติ
        Task<GameResultDto?> GetGameResultAsync(int gameId);
    }
}
