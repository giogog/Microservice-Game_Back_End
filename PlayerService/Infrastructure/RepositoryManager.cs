using Contracts;
using Domain.Entities;
using Infrastructure.Context;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure;

public class RepositoryManager : IRepositoryManager
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction _transaction;
    private readonly Lazy<IUserRepository> _userRepository;
    private readonly Lazy<IRoleRepository> _roleRepository;
    public RepositoryManager(ApplicationDbContext context, UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        _context = context;
        _userRepository = new(() => new UserRepository(userManager));
        _roleRepository = new(() => new RoleRepository(roleManager));
    }
    public IUserRepository UserRepository => _userRepository.Value;
    public IRoleRepository RoleRepository => _roleRepository.Value;
    public async Task BeginTransactionAsync() => _transaction = await _context.Database.BeginTransactionAsync();
    public async Task CommitTransactionAsync() => await _transaction.CommitAsync();
    public async Task RollbackTransactionAsync() => await _transaction.RollbackAsync();
    public void Dispose() => _transaction?.Dispose();
    public Task SaveAsync() => _context.SaveChangesAsync();
}
