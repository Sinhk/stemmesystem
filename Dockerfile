FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Server/Stemmesystem.Server.csproj", "Server/"]
COPY ["Migrations/SqlServerMigrations/SqlServerMigrations.csproj", "SqlServerMigrations/"]
COPY ["Data/StemmeSystem.Data.csproj", "Data/"]
COPY ["Shared/Stemmesystem.Shared.csproj", "Shared/"]
COPY ["Migrations/SqliteMigrations/SqliteMigrations.csproj", "SqliteMigrations/"]
COPY ["Migrations/PostgresMigrations/PostgresMigrations.csproj", "PostgresMigrations/"]
COPY ["Client/Stemmesystem.Client.csproj", "Client/"]
RUN dotnet restore "Server/Stemmesystem.Server.csproj"
COPY . .
WORKDIR "/src/Server"
RUN dotnet build "Stemmesystem.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Stemmesystem.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Stemmesystem.Server.dll"]
