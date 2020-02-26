using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using EventStore.Client;

namespace EventStore.HAL {
	internal class StreamRevisionConverter : JsonConverter<StreamRevision> {
		public override StreamRevision Read(ref Utf8JsonReader reader, Type typeToConvert,
			JsonSerializerOptions options) => throw new NotSupportedException();

		public override void Write(Utf8JsonWriter writer, StreamRevision value, JsonSerializerOptions options) =>
			writer.WriteNumberValue(value);
	}
}
