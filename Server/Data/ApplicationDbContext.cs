using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Duende.IdentityServer.EntityFramework.Options;
using Stemmesystem.Server.Models;

namespace Stemmesystem.Server.Data;

public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
    {
    }
}
