using System;
using System.Collections.Generic;
using ChessApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ChessApi.DbContext;

public partial class ChessDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public ChessDbContext()
    {
    }

    public ChessDbContext(DbContextOptions<ChessDbContext> options) : base(options)
    {
    }

    public virtual DbSet<active_game> active_games { get; set; }

    public virtual DbSet<ai_performance> ai_performances { get; set; }

    public virtual DbSet<friendship> friendships { get; set; }

    public virtual DbSet<game> games { get; set; }

    public virtual DbSet<game_invitation> game_invitations { get; set; }

    public virtual DbSet<game_room> game_rooms { get; set; }

    public virtual DbSet<game_state> game_states { get; set; }

    public virtual DbSet<matchmaking_queue> matchmaking_queues { get; set; }

    public virtual DbSet<move> moves { get; set; }

    public virtual DbSet<opening_statistic> opening_statistics { get; set; }

    public virtual DbSet<public_room> public_rooms { get; set; }

    public virtual DbSet<room_chat_message> room_chat_messages { get; set; }

    public virtual DbSet<room_participant> room_participants { get; set; }

    public virtual DbSet<system_setting> system_settings { get; set; }

    public virtual DbSet<user> users { get; set; }

    public virtual DbSet<user_ranking> user_rankings { get; set; }

    public virtual DbSet<user_statistic> user_statistics { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=192.168.1.13;port=32768;database=chess_game_db;user=root;password=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("9.4.0-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<active_game>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("active_games");

            entity.Property(e => e.black_player_type).HasColumnType("enum('human','ai_easy','ai_normal','ai_hard')");
            entity.Property(e => e.black_username).HasMaxLength(50);
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.current_turn)
                .HasDefaultValueSql("'white'")
                .HasColumnType("enum('white','black')");
            entity.Property(e => e.game_type).HasColumnType("enum('single_player','ai_vs_ai','local_multiplayer','online_multiplayer','room_game')");
            entity.Property(e => e.last_move_at).HasColumnType("timestamp");
            entity.Property(e => e.move_count).HasDefaultValueSql("'0'");
            entity.Property(e => e.room_code).HasMaxLength(8);
            entity.Property(e => e.room_name).HasMaxLength(100);
            entity.Property(e => e.white_player_type).HasColumnType("enum('human','ai_easy','ai_normal','ai_hard')");
            entity.Property(e => e.white_username).HasMaxLength(50);
        });

        modelBuilder.Entity<ai_performance>(entity =>
        {
            entity.HasKey(e => e.performance_id).HasName("PRIMARY");

            entity.ToTable("ai_performance");

            entity.HasIndex(e => new { e.game_id, e.ai_level }, "idx_game_ai");

            entity.Property(e => e.ai_level).HasColumnType("enum('easy','normal','hard')");
            entity.Property(e => e.algorithm_type).HasColumnType("enum('minimax','minimax_alpha_beta')");
            entity.Property(e => e.average_depth).HasPrecision(4, 2);
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.endgame_tablebase_moves).HasDefaultValueSql("'0'");
            entity.Property(e => e.opening_book_moves).HasDefaultValueSql("'0'");

            entity.HasOne(d => d.game).WithMany(p => p.ai_performances)
                .HasForeignKey(d => d.game_id)
                .HasConstraintName("ai_performance_ibfk_1");
        });

        modelBuilder.Entity<friendship>(entity =>
        {
            entity.HasKey(e => e.friendship_id).HasName("PRIMARY");

            entity.HasIndex(e => new { e.user1_id, e.status }, "idx_user1_status");

            entity.HasIndex(e => new { e.user2_id, e.status }, "idx_user2_status");

            entity.HasIndex(e => new { e.user1_id, e.user2_id }, "unique_friendship").IsUnique();

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.status)
                .HasDefaultValueSql("'pending'")
                .HasColumnType("enum('pending','accepted','blocked')");
            entity.Property(e => e.updated_at)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");

            entity.HasOne(d => d.user1).WithMany(p => p.friendshipuser1s)
                .HasForeignKey(d => d.user1_id)
                .HasConstraintName("friendships_ibfk_1");

            entity.HasOne(d => d.user2).WithMany(p => p.friendshipuser2s)
                .HasForeignKey(d => d.user2_id)
                .HasConstraintName("friendships_ibfk_2");
        });

        modelBuilder.Entity<game>(entity =>
        {
            entity.HasKey(e => e.game_id).HasName("PRIMARY");

            entity.HasIndex(e => e.black_player_id, "black_player_id");

            entity.HasIndex(e => e.created_at, "idx_created_at");

            entity.HasIndex(e => e.game_status, "idx_game_status");

            entity.HasIndex(e => e.game_type, "idx_game_type");

            entity.HasIndex(e => new { e.game_status, e.last_move_at }, "idx_games_active");

            entity.HasIndex(e => new { e.white_player_id, e.black_player_id }, "idx_players");

            entity.HasIndex(e => e.room_id, "idx_room_game");

            entity.Property(e => e.black_player_type).HasColumnType("enum('human','ai_easy','ai_normal','ai_hard')");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.current_turn)
                .HasDefaultValueSql("'white'")
                .HasColumnType("enum('white','black')");
            entity.Property(e => e.finished_at).HasColumnType("timestamp");
            entity.Property(e => e.game_status)
                .HasDefaultValueSql("'waiting'")
                .HasColumnType("enum('waiting','in_progress','finished','abandoned')");
            entity.Property(e => e.game_type).HasColumnType("enum('single_player','ai_vs_ai','local_multiplayer','online_multiplayer','room_game')");
            entity.Property(e => e.is_rated).HasDefaultValueSql("'1'");
            entity.Property(e => e.last_move_at).HasColumnType("timestamp");
            entity.Property(e => e.move_count).HasDefaultValueSql("'0'");
            entity.Property(e => e.result).HasColumnType("enum('white_wins','black_wins','draw','stalemate','abandoned')");
            entity.Property(e => e.result_reason).HasColumnType("enum('checkmate','resignation','timeout','stalemate','draw_agreement','insufficient_material','fifty_move_rule','threefold_repetition')");
            entity.Property(e => e.started_at).HasColumnType("timestamp");
            entity.Property(e => e.time_control_minutes).HasDefaultValueSql("'0'");
            entity.Property(e => e.white_player_type).HasColumnType("enum('human','ai_easy','ai_normal','ai_hard')");

            entity.HasOne(d => d.black_player).WithMany(p => p.gameblack_players)
                .HasForeignKey(d => d.black_player_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("games_ibfk_3");

            entity.HasOne(d => d.room).WithMany(p => p.games)
                .HasForeignKey(d => d.room_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("games_ibfk_1");

            entity.HasOne(d => d.white_player).WithMany(p => p.gamewhite_players)
                .HasForeignKey(d => d.white_player_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("games_ibfk_2");
        });

        modelBuilder.Entity<game_invitation>(entity =>
        {
            entity.HasKey(e => e.invitation_id).HasName("PRIMARY");

            entity.HasIndex(e => e.from_user_id, "from_user_id");

            entity.HasIndex(e => e.expires_at, "idx_expires_at");

            entity.HasIndex(e => new { e.to_user_id, e.status }, "idx_to_user_status");

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.expires_at).HasColumnType("timestamp");
            entity.Property(e => e.message).HasColumnType("text");
            entity.Property(e => e.responded_at).HasColumnType("timestamp");
            entity.Property(e => e.status)
                .HasDefaultValueSql("'pending'")
                .HasColumnType("enum('pending','accepted','declined','expired')");
            entity.Property(e => e.time_control_minutes).HasDefaultValueSql("'0'");

            entity.HasOne(d => d.from_user).WithMany(p => p.game_invitationfrom_users)
                .HasForeignKey(d => d.from_user_id)
                .HasConstraintName("game_invitations_ibfk_1");

            entity.HasOne(d => d.to_user).WithMany(p => p.game_invitationto_users)
                .HasForeignKey(d => d.to_user_id)
                .HasConstraintName("game_invitations_ibfk_2");
        });

        modelBuilder.Entity<game_room>(entity =>
        {
            entity.HasKey(e => e.room_id).HasName("PRIMARY");

            entity.HasIndex(e => e.created_by_user_id, "idx_created_by");

            entity.HasIndex(e => e.expires_at, "idx_expires_at");

            entity.HasIndex(e => new { e.min_rating, e.max_rating }, "idx_rating_range");

            entity.HasIndex(e => e.room_code, "idx_room_code").IsUnique();

            entity.HasIndex(e => e.room_status, "idx_room_status");

            entity.HasIndex(e => e.room_type, "idx_room_type");

            entity.Property(e => e.allow_spectators).HasDefaultValueSql("'1'");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.current_participants).HasDefaultValueSql("'0'");
            entity.Property(e => e.expires_at).HasColumnType("timestamp");
            entity.Property(e => e.game_settings).HasColumnType("json");
            entity.Property(e => e.is_rated).HasDefaultValueSql("'1'");
            entity.Property(e => e.last_activity)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.max_participants).HasDefaultValueSql("'2'");
            entity.Property(e => e.max_rating).HasDefaultValueSql("'3000'");
            entity.Property(e => e.min_rating).HasDefaultValueSql("'0'");
            entity.Property(e => e.password_hash).HasMaxLength(255);
            entity.Property(e => e.room_code).HasMaxLength(8);
            entity.Property(e => e.room_description).HasColumnType("text");
            entity.Property(e => e.room_name).HasMaxLength(100);
            entity.Property(e => e.room_status)
                .HasDefaultValueSql("'waiting'")
                .HasColumnType("enum('waiting','in_game','finished','closed')");
            entity.Property(e => e.room_type)
                .HasDefaultValueSql("'public'")
                .HasColumnType("enum('public','private','friends_only')");
            entity.Property(e => e.started_at).HasColumnType("timestamp");
            entity.Property(e => e.time_control_minutes).HasDefaultValueSql("'0'");

            entity.HasOne(d => d.created_by_user).WithMany(p => p.game_rooms)
                .HasForeignKey(d => d.created_by_user_id)
                .HasConstraintName("game_rooms_ibfk_1");
        });

        modelBuilder.Entity<game_state>(entity =>
        {
            entity.HasKey(e => e.state_id).HasName("PRIMARY");

            entity.HasIndex(e => new { e.game_id, e.move_number }, "idx_game_move").IsUnique();

            entity.HasIndex(e => e.fen_position, "idx_game_states_fen");

            entity.Property(e => e.board_state).HasColumnType("json");
            entity.Property(e => e.castle_rights).HasColumnType("json");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.en_passant_square).HasMaxLength(2);
            entity.Property(e => e.fen_position).HasMaxLength(100);
            entity.Property(e => e.fullmove_number).HasDefaultValueSql("'1'");
            entity.Property(e => e.halfmove_clock).HasDefaultValueSql("'0'");
            entity.Property(e => e.is_check).HasDefaultValueSql("'0'");
            entity.Property(e => e.is_checkmate).HasDefaultValueSql("'0'");
            entity.Property(e => e.is_stalemate).HasDefaultValueSql("'0'");

            entity.HasOne(d => d.game).WithMany(p => p.game_states)
                .HasForeignKey(d => d.game_id)
                .HasConstraintName("game_states_ibfk_1");
        });

        modelBuilder.Entity<matchmaking_queue>(entity =>
        {
            entity.HasKey(e => e.queue_id).HasName("PRIMARY");

            entity.ToTable("matchmaking_queue");

            entity.HasIndex(e => e.joined_at, "idx_joined_at");

            entity.HasIndex(e => new { e.min_rating, e.max_rating }, "idx_rating_range");

            entity.HasIndex(e => new { e.user_id, e.status }, "idx_user_waiting");

            entity.Property(e => e.joined_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.max_rating).HasDefaultValueSql("'3000'");
            entity.Property(e => e.min_rating).HasDefaultValueSql("'0'");
            entity.Property(e => e.preferred_time_control).HasDefaultValueSql("'0'");
            entity.Property(e => e.status)
                .HasDefaultValueSql("'waiting'")
                .HasColumnType("enum('waiting','matched','cancelled')");

            entity.HasOne(d => d.user).WithMany(p => p.matchmaking_queues)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("matchmaking_queue_ibfk_1");
        });

        modelBuilder.Entity<move>(entity =>
        {
            entity.HasKey(e => e.move_id).HasName("PRIMARY");

            entity.HasIndex(e => new { e.game_id, e.move_number }, "idx_game_moves");

            entity.HasIndex(e => new { e.game_id, e.move_number, e.player_color }, "idx_moves_game_sequence").IsUnique();

            entity.HasIndex(e => e.move_notation, "idx_notation");

            entity.Property(e => e.ai_evaluation_score).HasPrecision(6, 2);
            entity.Property(e => e.captured_piece).HasColumnType("enum('pawn','rook','knight','bishop','queen','king')");
            entity.Property(e => e.castle_type).HasColumnType("enum('kingside','queenside')");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.from_square).HasMaxLength(2);
            entity.Property(e => e.is_castle).HasDefaultValueSql("'0'");
            entity.Property(e => e.is_check).HasDefaultValueSql("'0'");
            entity.Property(e => e.is_checkmate).HasDefaultValueSql("'0'");
            entity.Property(e => e.is_en_passant).HasDefaultValueSql("'0'");
            entity.Property(e => e.move_notation).HasMaxLength(10);
            entity.Property(e => e.move_time_seconds).HasDefaultValueSql("'0'");
            entity.Property(e => e.piece_type).HasColumnType("enum('pawn','rook','knight','bishop','queen','king')");
            entity.Property(e => e.player_color).HasColumnType("enum('white','black')");
            entity.Property(e => e.promotion_piece).HasColumnType("enum('rook','knight','bishop','queen')");
            entity.Property(e => e.to_square).HasMaxLength(2);

            entity.HasOne(d => d.game).WithMany(p => p.moves)
                .HasForeignKey(d => d.game_id)
                .HasConstraintName("moves_ibfk_1");
        });

        modelBuilder.Entity<opening_statistic>(entity =>
        {
            entity.HasKey(e => e.opening_id).HasName("PRIMARY");

            entity.HasIndex(e => e.eco_code, "idx_eco_code");

            entity.HasIndex(e => e.games_played, "idx_games_played");

            entity.HasIndex(e => new { e.opening_name, e.eco_code }, "unique_opening").IsUnique();

            entity.Property(e => e.average_rating).HasDefaultValueSql("'0'");
            entity.Property(e => e.black_wins).HasDefaultValueSql("'0'");
            entity.Property(e => e.draws).HasDefaultValueSql("'0'");
            entity.Property(e => e.eco_code).HasMaxLength(3);
            entity.Property(e => e.games_played).HasDefaultValueSql("'0'");
            entity.Property(e => e.last_updated)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.move_sequence).HasMaxLength(500);
            entity.Property(e => e.opening_name).HasMaxLength(100);
            entity.Property(e => e.white_wins).HasDefaultValueSql("'0'");
        });

        modelBuilder.Entity<public_room>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("public_rooms");

            entity.Property(e => e.allow_spectators).HasDefaultValueSql("'1'");
            entity.Property(e => e.availability_status)
                .HasMaxLength(11)
                .HasDefaultValueSql("''")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.current_participants).HasDefaultValueSql("'0'");
            entity.Property(e => e.host_username).HasMaxLength(50);
            entity.Property(e => e.is_rated).HasDefaultValueSql("'1'");
            entity.Property(e => e.last_activity)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.max_participants).HasDefaultValueSql("'2'");
            entity.Property(e => e.max_rating).HasDefaultValueSql("'3000'");
            entity.Property(e => e.min_rating).HasDefaultValueSql("'0'");
            entity.Property(e => e.room_code).HasMaxLength(8);
            entity.Property(e => e.room_description).HasColumnType("text");
            entity.Property(e => e.room_name).HasMaxLength(100);
            entity.Property(e => e.room_status)
                .HasDefaultValueSql("'waiting'")
                .HasColumnType("enum('waiting','in_game','finished','closed')");
            entity.Property(e => e.time_control_minutes).HasDefaultValueSql("'0'");
        });

        modelBuilder.Entity<room_chat_message>(entity =>
        {
            entity.HasKey(e => e.message_id).HasName("PRIMARY");

            entity.HasIndex(e => e.message_type, "idx_message_type");

            entity.HasIndex(e => new { e.room_id, e.expires_at }, "idx_room_expires");

            entity.HasIndex(e => e.user_id, "user_id");

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.expires_at)
                .HasDefaultValueSql("(now() + interval 1 hour)")
                .HasColumnType("timestamp");
            entity.Property(e => e.message_content).HasColumnType("text");
            entity.Property(e => e.message_type)
                .HasDefaultValueSql("'system'")
                .HasColumnType("enum('system','game_event')");

            entity.HasOne(d => d.room).WithMany(p => p.room_chat_messages)
                .HasForeignKey(d => d.room_id)
                .HasConstraintName("room_chat_messages_ibfk_1");

            entity.HasOne(d => d.user).WithMany(p => p.room_chat_messages)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("room_chat_messages_ibfk_2");
        });

        modelBuilder.Entity<room_participant>(entity =>
        {
            entity.HasKey(e => e.participant_id).HasName("PRIMARY");

            entity.HasIndex(e => e.joined_at, "idx_joined_at");

            entity.HasIndex(e => new { e.room_id, e.role }, "idx_room_role");

            entity.HasIndex(e => new { e.user_id, e.status }, "idx_user_status");

            entity.HasIndex(e => new { e.room_id, e.user_id }, "unique_user_room").IsUnique();

            entity.Property(e => e.is_ready).HasDefaultValueSql("'0'");
            entity.Property(e => e.joined_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.left_at).HasColumnType("timestamp");
            entity.Property(e => e.player_color)
                .HasDefaultValueSql("'random'")
                .HasColumnType("enum('white','black','random','spectator')");
            entity.Property(e => e.role).HasColumnType("enum('host','player','spectator')");
            entity.Property(e => e.status)
                .HasDefaultValueSql("'active'")
                .HasColumnType("enum('active','left','kicked')");

            entity.HasOne(d => d.room).WithMany(p => p.room_participants)
                .HasForeignKey(d => d.room_id)
                .HasConstraintName("room_participants_ibfk_1");

            entity.HasOne(d => d.user).WithMany(p => p.room_participants)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("room_participants_ibfk_2");
        });

        modelBuilder.Entity<system_setting>(entity =>
        {
            entity.HasKey(e => e.setting_id).HasName("PRIMARY");

            entity.HasIndex(e => e.setting_key, "idx_setting_key").IsUnique();

            entity.Property(e => e.description).HasColumnType("text");
            entity.Property(e => e.setting_key).HasMaxLength(50);
            entity.Property(e => e.setting_value).HasColumnType("text");
            entity.Property(e => e.updated_at)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
        });

        modelBuilder.Entity<user>(entity =>
        {
            entity.HasKey(e => e.user_id).HasName("PRIMARY");

            entity.HasIndex(e => e.email, "email").IsUnique();

            entity.HasIndex(e => e.rating, "idx_rating");

            entity.HasIndex(e => e.status, "idx_status");

            entity.HasIndex(e => e.username, "idx_username").IsUnique();

            entity.HasIndex(e => new { e.rating, e.status }, "idx_users_rating_status").IsDescending(true, false);

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.email).HasMaxLength(100);
            entity.Property(e => e.games_drawn).HasDefaultValueSql("'0'");
            entity.Property(e => e.games_lost).HasDefaultValueSql("'0'");
            entity.Property(e => e.games_played).HasDefaultValueSql("'0'");
            entity.Property(e => e.games_won).HasDefaultValueSql("'0'");
            entity.Property(e => e.last_login).HasColumnType("timestamp");
            entity.Property(e => e.password_hash).HasColumnType("text");
            entity.Property(e => e.rating).HasDefaultValueSql("'1200'");
            entity.Property(e => e.status)
                .HasDefaultValueSql("'offline'")
                .HasColumnType("enum('offline','online','playing')");
            entity.Property(e => e.username).HasMaxLength(50);
        });

        modelBuilder.Entity<user_ranking>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("user_rankings");

            entity.Property(e => e.games_drawn).HasDefaultValueSql("'0'");
            entity.Property(e => e.games_lost).HasDefaultValueSql("'0'");
            entity.Property(e => e.games_played).HasDefaultValueSql("'0'");
            entity.Property(e => e.games_won).HasDefaultValueSql("'0'");
            entity.Property(e => e.rating).HasDefaultValueSql("'1200'");
            entity.Property(e => e.status)
                .HasDefaultValueSql("'offline'")
                .HasColumnType("enum('offline','online','playing')");
            entity.Property(e => e.username).HasMaxLength(50);
            entity.Property(e => e.win_percentage).HasPrecision(16, 2);
        });

        modelBuilder.Entity<user_statistic>(entity =>
        {
            entity.HasKey(e => e.stat_id).HasName("PRIMARY");

            entity.HasIndex(e => new { e.period_type, e.period_date }, "idx_period");

            entity.HasIndex(e => new { e.user_id, e.period_type, e.period_date }, "unique_user_period").IsUnique();

            entity.Property(e => e.average_game_length_moves)
                .HasPrecision(6, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.favorite_opening).HasMaxLength(50);
            entity.Property(e => e.games_drawn).HasDefaultValueSql("'0'");
            entity.Property(e => e.games_lost).HasDefaultValueSql("'0'");
            entity.Property(e => e.games_played).HasDefaultValueSql("'0'");
            entity.Property(e => e.games_won).HasDefaultValueSql("'0'");
            entity.Property(e => e.period_type).HasColumnType("enum('daily','weekly','monthly','all_time')");
            entity.Property(e => e.rating_change).HasDefaultValueSql("'0'");
            entity.Property(e => e.updated_at)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.win_rate_as_black)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("'0.00'");
            entity.Property(e => e.win_rate_as_white)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("'0.00'");

            entity.HasOne(d => d.user).WithMany(p => p.user_statistics)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("user_statistics_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
