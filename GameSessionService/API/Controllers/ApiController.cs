using GameSession.API;
using GameSession.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Game.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ApiController : ControllerBase
{
    protected readonly IRepositoryManager _repositoryManager;
    protected readonly IMediator _mediator;
    protected ApiResponse _response;
    public ApiController(IRepositoryManager repositoryManager) => _repositoryManager = repositoryManager;
    public ApiController(IMediator mediator) => _mediator = mediator;
    public ApiController(IRepositoryManager repositoryManager, IMediator mediator)
    {
        _repositoryManager = repositoryManager;
        _mediator = mediator;
    }
}
