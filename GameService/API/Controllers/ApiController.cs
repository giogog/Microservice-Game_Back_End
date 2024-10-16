using Game.API;
using Game.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/Game/[controller]")]
[ApiController]
public class ApiController : ControllerBase
{
    protected readonly IEventPublisher _eventPublisher;
    protected readonly IRepositoryManager _repositoryManager;
    protected ApiResponse _response;

    public ApiController(IRepositoryManager repositoryManager,IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
        _repositoryManager = repositoryManager;
    }



}
