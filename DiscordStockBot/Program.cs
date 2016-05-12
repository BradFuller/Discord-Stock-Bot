using System.Configuration;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;

namespace DiscordStockBot
{
	public class Program
	{
		private DiscordClient _client;
		
		private readonly Regex _stockRegex = new Regex("\\$([a-z\\.]{1,6})", RegexOptions.IgnoreCase);
		
		public void Start()
		{
			_client = new DiscordClient();

			_client.MessageReceived += async (s, e) =>
			{
				if (e.Message.IsAuthor) return;
				
				var stocks = _stockRegex.Matches(e.Message.Text);

				foreach (var stock in stocks)
				{
					var ticker = stock.ToString().Replace("$", "");
					var chartStream = await GetFileStream($"http://finviz.com/chart.ashx?t={ticker}&ty=c&ta=1&p=d&s=l");

					await e.Channel.SendFile($"{ticker}_chart.png", chartStream);
				}
			};
			
			_client.ExecuteAndWait(async () =>
			{
				await _client.Connect(ConfigurationManager.AppSettings["DiscordBotToken"]);
			});
		}

		private static async Task<Stream> GetFileStream(string url)
		{
			var client = new WebClient();
			var data = await client.DownloadDataTaskAsync(url);
			
			return new MemoryStream(data);
		}

		//https://discordapp.com/oauth2/authorize?client_id=180187453393469440&scope=bot&permissions=52224

		public static void Main(string[] args) => new Program().Start();
	}
}
