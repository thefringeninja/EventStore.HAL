using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EventStore.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace EventStore.HAL.AllStream {
	internal static class AllStreamMiddleware {
		public static IApplicationBuilder UseAllStream(this IApplicationBuilder builder, EventStoreClient eventStore) {
			return builder.MapMethods("/stream", (HttpMethod.Get, GetStream));

			async ValueTask<Response> GetStream(HttpContext context) {
				var embedPayload = context.Request.GetEmbedPayload();
				var readDirection = context.Request.GetReadDirection();
				var fromPositionInclusive = context.Request.GetPosition(readDirection);
				var maxCount = context.Request.GetMaxCount();

				var events = await eventStore
					.ReadAllAsync(readDirection, fromPositionInclusive, maxCount,
						resolveLinkTos: false, userCredentials: context.GetUserCredentials(),
						cancellationToken: context.RequestAborted)
					.OrderByDescending(e => e.OriginalEvent.Position)
					.ToArrayAsync(context.RequestAborted);

				return new HALResponse(AllStreamRepresentation.Instance,
					new AllStreamResource(readDirection, fromPositionInclusive, maxCount, embedPayload, events));
			}
		}
	}
}
