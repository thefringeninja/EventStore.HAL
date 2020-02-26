using System.Collections.Generic;
using Hallo;
using static EventStore.HAL.Constants;

namespace EventStore.HAL.Index {
	internal class IndexRepresentation : Hal<IndexResource>, IHalLinks<IndexResource>, IHalState<IndexResource> {
		public static readonly IndexRepresentation Instance = new IndexRepresentation();

		private IndexRepresentation() {
		}

		public IEnumerable<Link> LinksFor(IndexResource resource) {
			yield return new Link(Relations.Self, LinkFormatter.Index());
			yield return new Link(Relations.Index, LinkFormatter.Index());
			yield return new Link(Relations.Find, LinkFormatter.FindStreamTemplate());
			yield return new Link(Relations.Browse, LinkFormatter.BrowseStreamsTemplate());
			yield return new Link(Relations.Feed, LinkFormatter.AllStream());
		}

		public object StateFor(IndexResource resource) => resource;
	}
}
