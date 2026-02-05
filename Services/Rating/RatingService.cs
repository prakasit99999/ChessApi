using System;
using System.Threading.Tasks;
using ChessApi.DbContext;
using ChessApi.Services.Interfaces;

namespace ChessApi.Services.Rating
{
    public class RatingService : IRatingService
    {
        private readonly ChessDbContext _context;
        private const int KFactor = 32;

        public RatingService(ChessDbContext context)
        {
            _context = context;
        }

        public async Task ProcessGameResultAsync(int whitePlayerId, int blackPlayerId, string winnerColor)
        {
            var whitePlayer = await _context.users.FindAsync(whitePlayerId);
            var blackPlayer = await _context.users.FindAsync(blackPlayerId);

            if (whitePlayer == null || blackPlayer == null)
            {
                return; // หรือจะโยน Exception ก็ได้
            }

            double whiteRating = whitePlayer.rating ?? 1200;
            double blackRating = blackPlayer.rating ?? 1200;

            double expectedWhite = 1.0 / (1.0 + Math.Pow(10, (blackRating - whiteRating) / 400.0));
            double expectedBlack = 1.0 / (1.0 + Math.Pow(10, (whiteRating - blackRating) / 400.0));

            double scoreWhite, scoreBlack;

            switch (winnerColor.ToLower())
            {
                case "white":
                    scoreWhite = 1.0;
                    scoreBlack = 0.0;
                    break;
                case "black":
                    scoreWhite = 0.0;
                    scoreBlack = 1.0;
                    break;
                default: // Draw
                    scoreWhite = 0.5;
                    scoreBlack = 0.5;
                    break;
            }

            whitePlayer.rating = (int)Math.Round(whiteRating + KFactor * (scoreWhite - expectedWhite));
            blackPlayer.rating = (int)Math.Round(blackRating + KFactor * (scoreBlack - expectedBlack));

            await _context.SaveChangesAsync();
        }
    }
}