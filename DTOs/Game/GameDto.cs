namespace ChessApi.DTOs.Game
{
    public class GameDto
    {
        public int GameId { get; set; }
        public string GameType { get; set; }
        public string WhitePlayer { get; set; }
        public string BlackPlayer { get; set; }
        public string Status { get; set; }
        public string Result { get; set; }
        public int MoveCount { get; set; }
        public string CurrentTurn { get; set; }
    }
}
