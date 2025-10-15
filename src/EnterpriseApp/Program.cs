using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using EnterpriseApp;
using EnterpriseApp.Data;
using EnterpriseApp.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Use Autofac as the service provider
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Add services to the container via Startup
var startup = new Startup(builder.Configuration, builder.Environment);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<EnterpriseApp.Data.AppDbContext>();
	db.Database.EnsureCreated();
	if (!db.Products.Any())
	{
		db.Products.AddRange(new[] {
			new EnterpriseApp.Models.Product { Name = "NES for .NET 6", Price = 100M },
			new EnterpriseApp.Models.Product { Name = "NES for .NET 8", Price = 100M },
		});
		db.SaveChanges();
	}
}

// Configure the HTTP request pipeline
startup.Configure(app, app.Environment);


// Intentionally set a Windows-style data path to create a cross-platform issue in addition to our image creation issue
Environment.SetEnvironmentVariable("APP_DATA_PATH", "C:\\Windows\\Temp\\appdb");

// Run!
app.Run();

// Expose an empty partial Program class so integration tests can reference the entry point
public partial class Program { }
