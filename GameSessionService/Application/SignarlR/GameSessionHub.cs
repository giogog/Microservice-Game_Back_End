using GameSession.Contracts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace GameSession.Application.SignalR;
public class GameSessionHub : Hub
{
    private readonly ILogger<GameSessionHub> _logger;

    public GameSessionHub(ILogger<GameSessionHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("A client connected with connection ID: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        _logger.LogInformation("A client disconnected with connection ID: {ConnectionId}", Context.ConnectionId);
        if (exception != null)
        {
            _logger.LogError(exception, "An error occurred during disconnection.");
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task AddToSessionGroup(string sessionId)
    {
        string userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(userId, sessionId);
            _logger.LogInformation($"Connection {userId} added to Session with Id {sessionId}.");
        }

    }

    public async Task RemoveFromSessionGroup(string sessionId)
    {

        string userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(userId, sessionId);
            _logger.LogInformation($"Connection {userId} removed from Session with Id {sessionId}.");
        }
    }

    public async Task GameOver(string sessionId, int[] Score) // Ensure method name is "NewUser"
    {
        if (sessionId == null)
        {
            _logger.LogError("sessionId is a null");
            throw new ArgumentNullException(nameof(sessionId));
        }

        await Clients.Group(sessionId).SendAsync("GameOver",$"Score Team1 : {Score[0]} - {Score[1]} : Team2");
        _logger.LogInformation($"Session ended with Score Team1 : {Score[0]} - {Score[1]} : Team2");
    }

    public async Task RoundOver(string sessionId, int[] Score) // Ensure method name is "NewUser"
    {
        if (sessionId == null)
        {
            _logger.LogError("sessionId is a null");
            throw new ArgumentNullException(nameof(sessionId));
        }

        await Clients.Group(sessionId).SendAsync("RoundOver", $"Score Team1 : {Score[0]} - {Score[1]} : Team2");
        _logger.LogInformation($"Round ended with Score Team1 : {Score[0]} - {Score[1]} : Team2");
    }
}