$name=$args[0]
if(!$name){ 
    "Name is required"
    exit -1;
}
dotnet ef migrations add $name --project Migrations\SqlServerMigrations\ --startup-project Server --context StemmesystemContext -- --provider SqlServer
dotnet ef migrations add $name --project Migrations\SqliteMigrations\ --startup-project Server --context StemmesystemContext -- --provider Sqlite
dotnet ef migrations add $name --project Migrations\PostgresMigrations\ --startup-project Server --context StemmesystemContext -- --provider Postgres