$name=$args[0]
if(!$name){ 
    "Name is required"
    exit -1;
}
dotnet ef migrations add $name --startup-project src\Web --project src\Migrations\SqlServerMigrations\ -- SqlServer
dotnet ef migrations add $name --startup-project src\Web --project src\Migrations\SqliteMigrations\ -- Sqlite
dotnet ef migrations add $name --startup-project src\Web --project src\Migrations\PostgresMigrations\ -- Postgres