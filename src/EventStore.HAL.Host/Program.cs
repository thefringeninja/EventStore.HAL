using System;
using System.Text;
using System.Threading.Tasks;
using EventStore.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Extensions.Logging;

namespace EventStore.HAL {
	internal static class Program {
		public static async Task<int> Main(string[] args) {
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo.Console()
				.CreateLogger();

			var eventStoreClient = new EventStoreClient(new EventStoreClientSettings {
				LoggerFactory = new SerilogLoggerFactory()
			});

			await new HostBuilder()
				.ConfigureHostConfiguration(builder => builder
					.AddEnvironmentVariables("DOTNET_")
					.AddCommandLine(args ?? Array.Empty<string>()))
				.ConfigureAppConfiguration(builder => builder
					.AddEnvironmentVariables()
					.AddCommandLine(args ?? Array.Empty<string>()))
				.ConfigureLogging(logging => logging.AddSerilog())
				.ConfigureWebHostDefaults(builder => builder
					.UseKestrel()
					.ConfigureServices(services => services.AddCors().AddRouting())
					.Configure(app => app
						.UseEventStoreHALBrowser()
						.UseEventStoreHAL(eventStoreClient)))
				.RunConsoleAsync();
			return 0;
		}
	}
}
