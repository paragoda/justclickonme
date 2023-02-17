namespace Api.Db.Models
{
    public class Link
    {
        public Link(string slug, string userId, string? password, DateTime? expireTime, ulong clicksCount = 0)
        {
            Slug = slug;
            Password = password;
            ExpireTime = expireTime;
            ClicksCount = clicksCount;
            UserId = userId;
        }

        public string Slug { get; set; }

        public string? Password { get; set; }
        public DateTime? ExpireTime { get; set; }
        public ulong ClicksCount { get; set; } = 0;

        public string UserId { get; set; }
    }
}