using ChessApi.DbContext;
using ChessApi.DTOs.Matchmaking;
using ChessApi.Models;
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

        public async Task<MatchFoundDTOs?> JoinQueueAsync(JoinQueueDTOs request)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.username == request.Username);
            if (user == null) throw new Exception("User not found");

            // เคลียร์คิวเก่าที่ค้างอยู่ (ป้องกันการเข้าคิวซ้ำ)
            var oldQueue = await _context.matchmaking_queues
                .FirstOrDefaultAsync(q => q.user_id == user.user_id);
            if (oldQueue != null) { _context.matchmaking_queues.Remove(oldQueue); }

            // 1. ลองหาคู่ก่อนเข้าคิว (Matching Logic)
            var opponentQueue = await _context.matchmaking_queues
                .FirstOrDefaultAsync(q => q.user_id != user.user_id &&
                                          q.status == "waiting" &&
                                          q.min_rating <= user.rating &&
                                          q.max_rating >= user.rating);

            if (opponentQueue != null)
            {
                // ==== เจิคู่ทันที (Match Found!) ====
                
                // ดึงข้อมูล User ของคู่แข่ง
                var opponentUser = await _context.users.FindAsync(opponentQueue.user_id);

                // สร้างเกมใหม่ (ใช้สถานะ in_progress ตามที่แก้ Database แล้ว)
                var newGame = new game
                {
                    white_player_id = user.user_id, // เราเป็นคนเจอ เราเป็นสีขาว (หรือจะสุ่มก็ได้)
                    black_player_id = opponentQueue.user_id,
                    game_status = "in_progress", // *** ต้องตรงกับ ENUM ใน Database ***
                    game_type = "online_multiplayer",
                    white_player_type = "human",
                    black_player_type = "human",
                    created_at = DateTime.UtcNow
                };

                _context.games.Add(newGame);
                await _context.SaveChangesAsync(); // Save เพื่อเอา game_id

                // ลบคิวของคู่แข่งออก (เพราะได้เล่นแล้ว)
                _context.matchmaking_queues.Remove(opponentQueue);
                await _context.SaveChangesAsync();
                
                // ส่วนของเราไม่ได้สร้างคิว จึงไม่ต้องลบ
                
                return new MatchFoundDTOs
                {
                    GameId = newGame.game_id,
                    RoomCode = newGame.game_id.ToString(),
                    OpponentUsername = opponentUser.username,
                    Color = "white", 
                    GameType = "online_multiplayer"
                };
            }

            // 2. ถ้าไม่เจอคู่ -> สร้างคิวรอ (Waiting)
            var queueItem = new matchmaking_queue
            {
                user_id = user.user_id,
                min_rating = request.MinRating,
                max_rating = request.MaxRating,
                status = "waiting",
                joined_at = DateTime.UtcNow
            };

            _context.matchmaking_queues.Add(queueItem);
            await _context.SaveChangesAsync();

            return null; // ยังไม่เจอคู่ ต้องรอ Poll
        }

        public async Task<MatchFoundDTOs?> CheckForMatchAsync(string username)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.username == username);
            if (user == null) return null;

            // -----------------------------------------------------------------------
            // แก้ไขใหม่: เช็คจากตาราง games โดยตรง (แม่นยำกว่าเช็คสถานะคิว)
            // -----------------------------------------------------------------------
            var activeGame = await _context.games
                .Where(g => (g.white_player_id == user.user_id || g.black_player_id == user.user_id) 
                            && g.game_status == "in_progress" 
                            && g.result == null) // เกมยังไม่จบ
                .OrderByDescending(g => g.created_at) // เอาเกมล่าสุด
                .FirstOrDefaultAsync();

            if (activeGame != null)
            {
                // เจอเกมที่กำลัง Active อยู่! แสดงว่าจับคู่สำเร็จแล้ว
                
                // หา ID ของคู่แข่ง
                int opponentId = (int)((activeGame.white_player_id == user.user_id) 
                    ? activeGame.black_player_id 
                    : activeGame.white_player_id);
                    
                var opponentUser = await _context.users.FindAsync(opponentId);
                
                // Cleanup: ลบตัวเองออกจากคิวรอ (ถ้ายังมีค้างอยู่)
                var myQueue = await _context.matchmaking_queues
                    .FirstOrDefaultAsync(q => q.user_id == user.user_id);
                if (myQueue != null)
                {
                    _context.matchmaking_queues.Remove(myQueue);
                    await _context.SaveChangesAsync();
                }

                return new MatchFoundDTOs
                {
                    GameId = activeGame.game_id,
                    RoomCode = activeGame.game_id.ToString(),
                    OpponentUsername = opponentUser?.username ?? "Unknown",
                    Color = (activeGame.white_player_id == user.user_id) ? "white" : "black",
                    GameType = "online_multiplayer"
                };
            }

            return null; // ยังไม่เจอเกม รอต่อไป
        }

        public async Task CancelQueueAsync(CancelQueueDTOs request)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.username == request.Username);
            if (user != null)
            {
                var queueItem = await _context.matchmaking_queues.FirstOrDefaultAsync(q => q.user_id == user.user_id);
                if (queueItem != null)
                {
                    _context.matchmaking_queues.Remove(queueItem);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}