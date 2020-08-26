
# Host.CreateDefaultBuilder.Example
This is an example of how you can use the new generic Host's CreateDefaultBuilder() method with a Startup.cs pattern

Using the extension methods from Extensions namespace of this project, you can initialize a new generic IHost as so:

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

Your `Startup.cs` file follow this implementation:

 1. Have a public constructor with `IConfiguration` parameter
 2. Have a public method with this signature: `ConfigureServices(IServiceCollection)`

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
			...
		}
	}