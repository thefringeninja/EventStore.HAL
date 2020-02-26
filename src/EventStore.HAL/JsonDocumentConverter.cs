using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventStore.HAL {
	internal class JsonDocumentConverter : JsonConverter<JsonDocument> {
		public override JsonDocument Read(ref Utf8JsonReader reader, Type typeToConvert,
			JsonSerializerOptions options) =>
			JsonDocument.ParseValue(ref reader);

		public override void Write(Utf8JsonWriter writer, JsonDocument value, JsonSerializerOptions options) =>
			throw new NotSupportedException();
	}
}
