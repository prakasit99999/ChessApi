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
                return errors;
            }
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                errors.Add("Email cannot be null or empty.");
            }
            
            if (string.IsNullOrWhiteSpace(request.PasswordHash))
            {
                errors.Add("Password cannot be null or empty.");
            }
            return errors;
        }

    }
}
