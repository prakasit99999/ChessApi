using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChessApi.DbContext;
using ChessApi.DTOs.Game;
using ChessApi.DTOs.User;
using ChessApi.Hubs;
using ChessApi.Models;
using ChessApi.Services.Interfaces;
using ChessApi.Services.Move;
using ChessApi.Utilities.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChessApi.Services.Game
{
    public class GameService : IGameService
    {
        private readonly ChessDbContext _context;
        private readonly IRatingService _ratingService;
        private readonly IHubContext<GameHub> _gameHubContext;
        private readonly IHubContext<UserHub> _userHubContext;
        private static readonly ConcurrentDictionary<int, SemaphoreSlim> _locks = new();

        public GameService(
            ChessDbContext context, 
            IRatingService ratingService,
            IHubContext<GameHub> gameHubContext,
            IHubContext<UserHub> userHubContext)
        {
            _context = context;
            _ratingService = ratingService;
            _gameHubContext = gameHubContext;
            _userHubContext = userHubContext;
        }

        // 1️     CREATE GAME
        public async Task<int> CreateGameAsync(GameCreateDto dto)
        {
            int? whiteId = dto.WhitePlayerId;
            int? blackId = dto.BlackPlayerId;

            if (whiteId <= 0) whiteId = null;
            if (blackId <= 0) blackId = null;

            string status = "in_progress";

            switch (dto.GameType.ToLower())
            {
                case "online_multiplayer":
                    status = (whiteId.HasValue && blackId.HasValue)
                        ? "in_progress"
                        : "waiting_for_opponent";
                    break;

                case "single_player":
                case "ai_vs_ai":
                    status = "in_progress";
                    break;

                default:
                    status = "in_progress";
                    break;
            }

            var newGame = new game
            {
                game_type = dto.GameType,
                match_mode = dto.MatchMode == 1
                                ? "ranked"
                                : dto.MatchMode == 2
                                    ? "normal"
                                    : null,

                white_player_type = dto.WhitePlayerType,
                black_player_type = dto.BlackPlayerType,

                white_player_id = whiteId,
                black_player_id = blackId,

                game_status = status,
                move_count = 0,

                created_at = DateTime.UtcNow,
                started_at = status == "in_progress" ? DateTime.UtcNow : null,

                result = null,
                result_reason = null
            };

            _context.games.Add(newGame);

            if (dto.GameType.Equals("online_multiplayer", StringComparison.OrdinalIgnoreCase)
                && whiteId.HasValue
                && blackId.HasValue)
            {
                await SetUsersStatusByIdsAsync(new[] { whiteId.Value, blackId.Value }, "playing");
            }

            await _context.SaveChangesAsync();

            _ = BroadcastGameCountsAsync();

            return newGame.game_id;
        }

        // 2️ FINALIZE GAME
        public async Task<(bool Success, int? WhiteRating, int? BlackRating)> FinalizeGameAsync(GameResultDto dto)
        {

            var gameLock = _locks.GetOrAdd(dto.GameId, _ => new System.Threading.SemaphoreSlim(1, 1));
            await gameLock.WaitAsync();
            try
            {
                var game = await GetGameWithPlayers(dto.GameId);
                if (game == null) return (false, null, null);
                // กันยิงซ้ำ (Concurrency Guard)
                if (game.game_status != "in_progress")
                {
                    return (false, null, null);
                }

                using var tx = await _context.Database.BeginTransactionAsync();
                string rawResult = dto.Result?.ToLower() ?? "draw";

                game.game_status = "finished";
                game.result = MapResultToEnum.MapResultToString(rawResult);
                game.result_reason = dto.ResultReason;
                game.finished_at = DateTime.UtcNow;

                string winnerColor = rawResult switch
                {
                    "white_wins" => "white",
                    "black_wins" => "black",
                    "white" => "white",
                    "black" => "black",
                    _ => "draw"
                };

                UpdatePlayerStats(game, winnerColor);
                SetOnlinePlayersStatus(game, "online");

                await _context.SaveChangesAsync();

                // Rating เฉพาะ Ranked Online
                bool isRankedOnline =
                    game.game_type == "online_multiplayer" &&
                    game.match_mode == "ranked" &&
                    game.white_player_id.HasValue &&
                    game.black_player_id.HasValue &&
                    rawResult != "abandoned";

                if (isRankedOnline)
                {
                    await _ratingService.ProcessGameResultAsync(
                        game.white_player_id.Value,
                        game.black_player_id.Value,
                        winnerColor
                    );
                }

                await tx.CommitAsync();

                if (game.white_player != null)
                    await _context.Entry(game.white_player).ReloadAsync();

                if (game.black_player != null)
                    await _context.Entry(game.black_player).ReloadAsync();

                _ = BroadcastGameCountsAsync();

                return (true,
                    game.white_player?.rating,
                    game.black_player?.rating);

            }
            catch
            {
                return (false, null, null);
            }
            finally
            {
                gameLock.Release();
                _locks.TryRemove(dto.GameId, out _);
            }
        }

        // 3️ RESIGN / ABORT
        public async Task<(bool Success, int? WhiteRating, int? BlackRating)> ResignGameAsync(int gameId, int playerId, string reason)
        {
            var gameLock = _locks.GetOrAdd(gameId, _ => new System.Threading.SemaphoreSlim(1, 1));
            await gameLock.WaitAsync();
            try
            {
                var game = await GetGameWithPlayers(gameId);
                if (game == null || game.game_status != "in_progress")
                    return (false, null, null);

                bool isWhite = game.white_player_id == playerId;
                bool isBlack = game.black_player_id == playerId;

                if (!isWhite && !isBlack)
                    return (false, null, null);

                using var tx = await _context.Database.BeginTransactionAsync();

                // ถ้าเดินไม่ถึง 2 ตา → abandoned
                if (game.move_count < 2)
                {
                    game.game_status = "finished";
                    game.result = "abandoned";
                    game.result_reason = "game_aborted_early";
                    game.finished_at = DateTime.UtcNow;
                    SetOnlinePlayersStatus(game, "online");

                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();

                    return (true,
                        game.white_player?.rating,
                        game.black_player?.rating);
                }

                string winnerColor = isWhite ? "black" : "white";

                game.game_status = "finished";
                game.result = MapResultToEnum.MapResultToString(winnerColor);
                game.result_reason = string.IsNullOrEmpty(reason)
                                        ? "resignation"
                                        : reason;
                game.finished_at = DateTime.UtcNow;

                UpdatePlayerStats(game, winnerColor);
                SetOnlinePlayersStatus(game, "online");

                await _context.SaveChangesAsync();

                if (
                    game.game_type == "online_multiplayer" &&
                    game.match_mode == "ranked" &&
                    game.white_player_id.HasValue &&
                    game.black_player_id.HasValue
                )
                {
                    await _ratingService.ProcessGameResultAsync(
                        game.white_player_id.Value,
                        game.black_player_id.Value,
                        winnerColor
                    );
                }

                await tx.CommitAsync();

                if (game.white_player != null)
                    await _context.Entry(game.white_player).ReloadAsync();

                if (game.black_player != null)
                    await _context.Entry(game.black_player).ReloadAsync();

                _ = BroadcastGameCountsAsync();

                return (true,
                    game.white_player?.rating,
                    game.black_player?.rating);
            }
            catch
            {
                return (false, null, null);
            }
            finally
            {
                gameLock.Release();
                _locks.TryRemove(gameId, out _);
            }
        }
        // 4️ GET RESULT
        public async Task<GameResultDto?> GetGameResultAsync(int gameID)
        {
            var game = await _context.games
                .FirstOrDefaultAsync(g => g.game_id == gameID);

            if (game == null) return null;

            return new GameResultDto
            {
                GameId = game.game_id,
                GameType = game.game_type,
                MatchMode = game.match_mode,
                WhitePlayerType = game.white_player_type,
                BlackPlayerType = game.black_player_type,
                Result = game.result,
                ResultReason = game.result_reason,
                MoveCount = game.move_count,
                CreatedAt = game.created_at,
                StartedAt = game.started_at,
                FinishedAt = game.finished_at
            };
        }

        // 5️ GET END GAME INFO (for permission checks)
        public async Task<(bool Found, string? GameType, string? GameStatus, int? WhitePlayerId, int? BlackPlayerId)>
            GetEndGameInfoAsync(int gameId)
        {
            var game = await _context.games
                .AsNoTracking()
                .Where(g => g.game_id == gameId)
                .Select(g => new
                {
                    g.game_type,
                    g.game_status,
                    g.white_player_id,
                    g.black_player_id
                })
                .FirstOrDefaultAsync();

            if (game == null)
                return (false, null, null, null, null);

            return (true,
                game.game_type,
                game.game_status,
                game.white_player_id,
                game.black_player_id);
        }

        // PRIVATE METHODS

        private async Task<game?> GetGameWithPlayers(int gameId)
        {
            return await _context.games
                .Include(g => g.white_player)
                .Include(g => g.black_player)
                .FirstOrDefaultAsync(g => g.game_id == gameId);
        }



        private void UpdatePlayerStats(game game, string winner)
        {
            if (game.game_type != "online_multiplayer") return;

            if (game.white_player != null)
            {
                game.white_player.games_played++;

                if (winner == "white")
                    game.white_player.games_won++;
                else if (winner == "black")
                    game.white_player.games_lost++;
                else
                    game.white_player.games_drawn++;
            }

            if (game.black_player != null)
            {
                game.black_player.games_played++;

                if (winner == "black")
                    game.black_player.games_won++;
                else if (winner == "white")
                    game.black_player.games_lost++;
                else
                    game.black_player.games_drawn++;
            }
        }

        private static void SetOnlinePlayersStatus(game game, string status)
        {
            if (game.game_type != "online_multiplayer") return;

            if (game.white_player != null)
                game.white_player.status = status;

            if (game.black_player != null)
                game.black_player.status = status;
        }

        private async Task SetUsersStatusByIdsAsync(IEnumerable<int> userIds, string status)
        {
            var players = await _context.users
                .Where(u => userIds.Contains(u.user_id))
                .ToListAsync();

            foreach (var player in players)
                player.status = status;
        }

        // =========================================================
        // Game Table (ตาราง Game พร้อม Pagination & Filters)
        // =========================================================
        public async Task<PagedResult<GameListDto>> GetPagedGamesAsync(GamePagedRequest request)
        {
            var query = _context.games
                .AsNoTracking()
                .Include(g => g.white_player)
                .Include(g => g.black_player)
                .AsQueryable();

            if (request.UserId.HasValue && request.UserId > 0)
            {
                query = query.Where(g => g.white_player_id == request.UserId.Value || g.black_player_id == request.UserId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.GameType))
            {
                var cleanType = request.GameType.Trim().ToLowerInvariant();
                query = query.Where(g => g.game_type == cleanType);
            }

            if (!string.IsNullOrWhiteSpace(request.MatchMode))
            {
                var cleanMode = request.MatchMode.Trim().ToLowerInvariant();
                query = query.Where(g => g.match_mode == cleanMode);
            }

            if (!string.IsNullOrWhiteSpace(request.GameStatus))
            {
                var cleanStatus = request.GameStatus.Trim().ToLowerInvariant();
                query = query.Where(g => g.game_status == cleanStatus);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim().ToLowerInvariant();
                query = query.Where(g =>
                    (g.white_player != null && g.white_player.username.ToLower().Contains(term)) ||
                    (g.black_player != null && g.black_player.username.ToLower().Contains(term)) ||
                    (g.result_reason != null && g.result_reason.ToLower().Contains(term)) ||
                    g.game_id.ToString().Contains(term));
            }

            var totalCount = await query.CountAsync();

            var pagedGames = await query
                .OrderByDescending(g => g.created_at)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var items = pagedGames.Select(MapToGameListDto).ToList();

            return new PagedResult<GameListDto>
            {
                Items = items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }

        // =========================================================
        // User Games History (ประวัติเกมของ User คนนั้นๆ)
        // =========================================================
        public async Task<List<GameListDto>> GetUserGamesAsync(int userId, int limit = 20)
        {
            if (limit <= 0) limit = 20;
            if (limit > 100) limit = 100;

            var games = await _context.games
                .AsNoTracking()
                .Include(g => g.white_player)
                .Include(g => g.black_player)
                .Where(g => g.white_player_id == userId || g.black_player_id == userId)
                .OrderByDescending(g => g.created_at)
                .Take(limit)
                .ToListAsync();

            return games.Select(MapToGameListDto).ToList();
        }

        // =========================================================
        // Game Detail with Moves (รายละเอียดเกม + รายการตาเดิน)
        // =========================================================
        public async Task<GameDetailDto?> GetGameDetailAsync(int gameId)
        {
            var game = await _context.games
                .AsNoTracking()
                .Include(g => g.white_player)
                .Include(g => g.black_player)
                .Include(g => g.moves)
                .FirstOrDefaultAsync(g => g.game_id == gameId);

            if (game == null)
                return null;

            var baseDto = MapToGameListDto(game);

            return new GameDetailDto
            {
                GameId = baseDto.GameId,
                GameType = baseDto.GameType,
                MatchMode = baseDto.MatchMode,
                GameStatus = baseDto.GameStatus,
                Result = baseDto.Result,
                ResultReason = baseDto.ResultReason,
                MoveCount = baseDto.MoveCount,
                WhitePlayerId = baseDto.WhitePlayerId,
                WhitePlayerUsername = baseDto.WhitePlayerUsername,
                WhitePlayerRating = baseDto.WhitePlayerRating,
                WhitePlayerType = baseDto.WhitePlayerType,
                BlackPlayerId = baseDto.BlackPlayerId,
                BlackPlayerUsername = baseDto.BlackPlayerUsername,
                BlackPlayerRating = baseDto.BlackPlayerRating,
                BlackPlayerType = baseDto.BlackPlayerType,
                Winner = baseDto.Winner,
                CreatedAt = baseDto.CreatedAt,
                StartedAt = baseDto.StartedAt,
                FinishedAt = baseDto.FinishedAt,
                Moves = game.moves
                    .OrderBy(m => m.move_number)
                    .Select(MoveService.MapToMoveDetailDto)
                    .ToList()
            };
        }

        public static GameListDto MapToGameListDto(game g)
        {
            return new GameListDto
            {
                GameId = g.game_id,
                GameType = g.game_type,
                MatchMode = g.match_mode,
                GameStatus = g.game_status ?? "unknown",
                Result = g.result,
                ResultReason = g.result_reason,
                MoveCount = g.move_count ?? 0,

                WhitePlayerId = g.white_player_id,
                WhitePlayerUsername = g.white_player?.username,
                WhitePlayerRating = g.white_player?.rating,
                WhitePlayerType = g.white_player_type,

                BlackPlayerId = g.black_player_id,
                BlackPlayerUsername = g.black_player?.username,
                BlackPlayerRating = g.black_player?.rating,
                BlackPlayerType = g.black_player_type,

                Winner = g.result switch
                {
                    "white_wins" => "White",
                    "black_wins" => "Black",
                    "draw" => "Draw",
                    _ => null
                },

                CreatedAt = g.created_at,
                StartedAt = g.started_at,
                FinishedAt = g.finished_at
            };
        }

        //=========================================================
        // GetGameDetailAdminAsync Details for  Admin see uses playe game 
        //========================================================= 
        public async Task<GameDetails?> GetGameDetailAdminAsync(int gameId)
        {
            if(gameId <= 0) {
                return new GameDetails
                {
                    Success = false,
                    Message = "Game ID must be greater than 0"
                };
            };
            var game = await _context.games
                .AsNoTracking()
                .Include(g => g.white_player)
                .Include(g => g.black_player)
                .Include(g => g.ai_performances)
                .FirstOrDefaultAsync(g => g.game_id == gameId);

            if (game == null){
                return new GameDetails
                {
                    Success = false,
                    Message = "Game not found"
                };
            }

            // ดึงข้อมูล Move ทั้งหมดที่ game_id ตรงกับ game นั้น (ดึงรายละเอียดแบบเดียวกับ GetMoveByIdAsync ครบทุก field)
            var moves = await _context.moves
                .AsNoTracking()
                .Where(m => m.game_id == gameId)
                .OrderBy(m => m.move_number)
                .ToListAsync();

            var moveDetails = moves.Select(MoveService.MapToMoveDetailDto).ToList();

            var aiPerf = game.ai_performances?.FirstOrDefault();
            var lastAiMove = moves.LastOrDefault(m =>
                m.ai_evaluation_score.HasValue ||
                m.ai_depth_searched.HasValue ||
                m.ai_nodes_evaluated.HasValue);

            AiPerformanceDto? aiPerformance = null;
            bool isAiGame = (game.game_type != null && (game.game_type == "single_player" || game.game_type == "ai_vs_ai"))
                || (game.white_player_type != null && game.white_player_type.StartsWith("ai", StringComparison.OrdinalIgnoreCase))
                || (game.black_player_type != null && game.black_player_type.StartsWith("ai", StringComparison.OrdinalIgnoreCase));

            if (aiPerf != null || lastAiMove != null || isAiGame)
            {
                aiPerformance = new AiPerformanceDto
                {
                    AlgorithmType = aiPerf?.algorithm_type ?? lastAiMove?.algorithm_type,
                    EvaluationScore = lastAiMove?.ai_evaluation_score != null ? (float)lastAiMove.ai_evaluation_score : null,
                    AiDepthSearched = (int?)(aiPerf?.average_depth) ?? lastAiMove?.ai_depth_searched,
                    AiNodesEvaluated = aiPerf?.average_nodes_evaluated ?? lastAiMove?.ai_nodes_evaluated,
                    AiMoveTimeMs = aiPerf?.average_move_time_ms ?? lastAiMove?.move_time_ms
                };
            }

            return new GameDetails
            {
                GameId = game.game_id,
                GameType = game.game_type,
                MatchMode = game.match_mode,
                GameStatus = game.game_status,
                Result = game.result,
                ResultReason = game.result_reason,
                WhitePlayer = new PlayerDto
                {
                    UserId = game.white_player_id ?? 0,
                    Username = game.white_player?.username,
                    Rating = game.white_player?.rating ?? 0,
                    PlayerType = game.white_player_type
                },
                BlackPlayer = new PlayerDto
                {
                    UserId = game.black_player_id ?? 0,
                    Username = game.black_player?.username,
                    Rating = game.black_player?.rating ?? 0,
                    PlayerType = game.black_player_type
                },
                AiPerformance = aiPerformance,
                Moves = moveDetails
            };
        }

        // =========================================================
        // Game Counts & Statistics (สถิติเกมทั้งหมดสำหรับ REST API & SignalR)
        // =========================================================
        public async Task<GameCountsDto> GetGameCountsAsync()
        {
            // Total Games: นับจำนวน Row ทั้งหมดในตาราง game
            var totalGames = await _context.games.CountAsync();

            // Active Matches: เช็กสถานะ game_status == "in_progress" หรือ "active"
            var activeMatches = await _context.games
                .CountAsync(g => g.game_status == "in_progress" || g.game_status == "active");

            // AI Matches: จำนวนเกมที่มี AI ร่วมเล่น เช็ก game_type เป็น "single_player", "ai_vs_ai" หรือประเภทผู้เล่นขึ้นต้นด้วย "ai"
            var aiMatches = await _context.games
                .CountAsync(g => g.game_type == "single_player"
                              || g.game_type == "ai_vs_ai"
                              || (g.white_player_type != null && g.white_player_type.StartsWith("ai"))
                              || (g.black_player_type != null && g.black_player_type.StartsWith("ai")));

            // Ranked Games: เช็กโหมดการแข่งขัน match_mode == "ranked"
            var rankedGames = await _context.games
                .CountAsync(g => g.match_mode == "ranked");

            return new GameCountsDto
            {
                TotalGames = totalGames,
                ActiveMatches = activeMatches,
                AiMatches = aiMatches,
                RankedGames = rankedGames
            };
        }

        public async Task BroadcastGameCountsAsync()
        {
            var counts = await GetGameCountsAsync();
            await _gameHubContext.Clients.All.SendAsync("ReceiveGameCounts", counts);
            await _userHubContext.Clients.All.SendAsync("ReceiveGameCounts", counts);
        }

        // =========================================================
        // Chart Stats (สถิติแยกตามวัน สำหรับแสดงกราฟ)
        // =========================================================
        public async Task<List<GameChartDataDto>> GetChartStatsAsync(GameChartRequest request)
        {
            // กำหนดช่วงวันที่
            var endDate = (request.EndDate ?? DateTime.UtcNow).Date.AddDays(1); // ถึงสิ้นสุดวันนั้น
            var startDate = request.StartDate.HasValue
                ? request.StartDate.Value.Date
                : DateTime.UtcNow.Date.AddDays(-(request.Days - 1));

            // ดึงเกมทั้งหมดในช่วงวันที่กำหนด
            var games = await _context.games
                .AsNoTracking()
                .Where(g => g.created_at != null
                         && g.created_at.Value >= startDate
                         && g.created_at.Value < endDate)
                .Select(g => new
                {
                    Date = g.created_at!.Value.Date,
                    g.game_type,
                    g.white_player_type,
                    g.black_player_type
                })
                .ToListAsync();

            // GroupBy วัน
            var grouped = games
                .GroupBy(g => g.Date)
                .Select(grp =>
                {
                    // AI Games: game_type เป็น single_player, ai_vs_ai หรือ player_type ขึ้นต้นด้วย "ai"
                    var aiGames = grp.Count(g =>
                        g.game_type == "single_player"
                        || g.game_type == "ai_vs_ai"
                        || (g.white_player_type != null && g.white_player_type.StartsWith("ai"))
                        || (g.black_player_type != null && g.black_player_type.StartsWith("ai")));

                    // Multiplayer: online_multiplayer, local_multiplayer
                    var multiplayerGames = grp.Count(g =>
                        g.game_type == "online_multiplayer"
                        || g.game_type == "local_multiplayer");

                    return new GameChartDataDto
                    {
                        DateLabel = grp.Key.ToString("dd/MM/yyyy"),
                        DateIso   = grp.Key.ToString("yyyy-MM-dd"),
                        AiGames          = aiGames,
                        MultiplayerGames = multiplayerGames,
                        TotalGames       = grp.Count()
                    };
                })
                .OrderBy(d => d.DateIso)
                .ToList();

            // เติมวันที่ขาดหายให้ครบทุกวันในช่วง (เพื่อให้กราฟแสดงแกน X ต่อเนื่อง)
            var result = new List<GameChartDataDto>();
            for (var day = startDate; day < endDate; day = day.AddDays(1))
            {
                var existing = grouped.FirstOrDefault(d => d.DateIso == day.ToString("yyyy-MM-dd"));
                result.Add(existing ?? new GameChartDataDto
                {
                    DateLabel        = day.ToString("dd/MM/yyyy"),
                    DateIso          = day.ToString("yyyy-MM-dd"),
                    AiGames          = 0,
                    MultiplayerGames = 0,
                    TotalGames       = 0
                });
            }

            return result;
        }
    }
}

