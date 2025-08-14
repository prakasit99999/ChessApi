using ChessApi.DTOs.Matchmaking;

namespace ChessApi.Validations.Matchmaking
{
    public class JoinQueueValidation
    {
        public static List<string> Validate(JoinQueueRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.Username))
            {
                errors.Add("Username cannot be null or empty.");
            }
            if (request.MinRating < 0 || request.MaxRating < 0)
            {
                errors.Add("MinRating and MaxRating must be non-negative numbers.");
            }
            if (request.MinRating > request.MaxRating)
            {
                errors.Add("MinRating cannot be greater than MaxRating.");
            }
            if (request.PreferredTimeControl <= 0)
            {
                errors.Add("PreferredTimeControl must be a positive number.");
            }
            return errors;
        }
    }
}
