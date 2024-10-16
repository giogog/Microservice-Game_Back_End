using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class User : IdentityUser<Guid>
{
    public int Experiance { get; set; } = 0;
    public int Level { get; set; } = 1;
    public decimal Money { get; set; } = 0;
    public required string Region { get; set; }
    public ICollection<UserRole>? Roles { get; set; }
}
