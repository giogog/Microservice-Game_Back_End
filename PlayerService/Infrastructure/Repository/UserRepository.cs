using Contracts;
using Domain.Entities;
using Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Infrastructure.Repository;

public class UserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext context;

    public UserRepository(UserManager<User> userManager) => _userManager = userManager;

    public UserRepository(ApplicationDbContext context)
    {
        this.context = context;
    }
    public async Task<IdentityResult> AddToRole(User user, string roles) => await _userManager.AddToRoleAsync(user, roles);
    public async Task<IdentityResult> DeleteUserRole(User user, string role) => await _userManager.RemoveFromRoleAsync(user, role);

    public async Task<bool> CheckPassword(User user, string password) => await _userManager.CheckPasswordAsync(user, password);

    public async Task<IdentityResult> CreateUser(User user, string passord) => await _userManager.CreateAsync(user, passord);
    public async Task<IEnumerable<User>> GetAllUsers() => await _userManager.Users.ToListAsync();
    public async Task<IEnumerable<User>> GetUsersWithCondition(Expression<Func<User, bool>> expression) => await _userManager.Users.Where(expression).ToListAsync();
    public async Task<User> GetUser(Expression<Func<User, bool>> expression) => await _userManager.Users.FirstOrDefaultAsync(expression);
    public async Task<User> GetUserFromClaim(ClaimsPrincipal claimsPrincipal) => await _userManager.GetUserAsync(claimsPrincipal);
    public async Task<IdentityResult> ConfirmEmail(User user, string token) => await _userManager.ConfirmEmailAsync(user, token);

    public async Task<IdentityResult> ResetPassword(User user, string token, string newPassword) => await _userManager.ResetPasswordAsync(user, token, newPassword);
    public async Task UpdateUser(User user) => await _userManager.UpdateAsync(user);
    public IQueryable<User> Users() => _userManager.Users;


}
