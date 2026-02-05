using System.Threading.Tasks;

namespace ChessApi.Services.Interfaces
{
    public interface IRatingService
    {
        Task ProcessGameResultAsync(int whitePlayerId, int blackPlayerId, string winnerColor);
    }
}