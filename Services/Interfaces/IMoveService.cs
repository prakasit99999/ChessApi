using ChessApi.DTOs.Game;

namespace ChessApi.Services.Interfaces
{
    public interface IMoveService
    {
        Task<bool> MakeMoveAsync(MoveDto.MoveRequest dto , int? userId = null);
        Task<MoveDto.LatestMoveResponseDto?> GetLatestMoveAsync(int gameId);
        Task<List<MoveDto.MoveDetailDto>> GetGameMovesAsync(int gameId);
        Task<MoveDto.MoveDetailDto?> GetMoveByIdAsync(int moveId);
    }
}
