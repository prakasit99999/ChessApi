using System.Security.Claims;

namespace ChessApi.Utilities.Helpers
{
    public static class UserClaimHelper
    {
        public static int? GetUserIdFromToken(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            var idClaim =
                user.FindFirst("id")?.Value ??
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(idClaim, out int id))
                return id;

            return null;
        }

        public static string? GetUsernameFromToken(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            var usernameClaim =
                user.FindFirst("username")?.Value ??
                user.FindFirst(ClaimTypes.Name)?.Value;

            return usernameClaim;
        }
    }
}
