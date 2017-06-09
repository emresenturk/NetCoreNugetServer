using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NugetServer.Models;

namespace NugetServer
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddJsonFile($"ConnectionSettings.json", optional: false)
				.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public IConfigurationRoot Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<IConfiguration>(Configuration);
			// Add framework services.
			//services.AddDbContext<ApplicationDataContext>(options => options.UseNpgsql(Configuration.GetConnectionString("ApplicationDbContext")));
			services.AddDbContext<ApplicationDataContext>(options => options.UseSqlite(Configuration.GetConnectionString("ApplicationDbContext")));
			services.AddLogging();
			services.AddEntityFrameworkSqlite();
			services.AddMvc();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();

			app.UseMvc(routes => { routes.MapRoute(name: "Default", template: "{controller=Index}/{action=Index}/{id?}"); });
		}
	}
}
