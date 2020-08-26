using CreateDefaultHostBuilderExample.Models.AppSettingsModels;
using CreateDefaultHostBuilderExample.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CreateDefaultHostBuilderExample
{
	public class Startup
	{
		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			// inject some settings
			services.Configure<CountSheepSettingsModel>(
				Configuration.GetSection("CountSheepSettings"));

			// inject an IHostedService that depends on the above settings
			services.AddHostedService<CountSheepService>();

			// inject a BackgroundService
			services.AddHostedService<SheepQueueService>();

			// inject some transient services
			services.AddTransient<IDadJokeService, DadJokeService>();
			services.AddTransient<ISheepQueueService, SheepQueueService>();

			// inject a singleton
			services.AddSingleton<ISheepStorageService, SheepStorageService>();

			// add in an IHttpClientFactory
			services.AddHttpClient();

			// add in IMemoryCache
			services.AddMemoryCache();
		}
	}
}
