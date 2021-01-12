using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using EventStore.Client;
using Hallo;

namespace EventStore.HAL.Streams {
	internal class StreamRepresentation : Hal<StreamResource>, IHalLinks<StreamResource>, IHalState<StreamResource>,
		IHalEmbedded<StreamResource>, IDisposable {
		public static readonly StreamRepresentation Instance = new StreamRepresentation();
		private readonly JsonDocument _appendHyperSchema;
		private readonly JsonDocument _deleteStreamHyperSchema;

		private StreamRepresentation() {
			_appendHyperSchema = JsonHyperSchema.Get<StreamResource>("append");
			_deleteStreamHyperSchema = JsonHyperSchema.Get<StreamResource>("delete-stream");
		}

		private static IEnumerable<Link> GetStreamMessageLinks(ResolvedEvent resolvedEvent) {
			var @event = resolvedEvent.OriginalEvent;
			var self = LinkFormatter.StreamMessageByStreamVersion(@event.EventStreamId,
				StreamRevision.FromStreamPosition(@event.EventNumber));
			yield return new Link(Constants.Relations.Message, self);
			yield return new Link(Constants.Relations.Self, self);
			yield return new Link(Constants.Relations.Self, LinkFormatter.Stream(@event.EventStreamId));
		}

		private static Link Rebase(Link link) => link.Rebase("../");

		public IEnumerable<Link> LinksFor(StreamResource resource) =>
			LinksForInternal(resource).Select(Rebase);

		public object StateFor(StreamResource resource) => new object();

		public object EmbeddedFor(StreamResource resource) => new Dictionary<string, object> {
			[Constants.Relations.Message] = resource
				.Events
				.Select(e => new HalRepresentation(e, GetStreamMessageLinks(e).Select(Rebase))),
			[Constants.Relations.AppendToStream] = _appendHyperSchema.RootElement,
			[Constants.Relations.DeleteStream] = _deleteStreamHyperSchema.RootElement,
		};

		private static IEnumerable<Link> LinksForInternal(StreamResource resource) {
			yield return new Link(Constants.Relations.Index, "./");
			yield return new Link(Constants.Relations.Find, LinkFormatter.FindStreamTemplate());
			yield return new Link(Constants.Relations.Browse, LinkFormatter.BrowseStreamsTemplate());
			yield return new Link(Constants.Relations.Self, resource.Self);

			var first = LinkFormatter.ReadStream(resource.StreamId,
				StreamRevision.FromStreamPosition(StreamPosition.Start), resource.MaxCount,
				resource.EmbedPayload, Direction.Forwards);

			var last = LinkFormatter.ReadStream(resource.StreamId,
				StreamRevision.FromStreamPosition(StreamPosition.End), resource.MaxCount, resource.EmbedPayload,
				Direction.Backwards);

			yield return new Link(Constants.Relations.First, first);

			if (resource.Events.Count > 0) {
				var minStreamRevision = resource.Events.Min(x => x.OriginalEvent.EventNumber);
				if (minStreamRevision != StreamRevision.FromStreamPosition(StreamPosition.Start)) {
					yield return new Link(Constants.Relations.Previous,
						LinkFormatter.ReadStream(resource.StreamId,
							StreamRevision.FromStreamPosition(minStreamRevision - 1), resource.MaxCount,
							resource.EmbedPayload, Direction.Backwards));
				}
			}

			yield return new Link(Constants.Relations.Feed, resource.Self);

			if (resource.Events.Count > 0) {
				yield return new Link(Constants.Relations.Next,
					LinkFormatter.ReadStream(resource.StreamId,
						StreamRevision.FromStreamPosition(resource.Events.Max(x => x.OriginalEvent.EventNumber).Next()),
						resource.MaxCount, resource.EmbedPayload, Direction.Forwards));
			}

			yield return new Link(Constants.Relations.Last, last);
			yield return new Link(Constants.Relations.Metadata, LinkFormatter.StreamMetadata(resource.StreamId));
		}

		public void Dispose() {
			_appendHyperSchema.Dispose();
			_deleteStreamHyperSchema.Dispose();
		}
	}
}
