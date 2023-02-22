using Microsoft.AspNetCore.Identity;

namespace Data.Models;

public class User : IdentityUser
{
    public IReadOnlyCollection<Link> Links { get; set; } = default!;
}