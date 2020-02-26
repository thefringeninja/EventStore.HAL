using System.Collections.Generic;
using System.Linq;
using EventStore.Client;
using Hallo;

namespace EventStore.HAL.StreamMessages {
	internal class StreamMessageRepresentation : Hal<StreamMessageResource>, IHalState<StreamMessageResource>,
		IHalLinks<StreamMessageResource> {
		public static readonly StreamMessageRepresentation Instance = new StreamMessageRepresentation();

		private StreamMessageRepresentation() {
		}

		private static IEnumerable<Link> GetStreamMessageLinks(StreamMessageResource resource) {
			var self = LinkFormatter.StreamMessageByStreamVersion(resource.StreamId,
				resource.StreamRevision);
			yield return new Link(Constants.Relations.Message, self);
			yield return new Link(Constants.Relations.Self, self);
			yield return new Link(Constants.Relations.Self, LinkFormatter.Stream(resource.StreamId));
			if (resource.StreamRevision > StreamRevision.Start) {
				yield return new Link(Constants.Relations.Previous,
					LinkFormatter.StreamMessageByStreamVersion(resource.StreamId, resource.StreamRevision - 1));
			}

			yield return new Link(Constants.Relations.Next,
				LinkFormatter.StreamMessageByStreamVersion(resource.StreamId, resource.StreamRevision + 1));
		}

		private static Link Rebase(Link link) => link.Rebase("../..");

		public object StateFor(StreamMessageResource resource) => resource.Event;

		public IEnumerable<Link> LinksFor(StreamMessageResource resource) => GetStreamMessageLinks(resource)
			.Select(Rebase);
	}
}
