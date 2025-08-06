using ChessApi.DTOs.Matchmaking;

namespace ChessApi.Validations.Matchmaking
{
    public class JoinQueueValidator
    {
        public static void Validate(JoinQueueRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                throw new ArgumentException("Username cannot be null or empty.", nameof(request.Username));
            }
            if (request.MinRating < 0 || request.MaxRating < 0)
            {
                throw new ArgumentException("Ratings must be non-negative.", nameof(request));
            }
            if (request.MinRating > request.MaxRating)
            {
                throw new ArgumentException("MinRating cannot be greater than MaxRating.", nameof(request));
            }
            if (request.PreferredTimeControl <= 0)
            {
                throw new ArgumentException("PreferredTimeControl must be a positive number.", nameof(request.PreferredTimeControl));
            }
        }
    }
}
