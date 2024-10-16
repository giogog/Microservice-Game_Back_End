using GameSession.Domain.Entities;
using GameSession.Domain.Models;
using System.Collections.Concurrent;

namespace GameSession.Contracts;

public interface IGamesessionService
{
    Task<Session?> InitializeGameSession(string sessionId);
    Task UpdateSession(Session session);

    event EventHandler<KillActionEventArgs> PlayerKilled;

    event EventHandler<CheckStatsEventArgs> CheckStats;

    delegate void GameStateDelegate(Session session,bool roundOver);

    Task CheckSessionState((Session, List<KillAction>) sessionData, GameStateDelegate callback);

    Task<Dictionary<string, PlayerStatsDto>> CheckSessionPlayerStats(Session session, List<KillAction> actions);

    void InvokeKillAction(KillActionEventArgs e);
    void InvokePlayerStats(CheckStatsEventArgs e);
}
