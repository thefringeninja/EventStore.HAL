using System;
using System.Net.Http.Headers;
using System.Text;
using EventStore.Client;
using EventStore.HAL.AllStream;
using EventStore.HAL.Index;
using EventStore.HAL.StreamMessages;
using EventStore.HAL.StreamMetadata;
using EventStore.HAL.Streams;
using Microsoft.AspNetCore.Builder;
using MidFunc = System.Func<
	Microsoft.AspNetCore.Http.HttpContext,
	System.Func<System.Threading.Tasks.Task>,
	System.Threading.Tasks.Task
>;

namespace EventStore.HAL {
	public static class EventStoreHALMiddleware {
		public static IApplicationBuilder UseEventStoreHAL(this IApplicationBuilder app, EventStoreClient eventStore) =>
			app
				.UseRouting()
				.UseCors()
				.UseExceptionHandling()
				.Use(GetUserCredentials())
				.UseIndex()
				.UseAllStream(eventStore)
				.UseStreams(eventStore)
				.UseStreamMessages(eventStore)
				.UseStreamMetadata(eventStore);

		private static MidFunc GetUserCredentials() {
			var separator = new[] {':'};

			return (context, next) => {
				if (!AuthenticationHeaderValue.TryParse(context.Request.Headers["authorization"], out var authHeader)
				    || authHeader.Scheme != "Basic") {
					return next();
				}

				var parameter = authHeader.Parameter ?? string.Empty;
				var credentialBytes = new byte[parameter.Length * 3 / 4];

				if (!Convert.TryFromBase64String(parameter, credentialBytes, out var count)) {
					return next();
				}

				var credentials = Encoding.UTF8.GetString(credentialBytes, 0, count).Split(separator, 2);

				context.Items.Add(nameof(UserCredentials), new UserCredentials(credentials[0], credentials[^1]));

				return next();
			};
		}
	}
}
