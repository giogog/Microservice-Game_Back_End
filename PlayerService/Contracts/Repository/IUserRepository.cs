using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Contracts;

public interface IUserRepository
{
    Task<IdentityResult> CreateUser(User user, string passord);
    Task<User> GetUser(Expression<Func<User, bool>> expression);
    Task<IEnumerable<User>> GetAllUsers();
    Task<IEnumerable<User>> GetUsersWithCondition(Expression<Func<User, bool>> expression);
    Task<IdentityResult> AddToRole(User user, string role);
    Task<bool> CheckPassword(User user, string password);
    Task<User> GetUserFromClaim(ClaimsPrincipal claimsPrincipal);
    Task<IdentityResult> ConfirmEmail(User user, string token);
    Task<IdentityResult> ResetPassword(User user, string token, string newPassword);
    Task<IdentityResult> DeleteUserRole(User user, string role);
    Task UpdateUser(User user);
    IQueryable<User> Users();
}
