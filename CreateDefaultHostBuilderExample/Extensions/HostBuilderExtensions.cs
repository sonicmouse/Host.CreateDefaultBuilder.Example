using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace CreateDefaultHostBuilderExample.Extensions
{
	/// <summary>
	/// Extensions to emulate a typical "Startup.cs" pattern for <see cref="IHostBuilder"/>
	/// </summary>
	public static class HostBuilderExtensions
	{
		/// <summary>
		/// Specify the startup type to be used by the host.
		/// </summary>
		/// <typeparam name="TStartup">The type containing a constructor
		/// with <see cref="IConfiguration"/> parameter and also contains a public
		/// method named ConfigureServices with <see cref="IServiceCollection"/> parameter.</typeparam>
		/// <param name="hostBuilder">The <see cref="IHostBuilder"/> to initialize with TStartup.</param>
		/// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
		public static IHostBuilder UseStartup<TStartup>(
			this IHostBuilder hostBuilder) where TStartup : class =>
			UseStartup<TStartup>(hostBuilder, Array.Empty<string>());

		/// <summary>
		/// Specify the startup type to be used by the host.
		/// </summary>
		/// <typeparam name="TStartup">The type containing a constructor
		/// with <see cref="IConfiguration"/> parameter and also contains a public
		/// method named ConfigureServices with <see cref="IServiceCollection"/> parameter.</typeparam>
		/// <param name="hostBuilder">The <see cref="IHostBuilder"/> to initialize with TStartup.</param>
		/// <param name="args">The command line args.</param>
		/// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
		public static IHostBuilder UseStartup<TStartup>(
			this IHostBuilder hostBuilder, string[] args) where TStartup : class
		{
			// Build a typical IConfigurationRoot
			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.AddCommandLine(args ?? Array.Empty<string>())
				.Build();

			// Inject our configuration into the host builder, also adding
			// environment bound appsettings.json
			hostBuilder.ConfigureAppConfiguration((ctx, cfg) =>
			{
				cfg.AddConfiguration(configuration);
				cfg.AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json",
					optional: true, reloadOnChange: true);
			});

			// A sanity check that the target TStartup type has a constructor
			// of TStartup(IConfiguration)
			if(typeof(TStartup).GetConstructor(new Type[] { typeof(IConfiguration) }) == null)
			{
				throw new InvalidOperationException(
					$"{nameof(TStartup)} must implement a public constructor " +
					$"with a parameter of type {nameof(IConfiguration)}");
			}

			// Find a method that has this signature: ConfigureServices(IServiceCollection)
			var cfgServicesMethod = typeof(TStartup).GetMethod("ConfigureServices",
				new Type[] { typeof(IServiceCollection) });

			// A sanity check that the target TStartup type has a method with
			// signature ConfigureServices(IServiceCollection)
			if (cfgServicesMethod == null)
			{
				throw new InvalidOperationException(
					$"{nameof(TStartup)} must implement a public method " +
					$"\"ConfigureServices\"with a parameter of type {nameof(IServiceCollection)}");
			}

			// Create a new instance of TStartup
			var startup = (TStartup)Activator.CreateInstance(typeof(TStartup), configuration);

			// Send in our service collection to the ConfigureServices method
			hostBuilder.ConfigureServices(serviceCollection =>
				cfgServicesMethod.Invoke(startup, new object[] { serviceCollection }));

			// chain the response
			return hostBuilder;
		}
	}
}
