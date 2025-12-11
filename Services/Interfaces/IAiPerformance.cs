using ChessApi.DTOs.AiPerformance;
using ChessApi.DTOs.Auth;

namespace ChessApi.Services.Interfaces
{
    public interface IAiPerformance
    {
     Task<int> CreateAiPerformanceAsync(AiPerformanceCreateDto dto);
    Task<AiPerformanceDto?> GetAiPerformanceAsync(int performanceId);
    Task UpdateAiPerformanceAsync(AiPerformanceUpdateDto dto);
    }
}