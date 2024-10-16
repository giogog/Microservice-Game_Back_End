using Contracts;
using Domain.Entities;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Application.Services
{
    public class AuthorizationService : IAuthService
    {
        private readonly ITokenGenerator _tokenGenerator;
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(
            ITokenGenerator tokenGenerator,
            IRepositoryManager repositoryManager,
            ILogger<AuthorizationService> logger)
        {
            _tokenGenerator = tokenGenerator;
            _repositoryManager = repositoryManager;
            _logger = logger;
        }

        public async Task<LoginResponseDto> Authenticate(Expression<Func<User, bool>> expression)
        {
            _logger.LogInformation("Authenticating user.");

            var user = await _repositoryManager.UserRepository.GetUser(expression);
            if (user == null)
            {
                _logger.LogWarning("User not found for the provided expression.");
                throw new NotFoundException("User not found.");
            }

            _logger.LogInformation("User {Username} found. Generating token.", user.UserName);

            var token = await _tokenGenerator.GenerateToken(user);

            _logger.LogInformation("Token generated for user {Username}.", user.UserName);

            return new LoginResponseDto(user.Id, user.UserName, token);
        }

        public async Task<IdentityResult> Register(RegisterDto registerDto)
        {
            _logger.LogInformation("Registering user {Username}.", registerDto.Username);

            await _repositoryManager.BeginTransactionAsync();

            try
            {
                var user = new User
                {
                    UserName = registerDto.Username,
                    Email = registerDto.Email,
                    Region = registerDto.Region
                };

                var result = await _repositoryManager.UserRepository.CreateUser(user, registerDto.Password);

                var roleAssignmentResult = await AssignRoleToUser(user, "Player");
                if (!roleAssignmentResult.Succeeded)
                {
                    await _repositoryManager.RollbackTransactionAsync();
                    return roleAssignmentResult;
                }

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Player registration failed for {Username}: {Errors}", registerDto.Username, result.Errors);
                    await _repositoryManager.RollbackTransactionAsync();
                    return result;
                }

                await _repositoryManager.CommitTransactionAsync();

                _logger.LogInformation("Player {Username} registered successfully.", registerDto.Username);

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user registration for {Username}", registerDto.Username);
                await _repositoryManager.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<IdentityResult> Login(LoginDto loginDto)
        {
            _logger.LogInformation("Logging in user {Username}.", loginDto.Username);

            var user = await _repositoryManager.UserRepository.GetUser(user => user.UserName == loginDto.Username);

            if (user == null)
            {
                _logger.LogWarning("Player {Username} does not exist.", loginDto.Username);
                return IdentityResult.Failed(new IdentityError { Code = "UserDoesNotExist", Description = "The user does not exist." });
            }

            var passwordCheck = await _repositoryManager.UserRepository.CheckPassword(user, loginDto.Password);
            if (!passwordCheck)
            {
                _logger.LogWarning("Incorrect password for user {Username}.", loginDto.Username);
                return IdentityResult.Failed(new IdentityError { Code = "IncorrectPassword", Description = "Incorrect password." });
            }

            _logger.LogInformation("Player {Username} logged in successfully.", loginDto.Username);

            return IdentityResult.Success;
        }

        public async Task<User> GetUserWithClaim(ClaimsPrincipal principal)
        {
            var user = await _repositoryManager.UserRepository.GetUserFromClaim(principal);
            if (user == null)
            {
                _logger.LogWarning("Player not found with the given claims.");
                throw new NotFoundException("Player not found");
            }

            return user;
        }

        private async Task<IdentityResult> AssignRoleToUser(User user, string roleName)
        {
            _logger.LogInformation("Assigning role {Role} to user {Username}.", roleName, user.UserName);

            if (!await _repositoryManager.RoleRepository.UserRoleExists(roleName))
            {
                _logger.LogError("Role {Role} does not exist.", roleName);
                return IdentityResult.Failed(new IdentityError { Code = "RoleNotExists", Description = "Role does not exist." });
            }

            var result = await _repositoryManager.UserRepository.AddToRole(user, roleName);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to assign role {Role} to user {Username}: {Errors}", roleName, user.UserName, result.Errors);
            }
            else
            {
                _logger.LogInformation("Role {Role} assigned to user {Username} successfully.", roleName, user.UserName);
            }

            return result;
        }

    }
}
