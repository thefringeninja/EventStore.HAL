using System;
using Hallo;

namespace EventStore.HAL {
	internal static class LinkExtensions {
		/// <summary>
		/// https://github.com/visualeyes/halcyon/blob/master/src/Halcyon/HAL/Link.cs
		/// </summary>
		public static Link Rebase(this Link link, string baseUriPath) {
			if (string.IsNullOrWhiteSpace(baseUriPath)) {
				return link;
			}

			var uri = new Uri(link.Href, UriKind.RelativeOrAbsolute);

			if (uri.IsAbsoluteUri) {
				return link;
			}

			var baseUri = new Uri(baseUriPath, UriKind.RelativeOrAbsolute);

			return baseUri.IsAbsoluteUri
				? new Link(link.Rel, new Uri(baseUri, uri).ToString())
				: new Link(link.Rel, $"{baseUriPath.TrimEnd('/')}/{link.Href.TrimStart('/')}");
		}
	}
}
