using ChessApi.DbContext;
using ChessApi.DTOs.Auth;

namespace ChessApi.Validations.Auth
{
    public class RegisteVaildation
    {
        private readonly ChessDbContext _context;
        public RegisteVaildation(ChessDbContext context)
        {
            _context = context;
        }

        public static List<string> Validate(RegisterRequest request)
        {
            var errors = new List<string>();
      
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                errors.Add("Username cannot be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                errors.Add("Email cannot be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                errors.Add("Password cannot be null or empty.");
            }
            if (request.Password.Length < 6)
            {
                errors.Add("Password must be at least 6 characters long.");
            }
            return errors;
        }
    }
}
