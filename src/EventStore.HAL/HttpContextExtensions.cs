using EventStore.Client;
using Microsoft.AspNetCore.Http;

namespace EventStore.HAL {
	internal static class HttpContextExtensions {
		public static UserCredentials? GetUserCredentials(this HttpContext context)
			=> context.Items[nameof(UserCredentials)] as UserCredentials;
	}
}
