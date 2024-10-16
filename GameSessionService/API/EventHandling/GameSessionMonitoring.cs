using GameSession.Application.SignalR;
using GameSession.Contracts;
using GameSession.Contracts.Service;
using GameSession.Domain.Entities;
using GameSession.Infrastructure.GrpcClient;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace GameSession.API.EventHandling;

public class GameSessionMonitoring : BackgroundService
{
    private ConcurrentDictionary<Session, List<KillAction>> _gameSessions;

    // Services
    private readonly IGamesessionService _gamesessionService;
    
    private readonly IHubContext<GameSessionHub> _hubContext;
    private readonly IMediator _mediator;
    private readonly ILogger<GameSessionMonitoring> _logger;
    private readonly IPlayerService _playerService;

    public GameSessionMonitoring(IGamesessionService gamesessionService, IHubContext<GameSessionHub> hubContext, IMediator mediator, ILogger<GameSessionMonitoring> logger,IPlayerService playerService)
    {
        _gamesessionService = gamesessionService;
        _gameSessions = new ConcurrentDictionary<Session, List<KillAction>>();
        _hubContext = hubContext;
        _mediator = mediator;
        _logger = logger;
        _playerService = playerService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        // Log that the background service has started
        _logger.LogInformation("GameSessionMonitoring background service is starting.");

        _gamesessionService.PlayerKilled += async (sender, action) =>
        {
            _logger.LogInformation($"PlayerKilled event received. SessionId: {action.SessionId}, KillerId: {action.KillerId}, KilledId: {action.KilledId}");

            // Retrieve the current session or initialize a new one if not found
            var currentSession = _gameSessions.FirstOrDefault(s => s.Key.Id == action.SessionId).Key;

            if (currentSession == null)
            {
                _logger.LogInformation($"Initializing new game session. SessionId: {action.SessionId}");
                currentSession = await _gamesessionService.InitializeGameSession(action.SessionId);
                _gameSessions.TryAdd(currentSession, new List<KillAction>());
            }

            // Add the new kill action to the session's record
            _gameSessions[currentSession].Add(
                new KillAction
                {
                    KilledId = action.KilledId,
                    CurrentRoud = currentSession.CurrentRound,
                    KillerId = action.KillerId,
                    SessionId = currentSession.Id
                });

            _logger.LogInformation($"KillAction recorded. SessionId: {currentSession.Id}, KillerId: {action.KillerId}, KilledId: {action.KilledId}");

            // Check the session state and handle game round or game over scenarios
            await _gamesessionService.CheckSessionState((currentSession, _gameSessions[currentSession]), async (session, roundOver) =>
            {
                if (roundOver)
                {
                    if (session.Score[0] == session.MaxScore || session.Score[1] == session.MaxScore)
                    {
                        //Game over
                        await _hubContext.Clients.Group(session.Id).SendAsync("GameOver", $"Score Team1 : {session.Score[0]} - {session.Score[1]} : Team2");
                        if(session.Score[0] > session.Score[1])
                        {
                            await Task.WhenAll(_playerService.UpdatePlayers(session.Team1,true), _playerService.UpdatePlayers(session.Team2,false));
                        }
                        else
                        {
                            await Task.WhenAll(_playerService.UpdatePlayers(session.Team1, false), _playerService.UpdatePlayers(session.Team2, true));
                        }
                        
                        session.EndTime = DateTime.UtcNow;
                        _gameSessions.TryRemove(session, out _);

                    }
                    else
                    {
                        //Game Continues
                        await _hubContext.Clients.Group(session.Id).SendAsync("RoundOver", $"Score Team1 : {session.Score[0]} - {session.Score[1]} : Team2");
                        _logger.LogInformation($"Round over. Game continues. SessionId: {session.Id}, Score - Team1: {session.Score[0]}, Team2: {session.Score[1]}");

                    }
                    await _gamesessionService.UpdateSession(session);
                }
                //Game Continues
            });


        };

        _gamesessionService.CheckStats += async (sender, checkstats) =>
        {
            var currentSession = _gameSessions.FirstOrDefault(s => s.Key.Id == checkstats.SessionId).Key;

            if (currentSession == null)
            {
                currentSession = await _gamesessionService.InitializeGameSession(checkstats.SessionId);
                _gameSessions.TryAdd(currentSession, new List<KillAction>());
            }
            var stats = await _gamesessionService.CheckSessionPlayerStats(currentSession, _gameSessions[currentSession]);

            await _hubContext.Clients.Client(checkstats.PlayerId.ToString()).SendAsync("Stats", stats);


        };
        _logger.LogInformation("GameSessionMonitoring background service is ready.");
        return Task.CompletedTask;
    }
}
