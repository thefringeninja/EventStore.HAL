using System.Net.Http;
using System.Threading.Tasks;
using Hallo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace EventStore.HAL.Index {
	internal static class IndexMiddleware {
		public static IApplicationBuilder UseIndex(this IApplicationBuilder builder) {
			var resource = new IndexResource();
			var response = new HALResponse(IndexRepresentation.Instance, resource);

			return builder.MapMethods("/", (HttpMethod.Get, GetIndex));

			ValueTask<Response> GetIndex(HttpContext context) => new ValueTask<Response>(response);
		}
	}
}
