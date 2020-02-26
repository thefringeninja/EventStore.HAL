using System;
using EventStore.Client;
using Microsoft.Extensions.Primitives;

namespace EventStore.HAL {
	internal static class StringValuesExtensions {
		public static bool TryParsePosition(this StringValues value, out Position position) {
			position = default;

			var parts = value.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length != 2) {
				return false;
			}

			if (!ulong.TryParse(parts[0], out var commitPosition) ||
			    !ulong.TryParse(parts[1], out var preparePosition)) {
				return false;
			}

			position = new Position(commitPosition, preparePosition);
			return true;
		}

		public static bool TryParseStreamRevision(this StringValues value, out StreamRevision streamRevision) {
			streamRevision = default;

			if (!ulong.TryParse(value.ToString(), out var x)) {
				return false;
			}

			streamRevision = new StreamRevision(x);

			return true;
		}
	}
}
