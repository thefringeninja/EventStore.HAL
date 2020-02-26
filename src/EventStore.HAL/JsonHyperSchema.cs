using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace EventStore.HAL {
	internal static class JsonHyperSchema {
		public static JsonDocument Get<TResource>(string name) {
			using var stream = GetStream<TResource>(name, "json")
			                   ?? throw new Exception($"Embedded schema resource, {name}, not found. BUG!");
			return JsonDocument.Parse(stream);
		}

		private static Stream? GetStream<TResource>(string name, string extension)
			=> typeof(TResource)
				.GetTypeInfo().Assembly
				.GetManifestResourceStream(typeof(TResource), $"Schema.{name}.schema.{extension}");
	}
}
