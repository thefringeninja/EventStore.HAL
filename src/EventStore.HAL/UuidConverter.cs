using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using EventStore.Client;

namespace EventStore.HAL {
	internal class UuidConverter : JsonConverter<Uuid> {
		public override Uuid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			throw new NotSupportedException();

		public override void Write(Utf8JsonWriter writer, Uuid value, JsonSerializerOptions options) =>
			writer.WriteStringValue(value.ToString());
	}
}
