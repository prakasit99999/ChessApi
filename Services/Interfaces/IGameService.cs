using ChessApi.DTOs.Game;

namespace ChessApi.Services.Interfaces
{
    public interface IGameService
    {
        Task<int> CreateGameAsync(GameCreateDto dto);
        Task<(bool Success, int? WhiteRating, int? BlackRating)> FinalizeGameAsync(GameResultDto dto); // รวม EndGame + อัปเดตสถิติ
        Task<(bool Success, int? WhiteRating, int? BlackRating)> ResignGameAsync(int gameId, int playerId, string reason);
        Task<GameResultDto?> GetGameResultAsync(int gameId);
        Task<(bool Found, string? GameType, string? GameStatus, int? WhitePlayerId, int? BlackPlayerId)> GetEndGameInfoAsync(int gameId);
    }
}
