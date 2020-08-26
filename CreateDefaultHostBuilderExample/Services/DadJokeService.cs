using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CreateDefaultHostBuilderExample.Services
{
	public interface IDadJokeService
	{
		Task<string> GetDadJokeAsync();
	}

	public sealed class DadJokeService : IDadJokeService
	{
		private readonly IHttpClientFactory _httpClientFactory;

		public DadJokeService(IHttpClientFactory httpClientFactory)
		{
			_httpClientFactory = httpClientFactory;
		}

		private sealed class DadzJoke
		{
			[JsonProperty("joke")]
			public string Joke { get; set; }
		}

		public async Task<string> GetDadJokeAsync()
		{
			var client = _httpClientFactory.CreateClient();

			using(var req = new HttpRequestMessage(HttpMethod.Get, new Uri("https://icanhazdadjoke.com/")))
			{
				req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				using(var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead))
				{
					resp.EnsureSuccessStatusCode();

					using(var s = await resp.Content.ReadAsStreamAsync())
					using(var sr = new StreamReader(s))
					using (var jtr = new JsonTextReader(sr))
					{
						return new JsonSerializer().Deserialize<DadzJoke>(jtr).Joke;
					}
				}
			}
		}
	}
}
