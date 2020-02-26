using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Hallo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EventStore.HAL {
	internal static class EventStoreHALApplicationBuilderExtensions {
		public static IApplicationBuilder MapMethods(
			this IApplicationBuilder builder,
			string pattern,
			params (HttpMethod method, Func<HttpContext, ValueTask<Response>> getResponse)[] controller) {
			var optionsResponse = new HALResponse(OptionsRepresentation.Instance) {
				StatusCode = HttpStatusCode.OK,
				Headers = {
					("access-control-allow-headers", new[] {"Content-Type", "X-Requested-With", "Authorization"}),
					("access-control-allowed-methods", controller
						.Select(x => x.method)
						.Concat(new[] {HttpMethod.Options})
						.Distinct()
						.SelectMany(GetMethods)
						.Select(x => x.Method)
						.ToArray())
				}
			};

			return builder.UseEndpoints(endpoints => controller.Aggregate(
				endpoints.MapMethod(pattern, HttpMethod.Options, Options),
				Accumulate));

			IEndpointRouteBuilder Accumulate(IEndpointRouteBuilder endpoints,
				(HttpMethod, Func<HttpContext, ValueTask<Response>>) _) {
				var (method, getResponse) = _;
				return endpoints.MapMethod(pattern, method, getResponse);
			}

			ValueTask<Response> Options(HttpContext context) => new ValueTask<Response>(optionsResponse);
		}

		private static IEndpointRouteBuilder MapMethod(
			this IEndpointRouteBuilder builder,
			string pattern,
			HttpMethod method,
			Func<HttpContext, ValueTask<Response>> getResponse) {
			var methods = GetMethods(method);
			builder.MapMethods(pattern, methods.Select(x => x.Method), HandleRequest);

			return builder;

			async Task HandleRequest(HttpContext context) {
				var response = await getResponse(context);

				await response.Write(context.Response);
			}
		}

		private static HttpMethod[] GetMethods(HttpMethod method) =>
			method == HttpMethod.Get
				? new[] {HttpMethod.Get, HttpMethod.Head}
				: new[] {method};

		private class OptionsRepresentation : Hal<object> {
			public static readonly OptionsRepresentation Instance = new OptionsRepresentation();

			private OptionsRepresentation() {
			}
		}
	}
}
