using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CreateDefaultHostBuilderExample.Services
{
	public interface ISheepQueueService
	{
		public void PostSheepToQueue(string name);
	}

	public sealed class SheepQueueService : BackgroundService, ISheepQueueService
	{
		private static readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
		private static readonly ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();

		private readonly ISheepStorageService _sheepStorageService;
		private readonly IDadJokeService _dadJokeService;
		private readonly ILogger<SheepQueueService> _logger;

		public SheepQueueService(
			ISheepStorageService sheepStorage,
			IDadJokeService dadJokeService,
			ILogger<SheepQueueService> logger)
		{
			_sheepStorageService = sheepStorage;
			_dadJokeService = dadJokeService;
			_logger = logger;
		}

		public void PostSheepToQueue(string name)
		{
			_queue.Enqueue(name);
			_signal.Release();
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				try
				{
					await _signal.WaitAsync(stoppingToken);
					if (_queue.TryDequeue(out var name))
					{
						if(name.StartsWith("D"))
						{
							throw new InvalidDataException();
						}
						await _sheepStorageService.StoreSheepAsync(name);
					}
				}
				catch (InvalidDataException)
				{
					var dad = await _dadJokeService.GetDadJokeAsync();
					_logger.LogError(dad);
				}
				catch (OperationCanceledException)
				{
				}
			}
		}
	}
}
