using CreateDefaultHostBuilderExample.Models.AppSettingsModels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CreateDefaultHostBuilderExample.Services
{
	public sealed class CountSheepService : IHostedService
	{
		private readonly Random _r = new Random();
		private Timer _t;
		private readonly CountSheepSettingsModel _options;
		private readonly ISheepQueueService _sheepQueueService;

		public CountSheepService(
			IOptions<CountSheepSettingsModel> options,
			ISheepQueueService sheepQueueService)
		{
			_options = options.Value;
			_sheepQueueService = sheepQueueService;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_t = new Timer(_ =>
			{
				var name = string.Join(string.Empty,
					Enumerable.Range(0, _r.Next(4, 10)).Select(x =>
				{
					const string vowels = "aeiou", consonants = "bcdfghjklmnpqrstvwx";
					return (x & 1) == 1 ?
						vowels[_r.Next(vowels.Length)] :
							consonants[_r.Next(consonants.Length)];
				}));

				name = $"{name.First().ToString().ToUpper()}{name.Substring(1)}";

				_sheepQueueService.PostSheepToQueue(name);

				if (_options.HowManySheepToCount-- <= 0)
				{
					_t.Change(Timeout.Infinite, Timeout.Infinite);
				}

			}, null, 1000, _options.HowFastToCountThemSheep);
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_t?.Dispose();
			return Task.CompletedTask;
		}
	}
}
