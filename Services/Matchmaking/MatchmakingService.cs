using ChessApi.DbContext;
using ChessApi.DTOs.Matchmaking;
using ChessApi.Models; // Model ที่คุณสร้าง (user, game, matchmaking_queue)
using ChessApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChessApi.Services.Matchmaking
{
    public class MatchmakingService : IMatchmakingService
    {
        private readonly ChessDbContext _context;

        public MatchmakingService(ChessDbContext context)
        {
            _context = context;
        }

        public async Task JoinQueueAsync(JoinQueueDTOs request)
        {
            // ค้นหา User (ใช้ชื่อตัวแปรตาม Model ของคุณ: username ตัวเล็ก)
            var user = await _context.users.FirstOrDefaultAsync(u => u.username == request.Username);
            if (user == null) throw new Exception("User not found");

            // ลบคิวเก่าที่ยังค้างอยู่ (ถ้ามี)
            var oldQueue = await _context.matchmaking_queues
                .FirstOrDefaultAsync(q => q.user_id == user.user_id && q.status == "waiting");

            if (oldQueue != null)
            {
                oldQueue.status = "cancelled";
            }

            // สร้างคิวใหม่
            var queueItem = new matchmaking_queue
            {
                user_id = user.user_id,
                min_rating = request.MinRating,
                max_rating = request.MaxRating,
                status = "waiting",
                joined_at = DateTime.Now
            };

            _context.matchmaking_queues.Add(queueItem);
            await _context.SaveChangesAsync();
        }

        public async Task CancelQueueAsync(CancelQueueDTOs request)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.username == request.Username);
            if (user == null) return;

            var queueItem = await _context.matchmaking_queues
                .FirstOrDefaultAsync(q => q.user_id == user.user_id && q.status == "waiting");

            if (queueItem != null)
            {
                queueItem.status = "cancelled";
                await _context.SaveChangesAsync();
            }
        }

  
        public async Task<MatchFoundDTOs?> CheckForMatchAsync(string username)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.username == username);
            if (user == null) return null;

            // --- STEP A: เช็คว่าเรา "ถูกจับคู่" ไปแล้วหรือยัง? ---
            // (กรณีที่เราเป็นคนรอ แล้วมีคนอื่นมาจับคู่กับเราไปแล้ว)
            var existingGame = await _context.games
                .Include(g => g.white_player)
                .Include(g => g.black_player)
                .FirstOrDefaultAsync(g =>
                    (g.white_player_id == user.user_id || g.black_player_id == user.user_id)
                    && g.game_status == "waiting_for_connection");

            if (existingGame != null)
            {
                // เจอเกมที่สร้างไว้แล้ว!
                bool isWhite = existingGame.white_player_id == user.user_id;

                return new MatchFoundDTOs
                {
                    GameId = existingGame.game_id,

                    // ใช้ Game ID เป็น Room Code แทน (แปลงเป็น String)
                    RoomCode = existingGame.game_id.ToString(),

                    OpponentUsername = isWhite ? existingGame.black_player.username : existingGame.white_player.username,
                    Color = isWhite ? "white" : "black",
                    TimeControlMinutes = 10,
                    GameType = existingGame.game_type
                };
            }

            // --- STEP B: ถ้ายังไม่มีเกม ให้ลอง "ค้นหาคู่" ในคิว ---

            // 1. ดึงคิวของเรา
            var myQueue = await _context.matchmaking_queues
                .FirstOrDefaultAsync(q => q.user_id == user.user_id && q.status == "waiting");

            if (myQueue == null) return null; // ไม่ได้ต่อคิว

            // 2. ค้นหาคู่แข่ง (Opponent)
            var opponentQueue = await _context.matchmaking_queues
                .Include(q => q.user)
                .Where(q => q.status == "waiting" && q.user_id != user.user_id) // ไม่ใช่ตัวเอง
                                                                                // เช็ค Rating (Elo) ว่าเหมาะสมกันไหม
                .Where(q => q.user.rating >= (myQueue.min_rating ?? 0) && q.user.rating <= (myQueue.max_rating ?? 3000))
                .Where(q => user.rating >= (q.min_rating ?? 0) && user.rating <= (q.max_rating ?? 3000))
                .OrderBy(q => q.joined_at) // มาก่อนได้จับคู่ก่อน
                .FirstOrDefaultAsync();

            if (opponentQueue != null)
            {
                // --- เจอคู่! สร้างเกมใหม่ (Create Match) ---

                var newGame = new game
                {
                    // ไม่ใส่ room_code เพราะใน DB ไม่มี

                    white_player_id = user.user_id, // ผู้ที่เรียก API เป็นสีขาว
                    black_player_id = opponentQueue.user_id, // ผู้ที่รออยู่ เป็นสีดำ

                    // ** ใส่ค่า ENUM ให้ครบตาม Model game.cs เพื่อป้องกัน Error **
                    game_type = "online_multiplayer",
                    white_player_type = "human",
                    black_player_type = "human",
                    game_status = "waiting_for_connection",

                    created_at = DateTime.Now,
                    move_count = 0
                };

                _context.games.Add(newGame);

                // Save ครั้งที่ 1 เพื่อให้ Database Gen ค่า game_id ออกมา
                await _context.SaveChangesAsync();

                // อัปเดตสถานะคิวของทั้งคู่
                myQueue.status = "matched";
                opponentQueue.status = "matched";
                await _context.SaveChangesAsync();

                // ส่งข้อมูลกลับ (ใช้ Game ID ที่เพิ่งได้มา เป็น Room Code)
                return new MatchFoundDTOs
                {
                    GameId = newGame.game_id,
                    RoomCode = newGame.game_id.ToString(),
                    OpponentUsername = opponentQueue.user.username,
                    Color = "white",
                    TimeControlMinutes = 10,
                    GameType = "online_multiplayer"
                };
            }

            // ยังไม่เจอคู่
            return null;
        }
    }
}