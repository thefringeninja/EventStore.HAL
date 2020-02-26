using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace EventStore.HAL {
	internal class PermanentRedirectResponse : Response {
		public PermanentRedirectResponse(string location) {
			StatusCode = HttpStatusCode.MovedPermanently;
			Headers.Add(("location", location));
		}

		protected override ValueTask WriteBody(Stream stream, CancellationToken cancellationToken = default)
			=> new ValueTask(Task.CompletedTask);
	}
}
