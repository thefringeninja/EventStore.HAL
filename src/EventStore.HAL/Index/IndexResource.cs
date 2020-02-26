using System;
using System.Collections.Generic;
using System.Reflection;
using EventStore.Client;

namespace EventStore.HAL.Index {
	internal class IndexResource {
		public string Provider { get; }
		public IDictionary<string, object> Versions { get; }

		public IndexResource() {
			Provider = "EventStore";
			Versions = new Dictionary<string, object> {
				["EventStoreClient"] = GetVersion(typeof(EventStoreClient).Assembly)
			};
		}

		private static string GetVersion(Type type) => GetVersion(type.Assembly);

		private static string GetVersion(Assembly assembly)
			=> assembly
				   ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
				   ?.InformationalVersion
			   ?? assembly
				   ?.GetCustomAttribute<AssemblyVersionAttribute>()
				   ?.Version
			   ?? "unknown";
	}
}
