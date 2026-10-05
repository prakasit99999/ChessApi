namespace ChessApi.DTOs.User
{
    public class ProfileRequest
    {
        public int UserId { get; set; }
    }

    public class ProfileResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int Rating { get; set; }
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public int GamesDrawn { get; set; }
        public double WinRate { get; set; }
        public string Status { get; set; } = "offline"; // online, offline, playing
        public DateTime? CreatedAt { get; set; }
        public bool Success { get; set; } = false;
        public string? Message { get; set; } = null;
    }

    public class UpdateProfileRequest
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }

    public class AdminUpdateUserRequest
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? NewPassword { get; set; }
        public string? Password { get; set; }
        public int? Rating { get; set; }
        public string? Status { get; set; }
    }

    public class UpdateProfileResponse
    {
        public bool Success { get; set; } = false;
        public string? Message { get; set; } = null;
        public ProfileResponse? Profile { get; set; }
    }

    public class UpdateStatusRequest
    {
        public int UserId { get; set; }
        public string Status { get; set; } = "offline"; // online, offline, playing
    }

    public class UpdateStatusResponse
    {
        public bool Success { get; set; } = false;
        public string? Message { get; set; } = null;
    }

    public class UserListDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int Rating { get; set; }
        public string Status { get; set; } = "offline";
    }

    public class UserPagedRequest
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 10;
        private int _pageNumber = 1;

        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 10 : value);
        }

        public string? Search { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    public class UserCountsDto
    {
        public int TotalUsers { get; set; }
        public int Online { get; set; }
        public int Offline { get; set; }
        public int Pending { get; set; }
    }
}

