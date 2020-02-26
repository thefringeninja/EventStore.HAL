using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hallo;

namespace EventStore.HAL {
	internal class HALResponse : Response {
		private static readonly object EmptyBody = new object();
		private readonly object _resource;
		private readonly IHal _hal;

		private static readonly JsonSerializerOptions SerializerOptions
			= new JsonSerializerOptions {
				Converters = {
					new LinksConverter(),
					new HalRepresentationConverter(),
					new UuidConverter(),
					new StreamRevisionConverter(),
					new PositionConverter(),
					new ResolvedEventConverter(),
					new TimeSpanConverter(),
					new NullableTimeSpanConverter()
				},
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};

		public HALResponse(IHal hal, object? resource = null) {
			_resource = resource ?? EmptyBody;
			_hal = hal;
			Headers.Add(("content-type", Constants.MediaTypes.HalJson));
		}

		protected override async ValueTask WriteBody(Stream stream, CancellationToken cancellationToken = default) {
			var representation = await _hal.RepresentationOfAsync(_resource);
			await JsonSerializer.SerializeAsync(stream, representation, SerializerOptions, cancellationToken);
		}
	}
}
