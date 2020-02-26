using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EventStore.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace EventStore.HAL.Streams {
	internal static class StreamsMiddleware {
		public static IApplicationBuilder UseStreams(this IApplicationBuilder builder, EventStoreClient eventStore) {
			return builder.MapMethods(
				"/streams/{streamId}",
				(HttpMethod.Get, GetStream),
				(HttpMethod.Post, PostStream),
				(HttpMethod.Delete, DeleteStream));

			async ValueTask<Response> GetStream(HttpContext context) {
				var streamId = context.Request.GetStreamId()!;
				var embedPayload = context.Request.GetEmbedPayload();
				var readDirection = context.Request.GetReadDirection();
				var fromPositionInclusive = context.Request.GetStreamRevision(readDirection);
				var maxCount = context.Request.GetMaxCount();

				try {
					var events = await eventStore
						.ReadStreamAsync(readDirection, streamId, fromPositionInclusive, maxCount,
							resolveLinkTos: false,
							userCredentials: context.GetUserCredentials(),
							cancellationToken: context.RequestAborted)
						.OrderByDescending(e => e.OriginalEvent.EventNumber)
						.ToArrayAsync(context.RequestAborted);

					return new HALResponse(StreamRepresentation.Instance, new StreamResource(streamId, readDirection,
						fromPositionInclusive, maxCount, embedPayload, events));
				} catch (StreamNotFoundException) {
					return new HALResponse(StreamRepresentation.Instance, new StreamResource(streamId, readDirection,
						fromPositionInclusive, maxCount, embedPayload)) {
						StatusCode = HttpStatusCode.NotFound
					};
				} catch (StreamDeletedException) {
					return new HALResponse(StreamRepresentation.Instance, new StreamResource(streamId, readDirection,
						fromPositionInclusive, maxCount, embedPayload)) {
						StatusCode = HttpStatusCode.Gone
					};
				}
			}

			async ValueTask<Response> PostStream(HttpContext context) {
				var streamId = context.Request.GetStreamId();

				var eventData = await GetEvents(context).ToArrayAsync(context.RequestAborted);
				var writeResult = await eventStore.AppendToStreamAsync(streamId, AnyStreamRevision.Any, eventData,
					userCredentials: context.GetUserCredentials(),
					cancellationToken: context.RequestAborted);

				return new HALResponse(WriteResultRepresentation.Instance, (streamId, writeResult));
			}

			async ValueTask<Response> DeleteStream(HttpContext context) {
				var streamId = context.Request.GetStreamId();

				await eventStore.SoftDeleteAsync(streamId, AnyStreamRevision.Any,
					userCredentials: context.GetUserCredentials(),
					cancellationToken: context.RequestAborted);

				return new NoContentResponse();
			}
		}

		private static async IAsyncEnumerable<EventData> GetEvents(HttpContext context) {
			var events = await JsonSerializer.DeserializeAsync<JsonElement>(context.Request.Body);

			switch (events.ValueKind) {
				case JsonValueKind.Array: {
					foreach (var element in events.EnumerateArray()) {
						yield return GetEventData(element);
					}

					break;
				}
				case JsonValueKind.Object:
					yield return GetEventData(events);
					break;
				default:
					throw new JsonException("Invalid json.");
			}
		}

		private static EventData GetEventData(JsonElement element) {
			try {
				return new EventData(
					Uuid.Parse(element.GetProperty("messageId").GetString()),
					element.GetProperty("type").GetString(),
					Encoding.UTF8.GetBytes(element.GetProperty("data").GetRawText()),
					element.TryGetProperty("metadata", out var metadata)
						? Encoding.UTF8.GetBytes(metadata.GetRawText())
						: Array.Empty<byte>());
			} catch (Exception ex) when (ex is InvalidOperationException || ex is KeyNotFoundException) {
				throw new JsonException("Invalid json.", ex);
			}
		}
	}
}
