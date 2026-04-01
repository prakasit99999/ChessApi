using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChessApi.DbContext;
using ChessApi.DTOs.Game;
using ChessApi.DTOs.Invites;
using ChessApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChessApi.Services.Invites
{
    public class InviteService : IInviteService
    {
        private readonly ChessDbContext _context;
        private readonly IGameService _gameService;

        public InviteService(ChessDbContext context, IGameService gameService)
        {
            _context = context;
            _gameService = gameService;
        }

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

        // CREATE INVITE
        public async Task<InviteResponseDto> CreateAsync(InviteRequestDto dto)
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

            return await ToDtoAsync(entry);
        }

        // ACCEPT INVITE ( สร้างเกมตรงนี้)
        public async Task<InviteResponseDto?> AcceptAsync(string inviteId, int userId)
        {
            if (!_invites.TryGetValue(inviteId, out var entry))
                return null;

            bool expired;
            lock (entry)
            {
                if (entry.ToUserId != userId)
                    return null;

                if (entry.Status != "pending")
                    return null;

                expired = IsExpired(entry);
                entry.Status = expired ? "expired" : "accepted";
            }

            if (expired)
                return await ToDtoAsync(entry);

            // 🔥 สร้างเกมแบบ "เริ่มเล่นทันที"
            var gameId = await _gameService.CreateGameAsync(new GameCreateDto
            {
                GameType = "online_multiplayer",
                MatchMode = 2,
                WhitePlayerType = "human",
                BlackPlayerType = "human",
                WhitePlayerId = entry.FromUserId,
                BlackPlayerId = entry.ToUserId
            });

            entry.GameId = gameId;

            return await ToDtoAsync(entry);
        }


        // DECLINE
        public async Task<InviteResponseDto?> DeclineAsync(string inviteId, int userId)
        {
            if (!_invites.TryGetValue(inviteId, out var entry))
                return null;

            if (entry.ToUserId != userId)
                return null;

            if (IsExpired(entry))
            {
                entry.Status = "expired";
                return await ToDtoAsync(entry);
            }

            if (entry.Status != "pending")
                return null;

            entry.Status = "declined";
            return await ToDtoAsync(entry);
        }

        // CANCEL
        public async Task<InviteResponseDto?> CancelAsync(string inviteId, int userId)
        {
            if (!_invites.TryGetValue(inviteId, out var entry))
                return null;

            if (entry.FromUserId != userId)
                return null;

            if (entry.Status != "pending")
                return null;

            entry.Status = "canceled";
            return await ToDtoAsync(entry);
        }

        // INBOX
        public async Task<IReadOnlyList<InviteResponseDto>> GetInboxAsync(int userId)
        {
            var entries = _invites.Values
                .Where(i => i.ToUserId == userId)
                .ToList();

            var list = await ToDtoListAsync(entries);
            return list;
        }

        // SENT
        public async Task<IReadOnlyList<InviteResponseDto>> GetSentAsync(int userId)
        {
            var entries = _invites.Values
                .Where(i => i.FromUserId == userId)
                .ToList();

            var list = await ToDtoListAsync(entries);
            return list;
        }

        // HELPERS
        private static bool IsExpired(InviteEntry entry)
            => DateTime.UtcNow > entry.ExpiresAt;

        private async Task<InviteResponseDto> ToDtoAsync(InviteEntry entry)
        {
            var ids = new[] { entry.FromUserId, entry.ToUserId };
            var usernames = await _context.users
                .Where(u => ids.Contains(u.user_id))
                .Select(u => new { u.user_id, u.username })
                .ToDictionaryAsync(u => u.user_id, u => u.username);

            return ToDto(entry, usernames);
        }

        private async Task<IReadOnlyList<InviteResponseDto>> ToDtoListAsync(List<InviteEntry> entries)
        {
            if (entries.Count == 0)
                return new List<InviteResponseDto>();

            var userIds = entries
                .SelectMany(e => new[] { e.FromUserId, e.ToUserId })
                .Distinct()
                .ToList();

            var usernames = await _context.users
                .Where(u => userIds.Contains(u.user_id))
                .Select(u => new { u.user_id, u.username })
                .ToDictionaryAsync(u => u.user_id, u => u.username);

            return entries.Select(e => ToDto(e, usernames)).ToList();
        }

        private static InviteResponseDto ToDto(InviteEntry entry, Dictionary<int, string> usernames)
        {
            return new InviteResponseDto
            {
                InviteId = entry.Id,
                FromUserId = entry.FromUserId,
                FromUsername = usernames.TryGetValue(entry.FromUserId, out var fromUsername) ? fromUsername : null,
                ToUserId = entry.ToUserId,
                ToUsername = usernames.TryGetValue(entry.ToUserId, out var toUsername) ? toUsername : null,
                Status = entry.Status,
                CreatedAt = entry.CreatedAt,
                ExpiresAt = entry.ExpiresAt,
                GameId = entry.GameId
            };
        }
    }
}
