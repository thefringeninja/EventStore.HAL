namespace EventStore.HAL {
	internal static class Constants {
		public const int MaxCount = 20;

		public static class Relations {
			public const string EventStorePrefix = "streamStore";
			public const string Curies = "curies";
			public const string Self = "self";
			public const string First = "first";
			public const string Previous = "previous";
			public const string Next = "next";
			public const string Last = "last";
			public const string Index = EventStorePrefix + ":index";
			public const string Feed = EventStorePrefix + ":feed";
			public const string Message = EventStorePrefix + ":message";
			public const string Metadata = EventStorePrefix + ":metadata";
			public const string AppendToStream = EventStorePrefix + ":append";
			public const string DeleteStream = EventStorePrefix + ":delete-stream";
			public const string Find = EventStorePrefix + ":find";
			public const string Browse = EventStorePrefix + ":feed-browser";
		}

		public static class MediaTypes {
			public const string TextMarkdown = "text/markdown";
			public const string HalJson = "application/hal+json";
			public const string JsonHyperSchema = "application/schema+json";
			public const string Any = "*/*";
		}


		public static class Paths {
			public const string Streams = "streams";
			public const string AllStream = "stream";
			public const string Metadata = "metadata";
			public const string Docs = "docs";
		}
	}
}
