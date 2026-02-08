using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChessApi.DTOs.Invites;
using ChessApi.Services.Interfaces;
 
namespace ChessApi.Services.Invites
{
    public class InviteService : IInviteService
    {
        private sealed class InviteEntry
        {
            public string Id { get; init; } = "";
            public int FromUserId { get; init; }
            public int ToUserId { get; init; }
            public string Status { get; set; } = "pending";
            public DateTime CreatedAt { get; init; }
            public DateTime ExpiresAt { get; init; }
            public int? GameId { get; set; }
        }

        private static readonly ConcurrentDictionary<string, InviteEntry> _invites = new();

        public Task<InviteResponseDto> CreateAsync(InviteRequestDto dto)
        {
            var id = Guid.NewGuid().ToString("N");
            var now = DateTime.UtcNow;
            var ttl = dto.ExpiresInSeconds.GetValueOrDefault(300);
            if (ttl < 30) ttl = 30;
            if (ttl > 3600) ttl = 3600;

            var entry = new InviteEntry
            {
                Id = id,
                FromUserId = dto.FromUserId,
                ToUserId = dto.ToUserId,
                Status = "pending",
                CreatedAt = now,
                ExpiresAt = now.AddSeconds(ttl)
            };

            _invites[id] = entry;

            return Task.FromResult(ToDto(entry));
        }

        public Task<InviteResponseDto?> AcceptAsync(string inviteId, int userId)
        {
            if (!_invites.TryGetValue(inviteId, out var entry))
                return Task.FromResult<InviteResponseDto?>(null);

            if (entry.ToUserId != userId) return Task.FromResult<InviteResponseDto?>(null);
            if (IsExpired(entry))
            {
                entry.Status = "expired";
                return Task.FromResult<InviteResponseDto?>(ToDto(entry));
            }
            if (entry.Status != "pending") return Task.FromResult<InviteResponseDto?>(null);

            entry.Status = "accepted";
            return Task.FromResult<InviteResponseDto?>(ToDto(entry));
        }

        public Task<InviteResponseDto?> DeclineAsync(string inviteId, int userId)
        {
            if (!_invites.TryGetValue(inviteId, out var entry))
                return Task.FromResult<InviteResponseDto?>(null);

            if (entry.ToUserId != userId) return Task.FromResult<InviteResponseDto?>(null);
            if (IsExpired(entry))
            {
                entry.Status = "expired";
                return Task.FromResult<InviteResponseDto?>(ToDto(entry));
            }
            if (entry.Status != "pending") return Task.FromResult<InviteResponseDto?>(null);

            entry.Status = "declined";
            return Task.FromResult<InviteResponseDto?>(ToDto(entry));
        }

        public Task<InviteResponseDto?> CancelAsync(string inviteId, int userId)
        {
            if (!_invites.TryGetValue(inviteId, out var entry))
                return Task.FromResult<InviteResponseDto?>(null);

            if (entry.FromUserId != userId) return Task.FromResult<InviteResponseDto?>(null);
            if (entry.Status != "pending") return Task.FromResult<InviteResponseDto?>(null);

            entry.Status = "canceled";
            return Task.FromResult<InviteResponseDto?>(ToDto(entry));
        }

        public Task<IReadOnlyList<InviteResponseDto>> GetInboxAsync(int userId)
        {
            var list = _invites.Values
                .Where(i => i.ToUserId == userId)
                .Select(ToDto)
                .ToList();
            return Task.FromResult<IReadOnlyList<InviteResponseDto>>(list);
        }

        public Task<IReadOnlyList<InviteResponseDto>> GetSentAsync(int userId)
        {
            var list = _invites.Values
                .Where(i => i.FromUserId == userId)
                .Select(ToDto)
                .ToList();
            return Task.FromResult<IReadOnlyList<InviteResponseDto>>(list);
        }

        private static bool IsExpired(InviteEntry entry) => DateTime.UtcNow > entry.ExpiresAt;

        private static InviteResponseDto ToDto(InviteEntry entry)
        {
            return new InviteResponseDto
            {
                InviteId = entry.Id,
                FromUserId = entry.FromUserId,
                ToUserId = entry.ToUserId,
                Status = entry.Status,
                CreatedAt = entry.CreatedAt,
                ExpiresAt = entry.ExpiresAt,
                GameId = entry.GameId
            };
        }
    }
}
