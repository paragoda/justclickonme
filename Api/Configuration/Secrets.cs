namespace Api.Helpers
{
    public class Secrets
    {
        public string DbConnectionString { get; set; } = string.Empty;

        public string JwtIssuer { get; set; } = string.Empty;
        public string JwtAccessSecret { get; set; } = string.Empty;
        public string JwtRefreshSecret { get; set; } = string.Empty;

        public string GoogleClientId { get; set; } = string.Empty;
        public string GoogleClientSecret { get; set; } = string.Empty;
    }
}