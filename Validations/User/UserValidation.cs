using ChessApi.DTOs.User;

namespace ChessApi.Validations.User
{
    public class UserValidation
    {
        public static List<string> ValidateUpdateProfile(UpdateProfileRequest request)
        {
            var errors = new List<string>();

            if (request.Username != null)
            {
                var username = request.Username.Trim();
                if (string.IsNullOrWhiteSpace(username))
                {
                    errors.Add("Username cannot be empty.");
                }
                else if (username.Length < 3 || username.Length > 50)
                {
                    errors.Add("Username must be between 3 and 50 characters.");
                }
            }

            if (request.Email != null)
            {
                var email = request.Email.Trim();
                if (string.IsNullOrWhiteSpace(email))
                {
                    errors.Add("Email cannot be empty.");
                }
                else if (!email.Contains("@") || !email.Contains("."))
                {
                    errors.Add("Email must be a valid email address.");
                }
                else if (email.Length > 100)
                {
                    errors.Add("Email must not exceed 100 characters.");
                }
            }

            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(request.CurrentPassword))
                {
                    errors.Add("Current password is required to set a new password.");
                }
                if (request.NewPassword.Length < 6)
                {
                    errors.Add("New password must be at least 6 characters long.");
                }
            }

            return errors;
        }
    }
}
