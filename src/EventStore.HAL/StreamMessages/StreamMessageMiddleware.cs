using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EventStore.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace EventStore.HAL.StreamMessages {
	internal static class StreamMessageMiddleware {
		public static IApplicationBuilder UseStreamMessages(this IApplicationBuilder builder,
			EventStoreClient eventStore) {
			return builder.MapMethods(
				"/streams/{streamId}/{streamRevision:int}",
				(HttpMethod.Get, GetStreamMessage));

			async ValueTask<Response> GetStreamMessage(HttpContext context) {
				var streamId = context.Request.GetStreamId();
				var streamRevision = context.Request.GetStreamRevision();

				try {
					var @event = await eventStore.ReadStreamAsync(Direction.Forwards, streamId, streamRevision!.Value,
							1,
							resolveLinkTos: false, userCredentials: context.GetUserCredentials(),
							cancellationToken: context.RequestAborted)
						.Where(e => e.OriginalEvent.EventNumber == streamRevision)
						.SingleOrDefaultAsync(context.RequestAborted);

					return new HALResponse(StreamMessageRepresentation.Instance, new StreamMessageResource(
						streamId!, streamRevision!.Value, @event)) {
						StatusCode = @event.OriginalEvent == null ? HttpStatusCode.NotFound : HttpStatusCode.OK
					};
				} catch (StreamNotFoundException) {
					return new HALResponse(StreamMessageRepresentation.Instance, new StreamMessageResource(
						streamId!, streamRevision!.Value)) {
						StatusCode = HttpStatusCode.NotFound
					};
				} catch (StreamDeletedException) {
					return new HALResponse(StreamMessageRepresentation.Instance, new StreamMessageResource(
						streamId!, streamRevision!.Value)) {
						StatusCode = HttpStatusCode.Gone
					};
				}
			}
		}
	}
}
