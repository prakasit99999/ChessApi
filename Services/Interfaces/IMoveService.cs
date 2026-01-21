using ChessApi.DTOs.Game;

namespace ChessApi.Services.Interfaces
{
    public interface IMoveService
    {
        Task<bool> MakeMoveAsync(MoveDto.MoveRequest dto);
        Task<MoveDto.LatestMoveResponseDto?> GetLatestMoveAsync(int gameId);
    }
}
