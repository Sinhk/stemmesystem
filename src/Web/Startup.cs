using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stemmesystem.Data;
using Stemmesystem.Tools;
using Stemmesystem.Web.Areas.Identity;
using Stemmesystem.Web.Data;
using Stemmesystem.Web.Services;
using Stemmesystem.Web.Services.CSV;

namespace Stemmesystem.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
                     {
                         options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                         options.KnownNetworks.Clear();
                         options.KnownProxies.Clear();
                     });
            
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                //options.LowercaseQueryStrings = true;
            });
            
            services.AddDbContextFactory<StemmesystemContext>(options =>
            {
                if (Environment.IsDevelopment())
                {
                    options.UseSqlite(Configuration.GetConnectionString("DefaultConnection"), x=> x.MigrationsAssembly("SqliteMigrations"));
                    options.EnableSensitiveDataLogging();
                }
                else
                {
                    options.UseSqlServer(Configuration.GetConnectionString("StemmeDb"),x=> x.MigrationsAssembly("SqlServerMigrations"));
                }
            });
            services.AddScoped(p => p.GetRequiredService<IDbContextFactory<StemmesystemContext>>().CreateDbContext());

            #region Authentication
            services.AddDefaultIdentity<IdentityUser>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedAccount = true;
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddEntityFrameworkStores<StemmesystemContext>();


            var authBuilder = services.AddAuthentication();

            var googleAuthNSection = Configuration.GetSection("Authentication:Google");
            if (googleAuthNSection.GetChildren().Any())
            {
                authBuilder.AddGoogle(options =>
                {
                    options.ClientId = googleAuthNSection["ClientId"];
                    options.ClientSecret = googleAuthNSection["ClientSecret"];
                });
            }

            services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
            #endregion

            services.AddRazorPages();
            services.AddServerSideBlazor();
            
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddSignalR();
            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
            

            if (Environment.IsProduction())
            {
                services.AddDataProtection()
                    .PersistKeysToDbContext<StemmesystemContext>();
            }

            services.AddScoped<ArrangementService>();
            services.AddScoped<IDelegatService, DelegatService>();
            services.AddScoped<ISakService, SakService>();
            services.AddScoped<StemmeService>();
            services.AddSingleton<IKeyGenerator, RngKeyGenerator>();
            services.AddSingleton<IKeyHasher, KeyHasher>();
            services.AddTransient<CsvImport>();
            services.AddSingleton<ActiveTracker>();

            services.AddHttpClient<ISmsSender, ClickSendSender>();
            services.AddOptions<SveveOptions>()
                .BindConfiguration("Sveve")
                .ValidateDataAnnotations()
                ;

            services.AddOptions<ClickSendOptions>()
                .BindConfiguration("ClickSend")
                .ValidateDataAnnotations()
                ;
            
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IEpostSender, EmailSender>();
            services.AddOptions<SendMailOptions>()
                .BindConfiguration("SendGrid")
                .ValidateDataAnnotations()
                ;

            services.AddTransient<IPinSender, PinSender>();

            services.AddSingleton<NotificationManager>();
            
            services.AddAutoMapper(typeof(AutoMapperConfig));
        }

        /*
        private static string ParseHerokuPostgresString()
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
        */

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMapper mapper)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                mapper.ConfigurationProvider.AssertConfigurationIsValid();
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseForwardedHeaders();
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
