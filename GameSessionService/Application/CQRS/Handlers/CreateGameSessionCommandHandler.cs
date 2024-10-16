using GameSession.Application.MediatR.Commands;
using GameSession.Contracts;
using GameSession.Domain.Entities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace GameSession.Application.MediatR.Handlers;

public class CreateGameSessionCommandHandler : IRequestHandler<CreateGameSessionCommand>
{
    private readonly ILogger<CreateGameSessionCommandHandler> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public CreateGameSessionCommandHandler(ILogger<CreateGameSessionCommandHandler> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }
    public async Task Handle(CreateGameSessionCommand request, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repositoryManager = scope.ServiceProvider.GetRequiredService<IRepositoryManager>();
        var session = new Session() 
        {
            Region = request.Region,
            MaxScore = request.MaxScore,
            GameMode = request.GameMode
        };
        session.SetTeams(request.Players.Skip(request.Players.Count() / 2).ToList(),
            request.Players.SkipLast(request.Players.Count() / 2).ToList());

        await repositoryManager.GameSessionRepository.AddAsync(session);
    }


}
