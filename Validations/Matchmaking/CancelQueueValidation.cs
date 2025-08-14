using ChessApi.DTOs.Matchmaking;

namespace ChessApi.Validations.Matchmaking
{
    public class CancelQueueValidation
    {
        public static List<string>  Validate(CancelQueueRequest request)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                errors.Add("Username cannot be null or empty.");
            }

            return errors;
        }

    }
}
