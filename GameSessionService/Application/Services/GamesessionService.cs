using GameSession.Contracts;
using GameSession.Domain.Entities;
using GameSession.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace GameSession.Application.Services;

public class GamesessionService : IGamesessionService
{
    // Events for handling game actions and stats
    public event EventHandler<KillActionEventArgs> PlayerKilled;
    public event EventHandler<CheckStatsEventArgs> CheckStats;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GamesessionService> _logger;

    public GamesessionService(IServiceScopeFactory scopeFactory, ILogger<GamesessionService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    // Initializes a game session by fetching the session data from the repository.
    public async Task<Session?> InitializeGameSession(string sessionId)
    {
        _logger.LogInformation($"Initializing game session with ID: {sessionId}");
        using var scope = _scopeFactory.CreateScope();
        var repositoryManager = scope.ServiceProvider.GetRequiredService<IRepositoryManager>();
        var session = await repositoryManager.GameSessionRepository.GetByIdAsync(sessionId);

        if (session != null)
        {
            _logger.LogInformation($"Game session {sessionId} initialized successfully.");
        }
        else
        {
            _logger.LogWarning($"Game session {sessionId} could not be found.");
        }

        return session;
    }

    // Updates the session data in the repository.
    public async Task UpdateSession(Session session)
    {
        _logger.LogInformation($"Updating session with ID: {session.Id}");
        using var scope = _scopeFactory.CreateScope();
        var repositoryManager = scope.ServiceProvider.GetRequiredService<IRepositoryManager>();
        await repositoryManager.GameSessionRepository.UpdateAsync(session.Id, session);
        _logger.LogInformation($"Session {session.Id} updated successfully.");
    }

    // Checks player stats based on the actions performed during the game session.
    public async Task<Dictionary<string, PlayerStatsDto>> CheckSessionPlayerStats(Session session, List<KillAction> actions)
    {
        _logger.LogInformation($"Checking player stats for session ID: {session.Id}");
        ConcurrentDictionary<string, PlayerStatsDto> PlayerStatistics = new();

        var killsLookup = actions.GroupBy(a => a.KillerId).ToDictionary(g => g.Key, g => g.Count());
        var deathsLookup = actions.GroupBy(a => a.KilledId).ToDictionary(g => g.Key, g => g.Count());

        var team1 = session.GetTeam1();
        var team2 = session.GetTeam2();

        await Task.WhenAll(
            Task.Run(() =>
            {
                Parallel.ForEach(team1, player =>
                {
                    int kills = killsLookup.TryGetValue(player.Id, out var killCount) ? killCount : 0;
                    int deaths = deathsLookup.TryGetValue(player.Id, out var deathCount) ? deathCount : 0;
                    PlayerStatistics.TryAdd(player.UserName, new PlayerStatsDto("Team1", kills, deaths));
                });
            }),
            Task.Run(() =>
            {
                Parallel.ForEach(team2, player =>
                {
                    int kills = killsLookup.TryGetValue(player.Id, out var killCount) ? killCount : 0;
                    int deaths = deathsLookup.TryGetValue(player.Id, out var deathCount) ? deathCount : 0;
                    PlayerStatistics.TryAdd(player.UserName, new PlayerStatsDto("Team2", kills, deaths));
                });
            })
        );

        _logger.LogInformation($"PlayerDto stats computed for session ID: {session.Id}");
        return PlayerStatistics.ToDictionary();
    }

    //Checks the current state of the game session, determines if a round is over, and updates scores.

    public async Task CheckSessionState((Session, List<KillAction>) sessionData, IGamesessionService.GameStateDelegate callback)
    {
        _logger.LogInformation($"Checking session state for session ID: {sessionData.Item1.Id}");

        var actions = sessionData.Item2;
        var session = sessionData.Item1;

        var roundDeaths = actions.Where(a => a.CurrentRoud == session.CurrentRound).Select(a => a.KilledId).ToArray();
        bool roundOver = false;

        var team1 = session.GetTeam1();
        var team2 = session.GetTeam2();

        if (team1.All(p => roundDeaths.Contains(p.Id)))
        {
            session.Score[1]++;
            roundOver = true;
            session.CurrentRound++;
            _logger.LogInformation($"Round over: Team1 defeated. Session ID: {session.Id}");
        }
        if (team2.All(p => roundDeaths.Contains(p.Id)))
        {
            session.Score[0]++;
            roundOver = true;
            session.CurrentRound++;
            _logger.LogInformation($"Round over: Team2 defeated. Session ID: {session.Id}");
        }

        callback(session, roundOver);
    }


    public void InvokeKillAction(KillActionEventArgs e)
    {
        _logger.LogInformation($"Invoking KillAction event for Session ID: {e.SessionId}");
        PlayerKilled?.Invoke(this, e);
    }

    public void InvokePlayerStats(CheckStatsEventArgs e)
    {
        _logger.LogInformation($"Invoking CheckStats event for Session ID: {e.SessionId}");
        CheckStats?.Invoke(this, e);
    }
}
