using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace CreateDefaultHostBuilderExample.Extensions
{
	/// <summary>
	/// Extensions to emulate a typical "Startup.cs" pattern for <see cref="IHostBuilder"/>
	/// </summary>
	public static class HostBuilderExtensions
	{
		private const string ConfigureServicesMethodName = "ConfigureServices";

		/// <summary>
		/// Specify the startup type to be used by the host.
		/// </summary>
		/// <typeparam name="TStartup">The type containing an optional constructor with
		/// an <see cref="IConfiguration"/> parameter. The implementation should contain a public
		/// method named ConfigureServices with <see cref="IServiceCollection"/> parameter.</typeparam>
		/// <param name="hostBuilder">The <see cref="IHostBuilder"/> to initialize with TStartup.</param>
		/// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
		public static IHostBuilder UseStartup<TStartup>(
			this IHostBuilder hostBuilder) where TStartup : class
		{
			// Find a method that has this signature: ConfigureServices(IServiceCollection)
			var cfgServicesMethod = typeof(TStartup).GetMethod(ConfigureServicesMethodName,
				new Type[] { typeof(IServiceCollection) });

			// Check if TStartup has a ctor that takes a IConfiguration parameter
			var hasConfigCtor = typeof(TStartup).GetConstructor(
				new Type[]{ typeof(IConfiguration) }) != null;

			// I don't know how else to get the configuration built in time to pass
			// to the TStartup ctor. This seems to do it perfectly, though
			IConfiguration configuration = null;
			hostBuilder.ConfigureAppConfiguration(x => configuration = x.Build());

			// Send in the service collection to the ConfigureServices method
			hostBuilder.ConfigureServices(serviceCollection =>
			{
				// create a TStartup instance based on ctor
				var startUpObj = hasConfigCtor ?
					(TStartup)Activator.CreateInstance(typeof(TStartup), configuration) :
					(TStartup)Activator.CreateInstance(typeof(TStartup), null);

				// finally, call the ConfigureServices implemented by the TStartup object
				cfgServicesMethod?.Invoke(startUpObj, new object[] { serviceCollection });
			});

			// chain the response
			return hostBuilder;
		}
	}
}
