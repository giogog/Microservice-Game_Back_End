namespace Contracts;

public interface IRepositoryManager
{
    IUserRepository UserRepository { get; }
    IRoleRepository RoleRepository { get; }
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task SaveAsync();
}
