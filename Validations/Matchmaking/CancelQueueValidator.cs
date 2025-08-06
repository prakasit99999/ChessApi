using ChessApi.DTOs.Matchmaking;

namespace ChessApi.Validations.Matchmaking
{
    public class CancelQueueValidator
    {
        public static void Validate(CancelQueueRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                throw new ArgumentException("Username cannot be null or empty.", nameof(request.Username));
            }
            // Additional validation logic can be added here if needed
        }

    }
}
