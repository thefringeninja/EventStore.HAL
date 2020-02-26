using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using EventStore.Client;

namespace EventStore.HAL {
	internal class PositionConverter : JsonConverter<Position> {
		public override Position Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			throw new NotSupportedException();

		public override void Write(Utf8JsonWriter writer, Position value, JsonSerializerOptions options) =>
			writer.WriteStringValue($"{value.CommitPosition}/{value.PreparePosition}");
	}
}
