using ChessApi.DTOs.Game;

namespace ChessApi.Services.Interfaces
{
    public interface IMoveService
    {
        Task<int> LogMoveAsync(MoveDto moveDto);
        Task<MoveDto> AddMoveAsync(MoveDto moveDto);
        Task<List<MoveDto>> GetAllMovesAsync();
        Task<MoveDto> GetMoveByIdAsync(int id);
    }
}


