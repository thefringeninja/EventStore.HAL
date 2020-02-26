using System.Collections.Generic;
using System.Linq;
using EventStore.Client;
using Hallo;

namespace EventStore.HAL.StreamMetadata {
	internal class StreamMetadataResultRepresentation : Hal<(string, WriteResult)>,
		IHalLinks<(string streamId, WriteResult)>,
		IHalState<(string, WriteResult writeResult)> {
		public static readonly StreamMetadataResultRepresentation Instance = new StreamMetadataResultRepresentation();

		private StreamMetadataResultRepresentation() {
		}

		private static Link Rebase(Link link) => link.Rebase("../..");

		private static IEnumerable<Link> LinksForInternal((string streamId, WriteResult) resource) {
			yield return new Link(Constants.Relations.Index, "./");
			yield return new Link(Constants.Relations.Find, LinkFormatter.FindStreamTemplate());
			yield return new Link(Constants.Relations.Browse, LinkFormatter.BrowseStreamsTemplate());

			var self = LinkFormatter.StreamMetadata(resource.streamId);
			yield return new Link(Constants.Relations.Metadata, self);
			yield return new Link(Constants.Relations.Self, self);
			yield return new Link(Constants.Relations.Feed, LinkFormatter.Stream(resource.streamId));
		}

		public IEnumerable<Link> LinksFor((string streamId, WriteResult) resource) =>
			LinksForInternal(resource).Select(Rebase);

		public object StateFor((string, WriteResult writeResult) resource) =>
			new {resource.writeResult.LogPosition, resource.writeResult.NextExpectedVersion};
	}
}
