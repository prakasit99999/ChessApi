using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChessApi.DbContext;
using ChessApi.DTOs.Game;
using ChessApi.Models;
using ChessApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChessApi.Services.Game
{
    public class GameService : IGameService
    {
        private readonly ChessDbContext _context;

        public GameService(ChessDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateGameAsync(GameCreateDto dto)
        {
            int? whiteId = dto.WhitePlayerId;
            int? blackId = dto.BlackPlayerId;

            switch (dto.GameType.ToLower())
            {
                case "single_player":
                    if (whiteId <= 0) whiteId = null;
                    blackId = null;
                    break;
                case "ai_vs_ai":
                case "local_multiplayer":
                    whiteId = null;
                    blackId = null;
                    break;
                case "online_multiplayer":
                    if (whiteId <= 0 || blackId <= 0)
                        throw new Exception("Online Multiplayer requires valid User IDs.");
                    break;
                default:
                    if (whiteId <= 0) whiteId = null;
                    if (blackId <= 0) blackId = null;
                    break;
            }

            var newGame = new Models.game
            {
                game_type = dto.GameType,
                white_player_type = dto.WhitePlayerType,
                black_player_type = dto.BlackPlayerType,
                game_status = "in_progress",
                move_count = 0,
                created_at = DateTime.UtcNow,
                started_at = DateTime.UtcNow,
                white_player_id = whiteId,
                black_player_id = blackId,
                result = null,
                result_reason = null
            };

            _context.games.Add(newGame);
            await _context.SaveChangesAsync();
            return newGame.game_id;
        }

        //  2. End Game
        public async Task<bool> FinalizeGameAsync(GameResultDto dto)
        {
            var game = await GetGameWithPlayers(dto.GameId);
            if (game == null) return false;

            string rawResult = dto.Result?.ToLower() ?? "draw";

            // Update Game Status
            game.game_status = "finished";
            game.result = MapResultToEnum(rawResult);
            game.result_reason = dto.ResultReason?.ToLower();
            game.finished_at = DateTime.UtcNow;

            //  LOGIC ใหม่: อัปเดต Stats เฉพาะ Online Multiplayer และเกมไม่ Abandoned
            bool isOnline = game.game_type == "online_multiplayer";
            bool isAbandoned = (rawResult == "abandoned");

            if (isOnline && !isAbandoned)
            {
                UpdatePlayerStats(game, rawResult);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        //  3. Resign / Abort
        // ไฟล์: ChessApi.Services/Game/GameService.cs

        public async Task<bool> ResignGameAsync(int gameId, int playerId, string reason)
        {
            var game = await GetGameWithPlayers(gameId);

            // 1. เช็คว่ามีเกมและสถานะถูกต้อง
            if (game == null || game.game_status != "in_progress") return false;

            bool isWhite = (game.white_player_id == playerId) || (game.white_player_id == null && playerId <= 0);
            bool isBlack = (game.black_player_id == playerId) || (game.black_player_id == null && playerId <= 0);

            // ถ้าไม่ใช่ทั้งสีขาว และไม่ใช่ทั้งสีดำ ให้ถือว่าไม่มีสิทธิ์
            if (!isWhite && !isBlack) return false;

            // 2. กฎ: เดินน้อยกว่า 2 ตา = โมฆะ (Aborted Early)
            if (game.move_count < 2)
            {
                game.game_status = "abandoned";
                game.result = "abandoned";
                game.result_reason = "game_aborted_early";
                game.finished_at = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }

            // 3. หาผู้ชนะ (ถ้าคนกดออกคือสีขาว -> ดำชนะ)
            string winnerColor = "";
            if (isWhite) winnerColor = "black";
            else winnerColor = "white";

            // 4. บันทึกผล
            game.game_status = "finished";
            game.result = MapResultToEnum(winnerColor);

            // ✅ ใช้ค่า reason ที่ส่งมา (ถ้าไม่มีให้ใช้ resignation)
            game.result_reason = !string.IsNullOrEmpty(reason) ? reason : "resignation";

            game.finished_at = DateTime.UtcNow;

            if (game.game_type == "online_multiplayer")
            {
                UpdatePlayerStats(game, winnerColor);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<GameResultDto?> GetGameResultAsync(int gameID)
        {
            var game = await _context.games.FirstOrDefaultAsync(g => g.game_id == gameID);
            if (game == null) return null;

            return new GameResultDto
            {
                GameId = game.game_id,
                GameType = game.game_type,
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

        private async Task<game?> GetGameWithPlayers(int gameId)
        {
            return await _context.games
                .Include(g => g.white_player)
                .Include(g => g.black_player)
                .FirstOrDefaultAsync(g => g.game_id == gameId);
        }

        private string MapResultToEnum(string input)
        {
            return input switch
            {
                "white" => "white_wins",
                "black" => "black_wins",
                "draw" => "draw",
                "abandoned" => "abandoned",
                _ => "draw"
            };
        }

        private void UpdatePlayerStats(game game, string winner)
        {
            // กันเหนียวอีกชั้น
            if (game.game_type != "online_multiplayer") return;

            if (game.white_player != null)
            {
                game.white_player.games_played++;
                if (winner == "white") game.white_player.games_won++;
                else if (winner == "black") game.white_player.games_lost++;
                else game.white_player.games_drawn++;
            }

            if (game.black_player != null)
            {
                game.black_player.games_played++;
                if (winner == "black") game.black_player.games_won++;
                else if (winner == "white") game.black_player.games_lost++;
                else game.black_player.games_drawn++;
            }
        }
    }
}