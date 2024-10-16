using Contracts;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/Player/[controller]")]
[ApiController]
public class ApiController : ControllerBase
{
    protected readonly IAuthService _authService;
    protected readonly IEventPublisher _eventPublisher;
    protected ApiResponse _response;

    public ApiController(IAuthService authService) => _authService = authService;
    public ApiController(IEventPublisher eventPublisher, IAuthService authService)
    {
        _authService = authService;
        _eventPublisher = eventPublisher;
    }
    

}
