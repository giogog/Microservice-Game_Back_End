using Domain.Entities;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Contracts;

public interface IAuthService
{
    Task<IdentityResult> Login(LoginDto loginDto);
    Task<LoginResponseDto> Authenticate(Expression<Func<User, bool>> expression);
    Task<IdentityResult> Register(RegisterDto registerDto);
    Task<User> GetUserWithClaim(ClaimsPrincipal principal);

}
