using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using EventStore.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace EventStore.HAL.StreamMetadata {
	internal static class StreamMetadataMiddleware {
		public static IApplicationBuilder UseStreamMetadata(this IApplicationBuilder builder,
			EventStoreClient eventStore) {
			var options = new JsonSerializerOptions {
				Converters = {
					new TimeSpanConverter(),
					new NullableTimeSpanConverter(),
					new JsonDocumentConverter()
				},
				IgnoreNullValues = true,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};

			return builder.MapMethods(
				"/streams/{streamId}/metadata",
				(HttpMethod.Get, GetMetadata),
				(HttpMethod.Post, PostMetadata));

			async ValueTask<Response> GetMetadata(HttpContext context) {
				var streamId = context.Request.GetStreamId()!;

				var metadataResult = await eventStore.GetStreamMetadataAsync(streamId,
					userCredentials: context.GetUserCredentials(),
					cancellationToken: context.RequestAborted);

				var resource = new StreamMetadataResource(streamId, metadataResult.Metadata);
				return new HALResponse(StreamMetadataRepresentation.Instance, resource) {
					StatusCode = metadataResult == StreamMetadataResult.None(streamId)
						? HttpStatusCode.NotFound
						: HttpStatusCode.OK
				};
			}

			async ValueTask<Response> PostMetadata(HttpContext context) {
				var streamId = context.Request.GetStreamId();

				var dto = await JsonSerializer.DeserializeAsync<StreamMetadataDto>(context.Request.Body, options);

				var metadata = new Client.StreamMetadata(
					dto.MaxCount, dto.MaxAge, dto.TruncateBefore.HasValue
						? new StreamPosition(dto.TruncateBefore.Value)
						: new StreamPosition?(), dto.CacheControl,
					new StreamAcl(dto.Acl?.ReadRoles, dto.Acl?.WriteRoles, dto.Acl?.DeleteRoles, dto.Acl?.MetaReadRoles,
						dto.Acl?.MetaWriteRoles), dto.CustomMetadata);

				var result = await eventStore.SetStreamMetadataAsync(streamId!, StreamState.Any,
					metadata,
					userCredentials: context.GetUserCredentials(),
					cancellationToken: context.RequestAborted);

				return new HALResponse(StreamMetadataResultRepresentation.Instance,
					(streamId, result));
			}
		}

		private class StreamMetadataDto {
			public int? MaxCount { get; set; }
			public TimeSpan? MaxAge { get; set; }
			public TimeSpan? CacheControl { get; set; }
			public uint? TruncateBefore { get; set; }
			public JsonDocument? CustomMetadata { get; set; }
			public StreamAclDto? Acl { get; set; }

			public class StreamAclDto {
				public string[]? ReadRoles { get; }
				public string[]? WriteRoles { get; }
				public string[]? DeleteRoles { get; }
				public string[]? MetaReadRoles { get; }
				public string[]? MetaWriteRoles { get; }
			}
		}
	}
}
