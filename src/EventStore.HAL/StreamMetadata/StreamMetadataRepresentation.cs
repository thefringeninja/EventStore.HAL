using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Hallo;

namespace EventStore.HAL.StreamMetadata {
	internal class StreamMetadataRepresentation : Hal<StreamMetadataResource>, IHalLinks<StreamMetadataResource>,
		IHalEmbedded<StreamMetadataResource>, IHalState<StreamMetadataResource>, IDisposable {
		public static readonly StreamMetadataRepresentation Instance = new StreamMetadataRepresentation();
		private readonly JsonDocument _setStreamMetadata;

		private StreamMetadataRepresentation() {
			_setStreamMetadata = JsonHyperSchema.Get<StreamMetadataResource>("metadata");
		}

		private static Link Rebase(Link link) => link.Rebase("../..");

		private static IEnumerable<Link> LinksForInternal(StreamMetadataResource resource) {
			yield return new Link(Constants.Relations.Index, "./");
			yield return new Link(Constants.Relations.Find, LinkFormatter.FindStreamTemplate());
			yield return new Link(Constants.Relations.Browse, LinkFormatter.BrowseStreamsTemplate());

			var self = LinkFormatter.StreamMetadata(resource.StreamId);
			yield return new Link(Constants.Relations.Metadata, self);
			yield return new Link(Constants.Relations.Self, self);
			yield return new Link(Constants.Relations.Feed, LinkFormatter.Stream(resource.StreamId));
		}

		public IEnumerable<Link> LinksFor(StreamMetadataResource resource) =>
			LinksForInternal(resource).Select(Rebase);

		public object EmbeddedFor(StreamMetadataResource resource)
			=> new Dictionary<string, object> {
				[Constants.Relations.Metadata] = _setStreamMetadata.RootElement
			};

		public object StateFor(StreamMetadataResource resource) => new {
			resource.StreamId,
			resource.Metadata.MaxAge,
			resource.Metadata.MaxCount,
			resource.Metadata.TruncateBefore,
			resource.Metadata.CacheControl,
			resource.Metadata.Acl,
			MetadataJson = resource.Metadata.CustomMetadata?.RootElement
		};

		public void Dispose() => _setStreamMetadata.Dispose();
	}
}
