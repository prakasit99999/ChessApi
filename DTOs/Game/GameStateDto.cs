using System;
using ChessApi.DTOs.Game;

namespace ChessApi.DTOS.Game
{
    public class GameStateDto
    {
        // ข้อมูลอ้างอิง
        public int StateId { get; set; }
        public int GameId { get; set; }
        public int MoveNumber { get; set; }

        // ข้อมูลตำแหน่งหมาก (FEN) - แนะนำให้ใช้ตัวนี้เป็นหลักในการวาดกระดาน
        public string FenPosition { get; set; } 

        // สถานะของเกมในตานั้นๆ
        public bool IsCheck { get; set; }
        public bool IsCheckmate { get; set; }
        public bool IsStalemate { get; set; }

        // ข้อมูลสำหรับการวิเคราะห์ (จาก Model ของคุณ)
        public float? PositionScore { get; set; } // คะแนนความได้เปรียบ (เช่น +1.5, -0.8)
        public string PvLine { get; set; }        // Principal Variation (สายตาเดินที่ดีที่สุดที่ AI แนะนำ)
        
        public DateTime CreatedAt { get; set; }

        // เพิ่มเติม: ข้อมูลตาเดินล่าสุดเพื่อให้ Unity ทำ Highlight
        public MoveDto LastMove { get; set; }
    }
}