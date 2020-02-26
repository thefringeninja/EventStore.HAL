using EventStore.Client;

namespace EventStore.HAL.StreamMessages {
	internal class StreamMessageResource {
		public string StreamId { get; }
		public StreamRevision StreamRevision { get; }
		public ResolvedEvent Event { get; }

		public StreamMessageResource(string streamId, StreamRevision streamRevision,
			ResolvedEvent resolvedEvent = default) {
			StreamId = streamId;
			StreamRevision = streamRevision;
			Event = resolvedEvent;
		}
	}
}
