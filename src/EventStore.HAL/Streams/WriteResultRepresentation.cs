using System.Collections.Generic;
using System.Linq;
using EventStore.Client;
using Hallo;

namespace EventStore.HAL.Streams {
	internal class WriteResultRepresentation : Hal<(string, IWriteResult)>, IHalLinks<(string streamId, IWriteResult)>,
		IHalState<(string, IWriteResult writeResult)> {
		public static readonly WriteResultRepresentation Instance = new WriteResultRepresentation();

		private WriteResultRepresentation() {
		}

		public IEnumerable<Link> LinksFor((string streamId, IWriteResult) resource) =>
			LinksForInternal(resource.streamId)
				.Select(Rebase);

		public object StateFor((string, IWriteResult writeResult) resource) =>
			new {resource.writeResult.LogPosition, resource.writeResult.NextExpectedStreamRevision};

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
