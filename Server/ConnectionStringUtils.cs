using Npgsql;

namespace Stemmesystem.Server;

public class ConnectionStringUtils
{
    
    
    internal static string ParseHerokuPostgresString()
    {
        var databaseUrl = System.Environment.GetEnvironmentVariable("DATABASE_URL");
        if (databaseUrl == null) return string.Empty;
        var databaseUri = new Uri(databaseUrl);
        var userInfo = databaseUri.UserInfo.Split(':');

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = databaseUri.Host,
            Port = databaseUri.Port,
            Username = userInfo[0],
            Password = userInfo[1],
            Database = databaseUri.LocalPath.TrimStart('/'),
            TrustServerCertificate = true,
            SslMode = SslMode.Prefer
        };
        var connectionString = builder.ToString();
        return connectionString;
    }
}