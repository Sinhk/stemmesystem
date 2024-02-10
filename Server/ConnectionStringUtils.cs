using Npgsql;

namespace Stemmesystem.Server;

public class ConnectionStringUtils
{
    internal static string? ParseHerokuPostgresString()
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (databaseUrl == null) return null;
        var databaseUri = new Uri(databaseUrl);
        var userInfo = databaseUri.UserInfo.Split(':');

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = databaseUri.Host,
            Port = databaseUri.Port,
            Username = userInfo[0],
            Password = userInfo[1],
            Database = databaseUri.LocalPath.TrimStart('/'),
            SslMode = SslMode.Prefer
        };
        var connectionString = builder.ToString();
        return connectionString;
    }
}