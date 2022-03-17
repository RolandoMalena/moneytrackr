using Npgsql;

namespace MoneyTrackr.Data.Settings
{
    public class DatabaseSettings
    {
        public string Username { get; set; } = "postgres";
        public string Password { get; set; } = "password";
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5432;
        public string Database { get; set; } = "postgres";
        public SslMode SslMode { get; set; } = SslMode.Prefer;
        public bool TrustServerCertificate { get; set; } = true;
    }
}
