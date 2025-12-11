//using ChessApi.DbContext;
using ChessApi.DTOs.Auth;

namespace ChessApi.Validations.Auth
{
    public class RegisteVaildation
    {
        //private readonly ChessDbContext _context;
        //public RegisteVaildation(ChessDbContext context)
        //{
        //    _context = context;
        //}

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
            if (!request.Email.Contains("@"))
            {
                errors.Add("Email must be a valid email address.");
            }
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                errors.Add("Password cannot be null or empty.");
            }
            if (request.Password.Length < 10)
            {
                errors.Add("Password must be at least 10 characters long.");
            }
            
            return errors;
        }
    }
}
