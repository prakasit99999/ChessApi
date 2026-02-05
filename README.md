# ChessApi
# โครงสร้างไฟล์

```
ChessApi/
│
├── Areas/
│   └── Identity/
│       └── Pages/
│           └── Account/
│
├── bin/
│   ├── Debug/
│   │   ├── net8.0/
│   │   └── net9.0/
│   └── Release/
│       └── net8.0/
│
├── Controllers/
│   ├── ai_performance/
│   │   └── AiPerformanceController.cs
│   ├── Auth/
│   │   └── AuthController.cs
│   ├── Game/
│   │   └── GameController.cs
│   ├── Matchmaking/
│   │   └── MatchmakingController .cs
│   ├── Move/
│   │   └── MoveController.cs
│   ├── Social/
│   │   └── SocialController.cs
│   ├── User/
│   │   └── UserController.cs
│   └── WeatherForecastController.cs
│
├── DbContext/
│   └── ChessDbContext.cs
│
├── DTOs/
│   ├── AiPerfromance/
│   │   └── AiPerfomanceDTOs.cs
│   ├── Auth/
│   │   ├── AuthDTOs.cs
│   │   ├── LoginDTOs.cs
│   │   └── RegisterDTOs.cs
│   ├── Game/
│   │   ├── GameDto.cs
│   │   ├── GameResulDto.cs
│   │   └── GameStateDto.cs
│   ├── Matchmaking/
│   │   ├── CancelQueueDTOs.cs
│   │   ├── JoinQueueDTOs.cs
│   │   └── MatchFoundDTOs.cs
│   ├── Move/
│   │   └── MoveDto.cs
│   ├── Social/
│   │   └── SocialDto.cs
│   └── User/
│       └── ProfileDTOs.cs
│
├── Hubs/
│   └── GameHub.cs
│
├── Migrations/
│
├── Models/
│   ├── ai_performance.cs
│   ├── friendship.cs
│   ├── game_state.cs
│   ├── game.cs
│   ├── matchmaking_queue.cs
│   ├── move.cs
│   └── user.cs
│
├── obj/
│   ├── Container/
│   ├── Debug/
│   │   ├── net8.0/
│   │   └── net9.0/
│   └── Release/
│       └── net8.0/
│
├── Pages/
│
├── Properties/
│   └── launchSettings.json
│
├── Services/
│   ├── AiPerformance/
│   │   └── AiPerformanceService.cs
│   ├── Game/
│   │   └── GameService.cs
│   ├── Interfaces/
│   │   ├── IAiPerformance.cs
│   │   ├── IAuthService.cs
│   │   ├── IGameService.cs
│   │   ├── IMatchmakingService.cs
│   │   ├── IMoveService.cs
│   │   ├── ISocialService.cs
│   │   └── IUserService.cs
│   ├── Login/
│   │   ├── AuthService.cs
│   │   └── JwtService.cs
│   ├── Matchmaking/
│   │   └── MatchmakingService.cs
│   ├── Move/
│   │   └── MoveService.cs
│   ├── Social/
│   │   └── SocialService.cs
│   └── Users/
│       └── UserService.cs
│
├── Utilities/
│
├── Validations/
│   ├── Auth/
│   │   ├── LoginVaildation.cs
│   │   └── RegisteVaildation.cs
│   ├── Matchmaking/
│   │   ├── CancelQueueValidation.cs
│   │   └── JoinQueueValidation.cs
│   └── User/
│
├── appsettings.Development.json
├── appsettings.json
├── bundleconfig.json
├── ChessApi.csproj
├── ChessApi.csproj.user
├── ChessApi.http
├── ChessApi.sln
├── docker-compose.yml
├── Dockerfile
├── LICENSE.txt
├── Program.cs
├── ScaffoldingReadMe.txt
└── WeatherForecast.cs
```
