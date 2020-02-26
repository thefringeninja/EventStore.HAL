using System;
using System.Net;
using System.Text.Json;
using EventStore.Client;
using Hallo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using MidFunc = System.Func<
	Microsoft.AspNetCore.Http.HttpContext,
	System.Func<System.Threading.Tasks.Task>,
	System.Threading.Tasks.Task
>;

namespace EventStore.HAL {
	internal static class ExceptionHandlingMiddleware {
		public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
			=> builder.Use(ExceptionHandling);

		private static MidFunc ExceptionHandling => async (context, next) => {
			try {
				await next();
			} catch (Exception ex) {
				var response = ConvertException(ex);

				await response.Write(context.Response);
			}
		};

		private static Response ConvertException(Exception exception) => exception switch {
			StreamNotFoundException ex => new HALResponse(ProblemDetailsRepresentation.Instance,
				CreateProblemDetails(ex, "Stream not found.")) {
				StatusCode = HttpStatusCode.NotFound
			},
			StreamDeletedException ex => new HALResponse(ProblemDetailsRepresentation.Instance,
				CreateProblemDetails(ex, "Stream deleted.")) {
				StatusCode = HttpStatusCode.Gone
			},
			WrongExpectedVersionException ex => new HALResponse(ProblemDetailsRepresentation.Instance,
				CreateProblemDetails(ex, "Wrong expected version.")) {
				StatusCode = HttpStatusCode.Conflict
			},
			NotAuthenticatedException ex => new HALResponse(ProblemDetailsRepresentation.Instance,
				CreateProblemDetails(ex, "Not authenticated.")) {
				StatusCode = HttpStatusCode.Unauthorized
			},
			AccessDeniedException ex => new HALResponse(ProblemDetailsRepresentation.Instance,
				CreateProblemDetails(ex, "Access denied.")) {
				StatusCode = HttpStatusCode.Forbidden
			},
			RequiredMetadataPropertyMissingException ex => new HALResponse(ProblemDetailsRepresentation.Instance,
				CreateProblemDetails(ex, "Required metadata missing.")) {
				StatusCode = HttpStatusCode.BadRequest
			},
			JsonException ex => new HALResponse(ProblemDetailsRepresentation.Instance,
				CreateProblemDetails(ex, "Invalid json.", ex.InnerException?.Message)) {
				StatusCode = HttpStatusCode.BadRequest
			},
			_ => new HALResponse(ProblemDetailsRepresentation.Instance,
				CreateProblemDetails(exception, "Internal server error.")) {
				StatusCode = HttpStatusCode.InternalServerError
			}
		};

		private class ProblemDetails {
			public string Type { get; }
			public string Title { get; }
			public string Detail { get; }

			public ProblemDetails(string type, string title, string detail) {
				Type = type;
				Title = title;
				Detail = detail;
			}
		}

		private class ProblemDetailsRepresentation : Hal<ProblemDetails>, IHalState<ProblemDetails> {
			public static readonly ProblemDetailsRepresentation Instance = new ProblemDetailsRepresentation();

			private ProblemDetailsRepresentation() {
			}

			public object StateFor(ProblemDetails resource) => resource;
		}

		private static ProblemDetails CreateProblemDetails(Exception exception, string title, string? message = null)
			=> new ProblemDetails(exception.GetType().Name, title, message ?? exception.Message);
	}
}
