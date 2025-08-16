using ChessApi.DbContext;
using ChessApi.DTOs.Auth;

namespace ChessApi.Validations.Auth
{
    public class LoginVaildation
    {

        public static List<String> Validate(LoginRequest request)
        {

            var errors = new List<string>();
            if (request == null)
            {
                errors.Add("Request cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                errors.Add("Email cannot be null or empty.");
            }
            if(!request.Email.Contains("@"))
            {
                errors.Add("Email must be a valid email address.");
            }
            if (string.IsNullOrWhiteSpace(request.PasswordHash))
            {
                errors.Add("Password cannot be null or empty.");
            }
            return errors;
        }

    }
}
