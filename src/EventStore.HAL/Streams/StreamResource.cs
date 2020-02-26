using System;
using System.Collections.Generic;
using EventStore.Client;

namespace EventStore.HAL.Streams {
	internal class StreamResource {
		public StreamRevision FromStreamRevisionInclusive { get; }
		public string StreamId { get; }
		public bool EmbedPayload { get; }
		public Direction ReadDirection { get; }
		public ulong MaxCount { get; }
		public IReadOnlyList<ResolvedEvent> Events { get; }
		public string Self { get; }

		public StreamResource(string streamId, Direction readDirection, StreamRevision fromStreamRevisionInclusive,
			ulong maxCount, bool embedPayload, IReadOnlyList<ResolvedEvent>? events = null) {
			StreamId = streamId;
			EmbedPayload = embedPayload;
			ReadDirection = readDirection;
			FromStreamRevisionInclusive = fromStreamRevisionInclusive;
			MaxCount = maxCount;
			Events = events ?? Array.Empty<ResolvedEvent>();
			Self = LinkFormatter.ReadStream(StreamId, FromStreamRevisionInclusive, MaxCount, EmbedPayload,
				ReadDirection);
		}

		public sealed class NotFound : StreamResource {
			public NotFound(string streamId, Direction readDirection, StreamRevision fromStreamRevisionInclusive,
				ulong maxCount, bool embedPayload) : base(streamId, readDirection, fromStreamRevisionInclusive,
				maxCount, embedPayload, Array.Empty<ResolvedEvent>()) {
			}
		}

		public sealed class Tombstoned : StreamResource {
			public Tombstoned(string streamId, Direction readDirection, StreamRevision fromStreamRevisionInclusive,
				ulong maxCount, bool embedPayload) : base(streamId, readDirection, fromStreamRevisionInclusive,
				maxCount, embedPayload, Array.Empty<ResolvedEvent>()) {
			}
		}
	}
}
