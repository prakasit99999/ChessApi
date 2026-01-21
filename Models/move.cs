using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ChessApi.Models;

[Index("game_id", Name = "game_id")]
public partial class move
{
    /// <summary>
    /// รหัสระบุการเดินหมาก (Primary Key)
    /// </summary>
    [Key]
    public int move_id { get; set; }

    /// <summary>
    /// รหัสอ้างอิงเกม
    /// </summary>
    public int game_id { get; set; }

    /// <summary>
    /// ลำดับที่ของการเดินในเกมนั้นๆ
    /// </summary>
    public int move_number { get; set; }

    /// <summary>
    /// พิกัดเริ่มต้นแนวนอน (X)
    /// </summary>
    public sbyte start_x { get; set; }

    /// <summary>
    /// พิกัดเริ่มต้นแนวตั้ง (Y)
    /// </summary>
    public sbyte start_y { get; set; }

    /// <summary>
    /// พิกัดสิ้นสุดแนวนอน (X)
    /// </summary>
    public sbyte end_x { get; set; }

    /// <summary>
    /// พิกัดสิ้นสุดแนวตั้ง (Y)
    /// </summary>
    public sbyte end_y { get; set; }

    /// <summary>
    /// พิกัดแนวนอนของหมากที่ถูกกิน (ถ้ามี)
    /// </summary>
    public sbyte? captured_x { get; set; }

    /// <summary>
    /// พิกัดแนวตั้งของหมากที่ถูกกิน (ถ้ามี)
    /// </summary>
    public sbyte? captured_y { get; set; }

    /// <summary>
    /// พิกัดแนวนอนของการเลื่อนยศ (ถ้ามี)
    /// </summary>
    public sbyte? promoted_x { get; set; }

    /// <summary>
    /// พิกัดแนวตั้งของการเลื่อนยศ (ถ้ามี)
    /// </summary>
    public sbyte? promoted_y { get; set; }

    /// <summary>
    /// พิกัดแนวนอนของการกินผ่าน (En Passant) (ถ้ามี)
    /// </summary>
    public sbyte? enpassant_x { get; set; }

    /// <summary>
    /// พิกัดแนวตั้งของการกินผ่าน (En Passant) (ถ้ามี)
    /// </summary>
    public sbyte? enpassant_y { get; set; }

    /// <summary>
    /// ประเภทของหมากที่เดิน (pawn, rook, knight, bishop, queen, king)
    /// </summary>
    [Column(TypeName = "enum('pawn','rook','knight','bishop','queen','king')")]
    public string piece_type { get; set; } = null!;

    /// <summary>
    /// ฝ่ายที่เดิน (white, black)
    /// </summary>
    [Column(TypeName = "enum('white','black')")]
    public string team { get; set; } = null!;

    /// <summary>
    /// ประเภทหมากที่ถูกกิน (ถ้ามี)
    /// </summary>
    [Column(TypeName = "enum('pawn','rook','knight','bishop','queen','king')")]
    public string? captured_piece_type { get; set; }

    /// <summary>
    /// ฝ่ายของหมากที่ถูกกิน (ถ้ามี)
    /// </summary>
    [Column(TypeName = "enum('white','black')")]
    public string? captured_piece_team { get; set; }

    /// <summary>
    /// ประเภทหมากก่อนเลื่อนยศ (ปกติคือ pawn)
    /// </summary>
    [Column(TypeName = "enum('pawn','rook','knight','bishop','queen','king')")]
    public string? promoted_from { get; set; }

    /// <summary>
    /// ประเภทหมากหลังเลื่อนยศ
    /// </summary>
    [Column(TypeName = "enum('pawn','rook','knight','bishop','queen','king')")]
    public string? promoted_to { get; set; }

    /// <summary>
    /// เป็นการเข้าป้อมหรือไม่
    /// </summary>
    public bool? is_castling { get; set; }

    /// <summary>
    /// เป็นการกินผ่าน (En Passant) หรือไม่
    /// </summary>
    public bool? is_en_passant { get; set; }

    /// <summary>
    /// เป็นการกินหมากหรือไม่
    /// </summary>
    public bool? is_capture { get; set; }

    /// <summary>
    /// เป็นการรุกหรือไม่
    /// </summary>
    public bool? is_check { get; set; }

    /// <summary>
    /// เป็นการเดินเบี้ยสองช่องหรือไม่
    /// </summary>
    public bool? is_pawn_two_step { get; set; }

    /// <summary>
    /// หมากตัวนี้เคยเดินมาก่อนหน้านี้หรือไม่
    /// </summary>
    public bool? piece_has_moved_before { get; set; }

    /// <summary>
    /// ประเภทอัลกอริทึม AI ที่ใช้คำนวณตานี้ (ถ้าเป็น AI)
    /// </summary>
    [Column(TypeName = "enum('minimax','alpha_beta')")]
    public string? algorithm_type { get; set; }

    /// <summary>
    /// คะแนนประเมินสถานการณ์จาก AI
    /// </summary>
    public int? ai_evaluation_score { get; set; }

    /// <summary>
    /// ความลึกที่ AI ค้นหา
    /// </summary>
    public int? ai_depth_searched { get; set; }

    /// <summary>
    /// จำนวนโหนดที่ AI ประเมิน
    /// </summary>
    public int? ai_nodes_evaluated { get; set; }

    /// <summary>
    /// เวลาที่ใช้ในการคำนวณหรือเดิน (มิลลิวินาที)
    /// </summary>
    public int? move_time_ms { get; set; }

    /// <summary>
    /// เวลาที่บันทึกข้อมูล
    /// </summary>
    [Column(TypeName = "timestamp")]
    public DateTime? created_at { get; set; }

    [ForeignKey("game_id")]
    [InverseProperty("moves")]
    public virtual game game { get; set; } = null!;
}
