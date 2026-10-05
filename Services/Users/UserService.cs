using ChessApi.DbContext;
using ChessApi.DTOs.User;
using ChessApi.Hubs;
using ChessApi.Models;
using ChessApi.Services.Interfaces;
using ChessApi.Validations.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChessApi.Services.Users
{
    public class UserService : IUserService
    {
        private readonly ChessDbContext _context;
        private readonly IPasswordHasher<user> _passwordHasher;
        private readonly IHubContext<UserHub> _hubContext;

        public UserService(
            ChessDbContext context,
            IPasswordHasher<user> passwordHasher,
            IHubContext<UserHub> hubContext)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _hubContext = hubContext;
        }

        public async Task<ProfileResponse> GetProfileAsync(ProfileRequest request)
        {
            return await GetProfileByIdAsync(request.UserId);
        }

        public async Task<ProfileResponse> GetProfileByIdAsync(int userId)
        {
            var user = await _context.users.AsNoTracking().FirstOrDefaultAsync(u => u.user_id == userId);
            if (user == null)
            {
                return new ProfileResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            return MapToProfileResponse(user);
        }

        public async Task<UpdateProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            var validationErrors = UserValidation.ValidateUpdateProfile(request);
            if (validationErrors.Any())
            {
                return new UpdateProfileResponse
                {
                    Success = false,
                    Message = string.Join(", ", validationErrors)
                };
            }

            var user = await _context.users.FirstOrDefaultAsync(u => u.user_id == userId);
            if (user == null)
            {
                return new UpdateProfileResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            // Update Username if changed
            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                var newUsername = request.Username.Trim();
                if (!string.Equals(user.username, newUsername, StringComparison.OrdinalIgnoreCase))
                {
                    var usernameExists = await _context.users.AnyAsync(u => u.username == newUsername && u.user_id != userId);
                    if (usernameExists)
                    {
                        return new UpdateProfileResponse
                        {
                            Success = false,
                            Message = "Username already exists."
                        };
                    }
                    user.username = newUsername;
                }
            }

            // Update Email if changed
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var newEmail = request.Email.Trim();
                if (!string.Equals(user.email, newEmail, StringComparison.OrdinalIgnoreCase))
                {
                    var emailExists = await _context.users.AnyAsync(u => u.email == newEmail && u.user_id != userId);
                    if (emailExists)
                    {
                        return new UpdateProfileResponse
                        {
                            Success = false,
                            Message = "Email already exists."
                        };
                    }
                    user.email = newEmail;
                }
            }

            // Update Password if requested
            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.password_hash, request.CurrentPassword ?? string.Empty);
                if (verifyResult == PasswordVerificationResult.Failed)
                {
                    return new UpdateProfileResponse
                    {
                        Success = false,
                        Message = "Current password is incorrect."
                    };
                }

                user.password_hash = _passwordHasher.HashPassword(user, request.NewPassword);
            }

            _context.users.Update(user);
            await _context.SaveChangesAsync();

            return new UpdateProfileResponse
            {
                Success = true,
                Message = "Profile updated successfully.",
                Profile = MapToProfileResponse(user)
            };
        }

        public async Task<UpdateProfileResponse> AdminUpdateUserAsync(int userId, AdminUpdateUserRequest request)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.user_id == userId);
            if (user == null)
            {
                return new UpdateProfileResponse
                {
                    Success = false,
                    Message = $"User with ID {userId} not found."
                };
            }

            // Update Username if changed
            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                var newUsername = request.Username.Trim();
                if (!string.Equals(user.username, newUsername, StringComparison.OrdinalIgnoreCase))
                {
                    var usernameExists = await _context.users.AnyAsync(u => u.username == newUsername && u.user_id != userId);
                    if (usernameExists)
                    {
                        return new UpdateProfileResponse
                        {
                            Success = false,
                            Message = "Username already exists."
                        };
                    }
                    user.username = newUsername;
                }
            }

            // Update Email if changed
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var newEmail = request.Email.Trim();
                if (!string.Equals(user.email, newEmail, StringComparison.OrdinalIgnoreCase))
                {
                    var emailExists = await _context.users.AnyAsync(u => u.email == newEmail && u.user_id != userId);
                    if (emailExists)
                    {
                        return new UpdateProfileResponse
                        {
                            Success = false,
                            Message = "Email already exists."
                        };
                    }
                    user.email = newEmail;
                }
            }

            // Update Password if requested (Admin can reset password without old password)
            var newPassword = !string.IsNullOrWhiteSpace(request.NewPassword)
                ? request.NewPassword
                : request.Password;

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                user.password_hash = _passwordHasher.HashPassword(user, newPassword);
            }

            // Update Rating if provided
            if (request.Rating.HasValue)
            {
                user.rating = request.Rating.Value;
            }

            // Update Status if provided
            bool statusChanged = false;
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                var cleanStatus = request.Status.Trim().ToLowerInvariant();
                if (user.status != cleanStatus)
                {
                    user.status = cleanStatus;
                    statusChanged = true;
                }
            }

            _context.users.Update(user);
            await _context.SaveChangesAsync();

            if (statusChanged)
            {
                await BroadcastUserCountsAsync();
            }

            return new UpdateProfileResponse
            {
                Success = true,
                Message = $"User '{user.username}' updated successfully by Admin.",
                Profile = MapToProfileResponse(user)
            };
        }

        public async Task<bool> UpdateUserStatusAsync(int userId, string status)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.user_id == userId);
            if (user == null)
            {
                return false;
            }

            user.status = status;
            await _context.SaveChangesAsync();
            await BroadcastUserCountsAsync();
            return true;
        }

        public async Task<List<UserListDto>> GetAllPlayersAsync()
        {
            return await _context.users
                .AsNoTracking()
                .OrderBy(u => u.username)
                .Select(u => new UserListDto
                {
                    UserId = u.user_id,
                    Username = u.username,
                    Email = u.email,
                    Rating = u.rating ?? 1200,
                    Status = u.status ?? "offline"
                })
                .ToListAsync();
        }

        public async Task<List<UserListDto>> SearchPlayersAsync(string query)
        {
            var trimmedQuery = query.Trim();
            return await _context.users
                .AsNoTracking()
                .Where(u => u.username.Contains(trimmedQuery) || (u.email != null && u.email.Contains(trimmedQuery)))
                .OrderBy(u => u.username)
                .Select(u => new UserListDto
                {
                    UserId = u.user_id,
                    Username = u.username,
                    Email = u.email,
                    Rating = u.rating ?? 1200,
                    Status = u.status ?? "offline"
                })
                .ToListAsync();
        }

        public async Task<PagedResult<UserListDto>> GetPagedUsersAsync(UserPagedRequest request)
        {
            // ตัด Admin ออกก่อน (user_id == 1 หรือ username == "admin")
            var query = _context.users
                .AsNoTracking()
                .Where(u => u.user_id != 1 && u.username != "admin")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var trimmed = request.Search.Trim();
                query = query.Where(u => u.username.Contains(trimmed) || (u.email != null && u.email.Contains(trimmed)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(u => u.username)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(u => new UserListDto
                {
                    UserId = u.user_id,
                    Username = u.username,
                    Email = u.email,
                    Rating = u.rating ?? 1200,
                    Status = u.status ?? "offline"
                })
                .ToListAsync();

            return new PagedResult<UserListDto>
            {
                Items = items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<UserCountsDto> GetUserCountsAsync()
        {
            var totalUsers = await _context.users.CountAsync();

            // Online users (status is online or playing)
            var online = await _context.users
                .CountAsync(u => u.status != null && (u.status.ToLower() == "online" || u.status.ToLower() == "playing"));

            // Pending users (status is pending/waiting OR currently waiting in matchmaking queue)
            var dbPending = await _context.users
                .CountAsync(u => u.status != null && (u.status.ToLower() == "pending" || u.status.ToLower() == "waiting"));

            var queuePending = await _context.matchmaking_queues
                .Where(q => q.status == "waiting")
                .Select(q => q.user_id)
                .Distinct()
                .CountAsync();

            var pending = Math.Max(dbPending, queuePending);

            // Offline users (status is offline or null)
            var offline = await _context.users
                .CountAsync(u => u.status == null || u.status.ToLower() == "offline");

            // Consistency balance
            if (totalUsers > (online + offline + pending))
            {
                offline = Math.Max(0, totalUsers - online - pending);
            }

            return new UserCountsDto
            {
                TotalUsers = totalUsers,
                Online = online,
                Offline = offline,
                Pending = pending
            };
        }

        public async Task BroadcastUserCountsAsync()
        {
            var counts = await GetUserCountsAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveUserCounts", counts);
        }

        private static ProfileResponse MapToProfileResponse(user user, bool success = true, string message = "Profile retrieved successfully")
        {
            var gamesPlayed = user.games_played ?? 0;
            var gamesWon = user.games_won ?? 0;
            var winRate = gamesPlayed > 0 ? Math.Round((double)gamesWon / gamesPlayed * 100, 2) : 0;

            return new ProfileResponse
            {
                UserId = user.user_id,
                Username = user.username,
                Email = user.email,
                Rating = user.rating ?? 1200,
                GamesPlayed = gamesPlayed,
                GamesWon = gamesWon,
                GamesLost = user.games_lost ?? 0,
                GamesDrawn = user.games_drawn ?? 0,
                WinRate = winRate,
                Status = user.status ?? "offline",
                CreatedAt = user.created_at,
                Success = success,
                Message = message
            };
        }
    }
}

