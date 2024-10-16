namespace GameSession.Contracts;

public interface ISignalRHub
{
    Task GameOver(string sessionId, int[] Score);
    Task RoundOver(string sessionId, int[] Score);
}
