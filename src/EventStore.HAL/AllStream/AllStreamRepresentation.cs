using System.Collections.Generic;
using System.Linq;
using EventStore.Client;
using Hallo;

namespace EventStore.HAL.AllStream {
	internal class AllStreamRepresentation : Hal<AllStreamResource>, IHalLinks<AllStreamResource>,
		IHalEmbedded<AllStreamResource>, IHalState<AllStreamResource> {
		public static readonly AllStreamRepresentation Instance = new AllStreamRepresentation();
		private AllStreamRepresentation() {
		}

		public IEnumerable<Link> LinksFor(AllStreamResource resource) {
			yield return new Link(Constants.Relations.Index, LinkFormatter.Index());
			yield return new Link(Constants.Relations.Self, LinkFormatter.AllStream());
			yield return new Link(Constants.Relations.Find, LinkFormatter.FindStreamTemplate());
			yield return new Link(Constants.Relations.Browse, LinkFormatter.BrowseStreamsTemplate());
			yield return new Link(Constants.Relations.Feed, LinkFormatter.AllStream());

			var first = LinkFormatter.ReadAll(Position.Start, resource.MaxCount,
				resource.EmbedPayload, Direction.Forwards);

			var last = LinkFormatter.ReadAll(Position.End, resource.MaxCount,
				resource.EmbedPayload, Direction.Backwards);

			yield return new Link(Constants.Relations.First, first);

			if (resource.Events.Count > 0) {
				yield return new Link(Constants.Relations.Previous,
					LinkFormatter.ReadAll(resource.Events.Min(x => x.OriginalEvent.Position), resource.MaxCount,
						resource.EmbedPayload, Direction.Backwards));
			}

			yield return new Link(Constants.Relations.Feed, resource.Self);

			if (resource.Events.Count > 0) {
				yield return new Link(Constants.Relations.Next,
					LinkFormatter.ReadAll(resource.Events.Max(x => x.OriginalEvent.Position), resource.MaxCount,
						resource.EmbedPayload, Direction.Forwards));
			}

			yield return new Link(Constants.Relations.Last, last);
		}

		public object StateFor(AllStreamResource resource) => new object();

		public object EmbeddedFor(AllStreamResource resource)
			=> new Dictionary<string, object> {
				["streamStore:message"] = resource
					.Events
					.Select(e => new HalRepresentation(e, GetStreamMessageLinks(e)))
			};

		private static IEnumerable<Link> GetStreamMessageLinks(ResolvedEvent e) {
			var @event = e.OriginalEvent;
			var self = LinkFormatter.StreamMessageByStreamVersion(@event.EventStreamId,
				StreamRevision.FromStreamPosition(@event.EventNumber));
			yield return new Link(Constants.Relations.Message, self);
			yield return new Link(Constants.Relations.Self, self);
			yield return new Link(Constants.Relations.Feed, LinkFormatter.Stream(@event.EventStreamId));
		}
	}
}
