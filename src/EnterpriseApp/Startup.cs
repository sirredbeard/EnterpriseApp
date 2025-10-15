using Autofac;
using EnterpriseApp.Data;
using EnterpriseApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace EnterpriseApp;

public class Startup
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public Startup(IConfiguration config, IWebHostEnvironment env)
    {
        _config = config;
        _env = env;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Controllers with Newtonsoft.Json configured
        services.AddControllers()
            .AddNewtonsoftJson();

        // IdentityServer4 in-memory config for sample only. IdentityServer4 is unmaintained for later .NET versions. It's here for a reason. Don't use it!
        services.AddIdentityServer()
            .AddInMemoryClients(new[] {
                new IdentityServer4.Models.Client
                {
                    ClientId = "client",
                    AllowedGrantTypes = IdentityServer4.Models.GrantTypes.ClientCredentials,
                    // Disable the secret requirement to keep things simple, do not do this in prod!
                    RequireClientSecret = false,
                    AllowedScopes = { "api" }
                }
            })
            .AddInMemoryApiScopes(new[] { new IdentityServer4.Models.ApiScope("api", "Demo API") })
            // Also register an API resource so access tokens include the 'aud' (audience) claim
            .AddInMemoryApiResources(new[] { new IdentityServer4.Models.ApiResource("api", "Demo API") { Scopes = { "api" } } })
            .AddDeveloperSigningCredential();

        // Configure authentication to accept JWT tokens from IdentityServer
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Run IdentityServer4 on the same host and disable HTTPS requirement
                options.Authority = _config.GetValue<string>("Authority") ?? "http://localhost:5000";
                options.RequireHttpsMetadata = false;
                options.Audience = "api";
            });

        // EF Core with SQLite
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlite("Data Source=app.db"));

    // ProductService is registered in the Autofac module; no built-in DI registration needed

        // Add swagger for quick testing
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c => {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "EnterpriseApp API", Version = "v1" });
        });
    }

    // Allow adding things to Autofac container
    public void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterModule(new AutofacModule());
    }

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Serve a friendly static landing page at '/' from wwwroot/index.html
        app.UseDefaultFiles();
        app.UseStaticFiles();

        // Always enable Swagger/UI for this sample (You normally wouldn't do this in prod!)
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EnterpriseApp v1"));

    app.UseRouting();

    // Enable IdentityServer endpoints (token, discovery)
    app.UseIdentityServer();

    app.UseAuthentication();
    app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    private class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ProductService>().As<IProductService>().InstancePerLifetimeScope();
        }
    }
}
