using Microsoft.AspNetCore.Identity;

namespace Data.Models;

public class User : IdentityUser
{
    public int RefreshTokenVersion { get; set; } = 0;
    public IReadOnlyCollection<Link> Links { get; set; } = default!;
}