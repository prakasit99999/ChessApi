using Microsoft.AspNetCore.SignalR;
using ChessApi.DbContext;
using ChessApi.Models;
using ChessApi.DTOs.Game; // ใช้ Namespace ของคุณ
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ChessApi.Hubs
{
    public class GameHub : Hub
    {
        private readonly ChessDbContext _context;

        public GameHub(ChessDbContext context)
        {
            _context = context;
        }

        // รับข้อมูลเป็น MoveDto.MoveRequest ของคุณ
        public async Task SendMove(string gameId, MoveDto.MoveRequest moveData) 
        {
            int gId = int.Parse(gameId);

            // 1. ส่งข้อมูลให้คู่แข่ง (ส่งไปทั้ง Object เลย ฝั่ง Unity จะได้รับเป็น JSON)
            await Clients.OthersInGroup(gameId).SendAsync("ReceiveMove", moveData);

            // 2. บันทึกลง Database
            try 
            {
                // นับจำนวน Move เดิมเพื่อหา MoveNumber ถัดไป (ถ้า Client ไม่ได้ส่งมา หรือส่งมาผิด)
                int currentMoveCount = await _context.moves.CountAsync(m => m.game_id == gId);
                
                var newMove = new move
                {
                    game_id = gId,
                    move_number = currentMoveCount + 1,

                    // Map พิกัด (int -> byte)
                    start_x = (sbyte)(byte)moveData.StartX,
                    start_y = (sbyte)(byte)moveData.StartY,
                    end_x = (sbyte)(byte)moveData.EndX,
                    end_y = (sbyte)(byte)moveData.EndY,
                    
                    // Map Enums -> String (แปลงเป็นตัวพิมพ์เล็กตาม MySQL Enum)
                    piece_type = moveData.PieceType.ToString().ToLower(), // Pawn -> "pawn"
                    team = moveData.PlayerTurn.ToString().ToLower(),      // White -> "white"
                    
                    // Optional Fields: เช็คว่ามีค่าไหม
                    captured_x = (sbyte?)(byte)moveData.CapturedX,
                    captured_y = (sbyte?)(byte)moveData.CapturedY,
                    
                    captured_piece_type = moveData.CapturedPieceType != MoveDto.capturePieceType.None 
                        ? moveData.CapturedPieceType.ToString().ToLower() : null,
                        
                    captured_piece_team = moveData.CapturedPieceTeam != MoveDto.capturedPicecTeam.None 
                        ? moveData.CapturedPieceTeam.ToString().ToLower() : null,
                        
                    promoted_to = moveData.PromotedTo != MoveDto.promotedTo.None 
                        ? moveData.PromotedTo.ToString().ToLower() : null,
                    
                    promoted_from = moveData.PromotedFrom != MoveDto.promotedFrom.None
                         ? moveData.PromotedFrom.ToString().ToLower() : null,

                    // Flags
                    is_castling = moveData.IsCastling,
                    is_en_passant = moveData.IsEnPassant,
                    is_capture = moveData.IsCapture,
                    is_check = moveData.IsCheck,
                    is_pawn_two_step = moveData.IsPawnTwoStep,
                    piece_has_moved_before = moveData.PieceHasMovedBefore,
                    
                    // AI Data (ถ้ามี)
                    algorithm_type = moveData.AlgorithmType != MoveDto.algorithmType.None
                        ? moveData.AlgorithmType.ToString().ToLower() : null,
                    ai_evaluation_score = moveData.AiEvaluationScore,
                    ai_depth_searched = moveData.AiDepthSearched,
                    ai_nodes_evaluated = moveData.AiNodesEvaluated,
                    move_time_ms = moveData.MoveTimeMilliseconds,

                    created_at = DateTime.UtcNow
                };

                _context.moves.Add(newMove);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving move: {ex.Message}");
                if(ex.InnerException != null) Console.WriteLine($"Inner: {ex.InnerException.Message}");
            }
        }

        public async Task JoinGame(string gameId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
            await Clients.Group(gameId).SendAsync("UserJoined", Context.ConnectionId);
        }
        
        public async Task LeaveGame(string gameId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);
            await Clients.Group(gameId).SendAsync("UserLeft", Context.ConnectionId);
        }
    }
}