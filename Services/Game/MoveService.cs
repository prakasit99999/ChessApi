//using ChessApi.Models;
//using Microsoft.EntityFrameworkCore;
//using ChessApi.DbContext;
//using ChessApi.DTOs.Game;
//using ChessApi.Services.Interfaces;

//namespace ChessApi.Services.Game
//{
//    public class MoveService : IMoveService
//    {
//        private readonly ChessDbContext _context;
//        public MoveService(ChessDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<int> LogMoveAsync(MoveDto moveDto)
//        {
//            if (moveDto == null) throw new ArgumentNullException(nameof(moveDto));
//            var move = new move
//            {
//                game_id = moveDto.GameId,
//                move_number = moveDto.MoveNumber,
//                player_color = moveDto.PlayerColor,
//                piece_type = moveDto.PieceType,
//                from_square = moveDto.From,
//                to_square = moveDto.To,
//                move_notation = moveDto.MoveNotation,
//                captured_piece = moveDto.CapturedPiece,
//                promotion_piece = moveDto.PromotionPiece,
//                is_castle = moveDto.IsCastle,
//                castle_type = moveDto.CastleType,
//                is_en_passant = moveDto.IsEnPassant,
//                is_check = moveDto.IsCheck,
//                is_checkmate = moveDto.IsCheckmate,
//                move_time_seconds = moveDto.MoveTimeSeconds,
//                created_at = DateTime.UtcNow
//            };
//            _context.moves.Add(move);
//            await _context.SaveChangesAsync();
//            return move.move_id;
//        }
//        public async Task<MoveDto> AddMoveAsync(MoveDto moveDto)
//        {
//            if (moveDto == null) throw new ArgumentNullException(nameof(moveDto));
//            var move = new move
//            {
//                game_id = moveDto.GameId,
//                move_number = moveDto.MoveNumber,
//                player_color = moveDto.PlayerColor,
//                piece_type = moveDto.PieceType,
//                from_square = moveDto.From,
//                to_square = moveDto.To,
//                move_notation = moveDto.MoveNotation,
//                captured_piece = moveDto.CapturedPiece,
//                promotion_piece = moveDto.PromotionPiece,
//                is_castle = moveDto.IsCastle,
//                castle_type = moveDto.CastleType,
//                is_en_passant = moveDto.IsEnPassant,
//                is_check = moveDto.IsCheck,
//                is_checkmate = moveDto.IsCheckmate,
//                move_time_seconds = moveDto.MoveTimeSeconds,
//                created_at = DateTime.UtcNow
//            };
//            _context.moves.Add(move);
//            await _context.SaveChangesAsync();
//            return new MoveDto
//            {
//                GameId = move.game_id,
//                MoveNumber = move.move_number,
//                PlayerColor = move.player_color,
//                PieceType = move.piece_type,
//                From = move.from_square,
//                To = move.to_square,
//                MoveNotation = move.move_notation,
//                CapturedPiece = move.captured_piece,
//                PromotionPiece = move.promotion_piece,
//                IsCastle = (bool)move.is_castle!,
//                CastleType = move.castle_type,
//                IsEnPassant = (bool)move.is_en_passant!,
//                IsCheck = (bool)move.is_check!,
//                IsCheckmate = (bool)move.is_checkmate!,
//                MoveTimeSeconds = (int)move.move_time_seconds!,
//            };
//        }
//        public async Task<List<MoveDto>> GetAllMovesAsync()
//        {
//            return await _context.moves
//                .Select(m => new MoveDto
//                {
//                    GameId = m.game_id,
//                    MoveNumber = m.move_number,
//                    PlayerColor = m.player_color,
//                    PieceType = m.piece_type,
//                    From = m.from_square,
//                    To = m.to_square,
//                    MoveNotation = m.move_notation,
//                    CapturedPiece = m.captured_piece,
//                    PromotionPiece = m.promotion_piece,
//                    IsCastle = (bool)m.is_castle!,
//                    CastleType = m.castle_type,
//                    IsEnPassant = (bool)m.is_en_passant!,
//                    IsCheck = (bool)m.is_check!,
//                    IsCheckmate = (bool)m.is_checkmate!,
//                    MoveTimeSeconds = (int)m.move_time_seconds!,
//                }).ToListAsync();
//        }
//        public async Task<MoveDto> GetMoveByIdAsync(int id)
//        {
//            var move = await _context.moves.FindAsync(id);
//            if (move == null) return null;
//            return new MoveDto
//            {
//                GameId = move.game_id,
//                MoveNumber = move.move_number,
//                PlayerColor = move.player_color,
//                PieceType = move.piece_type,
//                From = move.from_square,
//                To = move.to_square,
//                MoveNotation = move.move_notation,
//                CapturedPiece = move.captured_piece,
//                PromotionPiece = move.promotion_piece,
//                IsCastle = (bool)move.is_castle!,
//                CastleType = move.castle_type,
//                IsEnPassant = (bool)move.is_en_passant!,
//                IsCheck = (bool)move.is_check!,
//                IsCheckmate = (bool)move.is_checkmate!,
//                MoveTimeSeconds = (int)move.move_time_seconds!,
//            };
//        }


//    }
//}
