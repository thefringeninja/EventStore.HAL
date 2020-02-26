using System;
using EventStore.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace EventStore.HAL {
	internal static class HttpRequestExtensions {
		public static string? GetStreamId(this HttpRequest request) =>
			(request.HttpContext.GetRouteData().Values["streamId"]?.ToString() ?? string.Empty).Unescape();

		public static StreamRevision? GetStreamRevision(this HttpRequest request) {
			var value = request.HttpContext.GetRouteData().Values["streamRevision"]?.ToString();

			return value switch {
				null => new StreamRevision?(),
				_ => ulong.TryParse(value, out var streamRevision)
					? new StreamRevision(streamRevision)
					: new StreamRevision?()
			};
		}

		public static ulong GetMaxCount(this HttpRequest request) =>
			request.Query.TryGetValueCaseInsensitive('m', out var value)
				? ulong.TryParse(value, out var count)
					? count <= 0
						? Constants.MaxCount
						: count
					: Constants.MaxCount
				: Constants.MaxCount;

		public static StreamRevision GetStreamRevision(this HttpRequest request, Direction readDirection) =>
			request.Query.TryGetValueCaseInsensitive('p', out var value)
				? value.TryParseStreamRevision(out var streamRevision)
					? readDirection == Direction.Forwards
						? streamRevision < StreamRevision.Start
							? StreamRevision.Start
							: streamRevision
						: streamRevision < StreamRevision.End
							? StreamRevision.End
							: streamRevision
					: readDirection == Direction.Forwards
						? StreamRevision.Start
						: StreamRevision.End
				: readDirection == Direction.Forwards
					? StreamRevision.Start
					: StreamRevision.End;

		public static Position GetPosition(this HttpRequest request, Direction readDirection) =>
			request.Query.TryGetValueCaseInsensitive('p', out var value)
				? value.TryParsePosition(out var position)
					? position < Position.End
						? Position.End
						: position
					: readDirection == Direction.Forwards
						? Position.Start
						: Position.End
				: readDirection == Direction.Forwards
					? Position.Start
					: Position.End;

		public static Direction GetReadDirection(this HttpRequest request) =>
			request.Query.TryGetValueCaseInsensitive('d', out var value) &&
			string.Equals("f", value, StringComparison.OrdinalIgnoreCase)
				? Direction.Forwards
				: Direction.Backwards;

		public static bool GetEmbedPayload(this HttpRequest request) =>
			request.Query.TryGetValueCaseInsensitive('e', out var value) &&
			value == "1";

		private static string Unescape(this string s)
			=> Uri.UnescapeDataString(s);

		private static bool
			TryGetValueCaseInsensitive(this IQueryCollection query, char key, out StringValues values) =>
			char.IsUpper(key)
				? query.TryGetValue(key.ToString(), out values)
				  || query.TryGetValue(char.ToLower(key).ToString(), out values)
				: query.TryGetValue(key.ToString(), out values)
				  || query.TryGetValue(char.ToUpper(key).ToString(), out values);
	}
}
