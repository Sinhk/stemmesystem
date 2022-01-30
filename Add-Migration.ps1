$name=$args[0]
if(!$name){ 
    "Name is required"
    exit -1;
}
dotnet ef migrations add $name --project Migrations\SqlServerMigrations\ --context StemmesystemContext -- SqlServer
dotnet ef migrations add $name --project Migrations\SqliteMigrations\ --context StemmesystemContext -- Sqlite
dotnet ef migrations add $name --project Migrations\PostgresMigrations\ --context StemmesystemContext -- Postgres