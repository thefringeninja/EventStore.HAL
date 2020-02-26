using System.Collections.Generic;
using EventStore.Client;

namespace EventStore.HAL.AllStream {
	internal class AllStreamResource {
		public Position FromPositionInclusive { get; }
		public bool EmbedPayload { get; }
		public Direction ReadDirection { get; }
		public ulong MaxCount { get; }
		public IReadOnlyList<ResolvedEvent> Events { get; }
		public string Self { get; }

		public AllStreamResource(Direction readDirection, Position fromPositionExclusive, ulong maxCount,
			bool embedPayload, IReadOnlyList<ResolvedEvent> events) {
			EmbedPayload = embedPayload;
			ReadDirection = readDirection;
			FromPositionInclusive = fromPositionExclusive;
			MaxCount = maxCount;
			Events = events;
			Self = LinkFormatter.ReadAll(FromPositionInclusive, MaxCount, EmbedPayload, ReadDirection);
		}
	}
}
