diff --git a/d:\Project\ChessApi\README.md b/d:\Project\ChessApi\README.md
new file mode 100644
--- /dev/null
+++ b/d:\Project\ChessApi\README.md
@@ -0,0 +1,121 @@
+# ChessApi
+
+## โครงสร้างโปรเจกต์ (อัปเดตล่าสุด)
+
+หมายเหตุ: ไม่รวมไฟล์ build เช่น `bin/`, `obj/`, `.vs/`
+
+```
+ChessApi/
+├─ .dockerignore
+├─ .gitattributes
+├─ .gitignore
+├─ .vscode/
+│  └─ mcp.json
+├─ Areas/
+│  └─ Identity/
+│     └─ Pages/
+│        └─ Account/
+├─ Controllers/
+│  ├─ ai_performance/
+│  │  └─ AiPerformanceController.cs
+│  ├─ Auth/
+│  │  └─ AuthController.cs
+│  ├─ Game/
+│  │  └─ GameController.cs
+│  ├─ Invites/
+│  │  └─ invitesController.cs
+│  ├─ Leaderboard/
+│  │  └─ LeaderboardController.cs
+│  ├─ Matchmaking/
+│  │  └─ MatchmakingController .cs
+│  ├─ Move/
+│  │  └─ MoveController.cs
+│  ├─ User/
+│  │  └─ UserController.cs
+│  └─ WeatherForecastController.cs
+├─ DbContext/
+│  └─ ChessDbContext.cs
+├─ DTOs/
+│  ├─ AiPerfromance/
+│  │  └─ AiPerfomanceDTOs.cs
+│  ├─ Auth/
+│  │  ├─ AuthDTOs.cs
+│  │  ├─ LoginDTOs.cs
+│  │  └─ RegisterDTOs.cs
+│  ├─ Game/
+│  │  └─ GameDto.cs
+│  ├─ Invites/
+│  │  └─ InviteDTOs.cs
+│  ├─ Leaderboard/
+│  │  └─ LeaderboardDto.cs
+│  ├─ Matchmaking/
+│  │  ├─ CancelQueueDTOs.cs
+│  │  ├─ JoinQueueDTOs.cs
+│  │  └─ MatchFoundDTOs.cs
+│  ├─ Move/
+│  │  └─ MoveDto.cs
+│  ├─ Rating/
+│  │  └─ RatingDto.cs
+│  └─ User/
+│     └─ UserDTOs.cs
+├─ Hubs/
+│  └─ GameHub.cs
+├─ Models/
+│  ├─ ai_performance.cs
+│  ├─ game.cs
+│  ├─ game_state.cs
+│  ├─ matchmaking_queue.cs
+│  ├─ move.cs
+│  └─ user.cs
+├─ Properties/
+│  └─ launchSettings.json
+├─ Services/
+│  ├─ AiPerformance/
+│  │  └─ AiPerformanceService.cs
+│  ├─ Game/
+│  │  └─ GameService.cs
+│  ├─ Interfaces/
+│  │  ├─ IAiPerformance.cs
+│  │  ├─ IAuthService.cs
+│  │  ├─ IGameService.cs
+│  │  ├─ IInviteService.cs
+│  │  ├─ ILeaderboardService.cs
+│  │  ├─ IMatchmakingService.cs
+│  │  ├─ IMoveService.cs
+│  │  ├─ IRatingService.cs
+│  │  └─ IUserService.cs
+│  ├─ Invites/
+│  │  └─ InviteService.cs
+│  ├─ Leaderboard/
+│  │  └─ LeaderboardService.cs
+│  ├─ Login/
+│  │  ├─ AuthService.cs
+│  │  └─ JwtService.cs
+│  ├─ Matchmaking/
+│  │  └─ MatchmakingService.cs
+│  ├─ Move/
+│  │  └─ MoveService.cs
+│  ├─ Rating/
+│  │  └─ RatingService.cs
+│  └─ Users/
+│     └─ UserService.cs
+├─ Validations/
+│  ├─ Auth/
+│  │  ├─ LoginVaildation.cs
+│  │  └─ RegisteVaildation.cs
+│  └─ Matchmaking/
+│     ├─ CancelQueueValidation.cs
+│     └─ JoinQueueValidation.cs
+├─ appsettings.Development.json
+├─ appsettings.json
+├─ bundleconfig.json
+├─ ChessApi.csproj
+├─ ChessApi.http
+├─ ChessApi.sln
+├─ docker-compose.yml
+├─ Dockerfile
+├─ LICENSE.txt
+├─ Program.cs
+├─ README.md
+└─ WeatherForecast.cs
+```
