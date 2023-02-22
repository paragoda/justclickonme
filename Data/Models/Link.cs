namespace Data.Models;

public class Link
{
    public Link(string slug, string destination, string userId)
    {
        Slug = slug;
        Destination = destination;
        UserId = userId;
    }

    public string Slug { get; set; }
    public string Destination { get; set; }

    //public string? Password { get; set; }
    //public DateTime? ExpireTime { get; set; }
    //public ulong ClicksCount { get; set; } = 0;

    public string UserId { get; set; }
}