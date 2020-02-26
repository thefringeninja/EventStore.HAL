using System;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Text.Json.Serialization;
using EventStore.Client;
using Microsoft.Net.Http.Headers;

namespace EventStore.HAL {
	internal class ResolvedEventConverter : JsonConverter<ResolvedEvent> {
		public override ResolvedEvent Read(ref Utf8JsonReader reader, Type typeToConvert,
			JsonSerializerOptions options) =>
			throw new NotSupportedException();

		public override void Write(Utf8JsonWriter writer, ResolvedEvent value, JsonSerializerOptions options) {
			var @event = value.OriginalEvent;

			if (@event == null) {
				writer.WriteStartObject();
				writer.WriteEndObject();
				return;
			}

			writer.WriteStartObject();

			writer.WritePropertyName("messageId");
			options.GetConverter<Uuid>().Write(writer, @event.EventId, options);

			writer.WriteString("createdUtc", @event.Created);

			writer.WritePropertyName("position");
			options.GetConverter<Position>().Write(writer, @event.Position, options);

			writer.WriteString("streamId", @event.EventStreamId);

			writer.WritePropertyName("streamVersion");
			options.GetConverter<StreamRevision>().Write(writer, @event.EventNumber, options);

			writer.WriteString("type", @event.EventType);

			writer.WritePropertyName("payload");
			if (MediaTypeHeaderValue.TryParse(@event.ContentType, out var contentType)
			    && contentType.SubType.EndsWith("json", StringComparison.Ordinal)) {
				var reader = new Utf8JsonReader(@event.Data.AsSpan());
				if (JsonDocument.TryParseValue(ref reader, out var payload)) {
					payload.WriteTo(writer);
				} else {
					writer.WriteBase64StringValue(@event.Data);
				}
			} else {
				writer.WriteBase64StringValue(@event.Data);
			}

			writer.WriteEndObject();
		}
	}

	internal static class JsonSerializerOptionsExtensions {
		public static JsonConverter<T> GetConverter<T>(this JsonSerializerOptions options)
			=> (JsonConverter<T>) options.GetConverter(typeof(T));
	}
}
