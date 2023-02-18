namespace Api.Helpers
{
    public class Secrets
    {
        public Secrets(string dbConnectionString, string jwtIssuer, string jwtAudience, string jwtKey)
        {
            DbConnectionString = dbConnectionString;
            JwtIssuer = jwtIssuer;
            JwtAudience = jwtAudience;
            JwtKey = jwtKey;
        }

        public string DbConnectionString { get; }
        public string JwtIssuer { get; }
        public string JwtAudience { get; }
        public string JwtKey { get; }
    }
}