using Microsoft.AspNetCore.Identity;

namespace Api.Db.Models
{
    public class User : IdentityUser
    {
        public IReadOnlyCollection<Link> Links { get; set; }
    }
}