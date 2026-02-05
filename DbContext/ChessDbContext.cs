using System;
using System.Collections.Generic;
using ChessApi.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace ChessApi.DbContext;

public partial class ChessDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public ChessDbContext()
    {
    }

    public ChessDbContext(DbContextOptions<ChessDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ai_performance> ai_performances { get; set; }

    public virtual DbSet<game> games { get; set; }

    public virtual DbSet<game_state> game_states { get; set; }

    public virtual DbSet<matchmaking_queue> matchmaking_queues { get; set; }

    public virtual DbSet<move> moves { get; set; }

    public virtual DbSet<user> users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=db;port=3306;database=chess_game_db;user=root;password=root", ServerVersion.Parse("8.0.43-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<ai_performance>(entity =>
        {
            entity.HasKey(e => e.performance_id).HasName("PRIMARY");

            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.game).WithMany(p => p.ai_performances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ai_performance_ibfk_1");
        });

        modelBuilder.Entity<game>(entity =>
        {
            entity.HasKey(e => e.game_id).HasName("PRIMARY");

            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.game_status).HasDefaultValueSql("'in_progress'");
            entity.Property(e => e.move_count).HasDefaultValueSql("'0'");

            entity.HasOne(d => d.black_player).WithMany(p => p.gameblack_players).HasConstraintName("games_ibfk_2");

            entity.HasOne(d => d.white_player).WithMany(p => p.gamewhite_players).HasConstraintName("games_ibfk_1");
        });

        modelBuilder.Entity<game_state>(entity =>
        {
            entity.HasKey(e => e.state_id).HasName("PRIMARY");

            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.is_check).HasDefaultValueSql("'0'");
            entity.Property(e => e.is_checkmate).HasDefaultValueSql("'0'");
            entity.Property(e => e.is_stalemate).HasDefaultValueSql("'0'");

            entity.HasOne(d => d.game).WithMany(p => p.game_states)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("game_states_ibfk_1");
        });

        modelBuilder.Entity<matchmaking_queue>(entity =>
        {
            entity.HasKey(e => e.queue_id).HasName("PRIMARY");

            entity.Property(e => e.joined_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.max_rating).HasDefaultValueSql("'3000'");
            entity.Property(e => e.min_rating).HasDefaultValueSql("'0'");
            entity.Property(e => e.status).HasDefaultValueSql("'waiting'");

            entity.HasOne(d => d.user).WithMany(p => p.matchmaking_queues)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("matchmaking_queue_ibfk_1");
        });

        modelBuilder.Entity<move>(entity =>
        {
            entity.HasKey(e => e.move_id).HasName("PRIMARY");

            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.is_capture).HasDefaultValueSql("'0'");
            entity.Property(e => e.is_castling).HasDefaultValueSql("'0'");
            entity.Property(e => e.is_check).HasDefaultValueSql("'0'");
            entity.Property(e => e.is_en_passant).HasDefaultValueSql("'0'");
            entity.Property(e => e.is_pawn_two_step).HasDefaultValueSql("'0'");
            entity.Property(e => e.piece_has_moved_before).HasDefaultValueSql("'0'");

            entity.HasOne(d => d.game).WithMany(p => p.moves)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("moves_ibfk_1");
        });

        modelBuilder.Entity<user>(entity =>
        {
            entity.HasKey(e => e.user_id).HasName("PRIMARY");

            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.games_drawn).HasDefaultValueSql("'0'");
            entity.Property(e => e.games_lost).HasDefaultValueSql("'0'");
            entity.Property(e => e.games_played).HasDefaultValueSql("'0'");
            entity.Property(e => e.games_won).HasDefaultValueSql("'0'");
            entity.Property(e => e.rating).HasDefaultValueSql("'1200'");
            entity.Property(e => e.status).HasDefaultValueSql("'offline'");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
