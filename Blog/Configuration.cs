namespace Blog
{
    public static class Configuration
    {
        public static string JwtKey { get; set; } = "p9cg2qf8bStBX9lDSEWWVKfxUZHE0btQZckd";
        public static string ApiKeyName = "api-key";
        public static string ApiKey = "curso_api_TI45SELe9aUdCF";
        public static SmtpConfiguration Smtp = new();

        public class SmtpConfiguration
        {
            public string Host { get; set; }
            public int Port { get; set; } = 25;
            public string UserName { get; set; }
            public string Password { get; set; }
        }
    }
}
