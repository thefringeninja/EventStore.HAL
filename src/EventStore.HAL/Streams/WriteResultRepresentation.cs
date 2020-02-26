using System.Collections.Generic;
using System.Linq;
using EventStore.Client;
using Hallo;

namespace EventStore.HAL.Streams {
	internal class WriteResultRepresentation : Hal<(string, WriteResult)>, IHalLinks<(string streamId, WriteResult)>,
		IHalState<(string, WriteResult writeResult)> {
		public static readonly WriteResultRepresentation Instance = new WriteResultRepresentation();

		private WriteResultRepresentation() {
		}

		public IEnumerable<Link> LinksFor((string streamId, WriteResult) resource) =>
			LinksForInternal(resource.streamId)
				.Select(Rebase);

		public object StateFor((string, WriteResult writeResult) resource) =>
			new {resource.writeResult.LogPosition, resource.writeResult.NextExpectedVersion};

		private static IEnumerable<Link> LinksForInternal(string streamId) {
			var self = LinkFormatter.Stream(streamId);
			yield return new Link(Constants.Relations.Index, "./");
			yield return new Link(Constants.Relations.Find, LinkFormatter.FindStreamTemplate());
			yield return new Link(Constants.Relations.Browse, LinkFormatter.BrowseStreamsTemplate());
			yield return new Link(Constants.Relations.Feed, self);
			yield return new Link(Constants.Relations.Self, self);
		}

		private static Link Rebase(Link link) => link.Rebase("../");
	}
}
