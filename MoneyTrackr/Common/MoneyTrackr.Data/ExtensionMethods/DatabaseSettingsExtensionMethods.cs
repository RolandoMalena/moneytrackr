using Microsoft.Extensions.Configuration;
using MoneyTrackr.Data.Settings;
using Npgsql;

namespace MoneyTrackr.Data.ExtensionMethods
{
    public static class DatabaseSettingsExtensionMethods
    {
        private static readonly string DatabaseSettingsArea = "Database";

        private static readonly string Username = DatabaseSettingsArea + ConfigurationPath.KeyDelimiter + "Username";
        private static readonly string Password = DatabaseSettingsArea + ConfigurationPath.KeyDelimiter + "Password";
        private static readonly string Host = DatabaseSettingsArea + ConfigurationPath.KeyDelimiter + "Host";
        private static readonly string Port = DatabaseSettingsArea + ConfigurationPath.KeyDelimiter + "Port";
        private static readonly string Database = DatabaseSettingsArea + ConfigurationPath.KeyDelimiter + "Database";
        private static readonly string SslMode = DatabaseSettingsArea + ConfigurationPath.KeyDelimiter + "SslMode";
        private static readonly string TrustServerCertificate = DatabaseSettingsArea + ConfigurationPath.KeyDelimiter + "TrustServerCertificate";

        public static void MapConfiguration(this DatabaseSettings settings, IConfiguration config)
        {
            settings.Username = config[Username] ?? settings.Username;
            settings.Password = config[Password] ?? settings.Password;
            settings.Host = config[Host] ?? settings.Host;
            settings.Port = int.TryParse(config[Port], out var port) ? port : settings.Port;
            settings.Database = config[Database] ?? settings.Database;
            settings.SslMode = Enum.TryParse(config[SslMode], out SslMode ssl) ? ssl : settings.SslMode;
            settings.TrustServerCertificate = bool.TryParse(config[TrustServerCertificate], out var trustServerCertificate) ? trustServerCertificate : settings.TrustServerCertificate;
        }

        public static string GetConnectionString(this DatabaseSettings settings)
        {
            NpgsqlConnectionStringBuilder builder = new()
            {
                Username = settings.Username,
                Password = settings.Password,
                Host = settings.Host,
                Port = settings.Port,
                Database = settings.Database,
                SslMode = settings.SslMode,
                TrustServerCertificate = settings.TrustServerCertificate
            };

            return builder.ConnectionString;
        }
    }
}
