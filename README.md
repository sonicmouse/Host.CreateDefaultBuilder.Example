
# Host.CreateDefaultBuilder.Example
This is an example of how you can use the new generic Host's `CreateDefaultBuilder()` method with a `Startup.cs` pattern.

Using the extension methods from Extensions namespace of this project, you can initialize a new generic `IHost` as so:

    using CreateDefaultHostBuilderExample.Extensions;
    using Microsoft.Extensions.Hosting;
    using System.Threading.Tasks;
    
    class Program
    {
    	static async Task Main(string[] args)
    	{
    		var host = CreateHostBuilder(args).Build();
    		await host.RunAsync();
    	}
    
    	public static IHostBuilder CreateHostBuilder(string[] args) =>
    		Host.CreateDefaultBuilder(args)
    		.UseStartup<Startup>();
    }

Your `Startup.cs` file must follow this specification:

 1. Implement an **optional** public constructor with an `IConfiguration` parameter.
 2. **Should** implement a public method with this signature: `void ConfigureServices(IServiceCollection)`.

For example:

	public class Startup
	{
		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			// Inject a configuration:
			services.Configure<MySettingsModel>(
				Configuration.GetSection("SettingsSection"));

			// Inject your services, too. For example:
			// services.AddHostedService<MyBackgroundService>();
			// services.AddTransient<IMyService, MyService>();
		}
	}

