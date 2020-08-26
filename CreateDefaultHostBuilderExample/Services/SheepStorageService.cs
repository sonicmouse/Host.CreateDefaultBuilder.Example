using CreateDefaultHostBuilderExample.Models.AppSettingsModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CreateDefaultHostBuilderExample.Services
{
	public interface ISheepStorageService
	{
		Task StoreSheepAsync(string name);
	}

	public sealed class SheepStorageService : ISheepStorageService
	{
		private readonly IMemoryCache _memoryCache;
		private readonly ILogger<SheepStorageService> _logger;
		private readonly CountSheepSettingsModel _countSheepSettings;

		public SheepStorageService(
			IMemoryCache memoryCache,
			ILogger<SheepStorageService> logger,
			IOptions<CountSheepSettingsModel> options)
		{
			_memoryCache = memoryCache;
			_logger = logger;
			_countSheepSettings = options.Value;
		}

		public Task StoreSheepAsync(string name)
		{
			_logger.LogInformation($"Adding sheep to storage with name: {name}");

			var expToken = new CancellationChangeToken(
				new CancellationTokenSource(
					TimeSpan.FromMilliseconds(_countSheepSettings.HowLongUntilEviction)).Token);

			_memoryCache.Set($"$key_{name}", name, new MemoryCacheEntryOptions()
				.AddExpirationToken(expToken)
				.RegisterPostEvictionCallback((key, val, reason, state) =>
			{
				_logger.LogWarning($"Evicting sheep \"{val}\" because of \"{reason}\"");
			}));

			return Task.CompletedTask;
		}
	}
}
