using System;
using System.Text;
using EventStore.Client;

namespace EventStore.HAL {
	internal static class LinkFormatter {
		public static string AllStream() => Constants.Paths.AllStream;

		public static string StreamMessageByStreamVersion(string streamId, StreamRevision streamVersion)
			=> $"{Stream(streamId)}/{streamVersion}";


		public static string Docs(string rel)
			=> $"{Constants.Paths.Docs}/{rel}";

		public static string StreamMetadata(string streamId)
			=> $"{Stream(streamId)}/{Constants.Paths.Metadata}";

		public static string Index() => "./";

		public static string DocsTemplate()
			=> $"{Constants.Paths.Docs}/{{rel}}";

		public static string FindStreamTemplate()
			=> $"{Constants.Paths.Streams}/{{streamId}}";

		public static string BrowseStreamsTemplate() => $"{Constants.Paths.Streams}{{?p,t,m}}";

		public static string Stream(string streamId) => $"{Constants.Paths.Streams}/{EncodeStreamId(streamId)}";

		public static string ReadAll(Position position, long maxCount, bool embed, Direction direction) =>
			$"{Constants.Paths.AllStream}?{GetStreamQueryString(position, maxCount, embed, direction)}";

		public static string ReadStream(string streamId, StreamRevision fromVersionInclusive, long maxCount,
			bool embed, Direction direction) =>
			$"{Stream(streamId)}?{GetStreamQueryString(fromVersionInclusive, maxCount, embed, direction)}";

		private static string GetStreamQueryString(Position position, long maxCount, bool embed,
			Direction direction) =>
			GetStreamQueryString($"{position.CommitPosition}/{position.PreparePosition}", maxCount, embed,
				direction);

		private static string GetStreamQueryString(StreamRevision streamRevision, long maxCount, bool embed,
			Direction direction) =>
			GetStreamQueryString(streamRevision.ToString(), maxCount, embed, direction);

		private static string GetStreamQueryString(string streamRevisionOrPosition, long maxCount, bool embed,
			Direction direction) {
			var builder = new StringBuilder()
				.Append("d=").Append(direction == Direction.Forwards ? "f" : "b")
				.Append("&p=").Append(streamRevisionOrPosition)
				.Append("&m=").Append(maxCount);

			return (embed
					? builder.Append("&e=1")
					: builder)
				.ToString();
		}

		private static string EncodeStreamId(string streamId) => Uri.EscapeDataString(streamId.Replace("/", "%2f"));
	}
}
