namespace Api.Helpers
{
    public class Secrets
    {
        public string DbConnectionString { get; set; } = string.Empty;
        public string JwtIssuer { get; set; } = string.Empty;

        //public string JwtAudience { get; set; } = string.Empty;
        public string JwtKey { get; set; } = string.Empty;
    }
}