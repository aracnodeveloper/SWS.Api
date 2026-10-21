using System.Collections;
using System.Reflection;
using App.Shared.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;

namespace App.Shared.Infrastructure;

/// <summary>
/// Envuelve respuestas JSON en <c>{ code, message, data }</c> (éxito y error; en errores <c>data</c> es null).
/// Las acciones que devolvían 204 pasan a HTTP 200 con cuerpo JSON para poder incluir el sobre sin violar RFC.
/// </summary>
public sealed class StandardApiResponseEnvelopeFilter : IAlwaysRunResultFilter
{
    public void OnResultExecuted(ResultExecutedContext context) { }

    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (ShouldSkip(context))
            return;

        var result = context.Result;
        if (result is null)
            return;

        switch (result)
        {
            case FileResult:
            case RedirectResult:
                return;
        }

        if (
            result is RedirectToActionResult
            || result is LocalRedirectResult
            || result is RedirectToRouteResult
        )
            return;

        if (result is UnauthorizedResult)
        {
            context.Result = NewError(401, "Unauthorized.");
            return;
        }

        if (result is ForbidResult)
        {
            context.Result = NewError(403, "Forbidden.");
            return;
        }

        if (result is BadRequestResult)
        {
            context.Result = NewError(400, HttpReason(400));
            return;
        }

        if (result is NotFoundResult)
        {
            context.Result = NewError(404, HttpReason(404));
            return;
        }

        if (result is OkResult)
        {
            var msg = BuildSuccessMessage(context, 200);
            context.Result = NewSuccess(200, msg, null);
            return;
        }

        // 204 sin cuerpo no permite JSON; respondemos 200 con el mismo sobre para mantener formato único.
        if (result is NoContentResult)
        {
            var msg = BuildSuccessMessage(context, 204);
            context.Result = NewSuccess(200, msg, null);
            return;
        }

        if (result is CreatedAtActionResult created)
        {
            if (IsAlreadyEnvelope(created.Value))
                return;

            var status = created.StatusCode ?? 201;
            var message = BuildSuccessMessage(context, status);
            object? data = created.Value;
            if (created.Value is HttpMessage hm)
            {
                message = hm.message ?? message;
                data = null;
            }

            var envelope = new ApiSuccessResponse<object?>
            {
                Code = status,
                Message = message,
                Data = data,
            };

            context.Result = new CreatedAtActionResult(
                created.ActionName,
                created.ControllerName,
                created.RouteValues,
                envelope
            )
            {
                StatusCode = created.StatusCode,
            };
            return;
        }

        if (result is ObjectResult obj)
        {
            if (IsAlreadyEnvelope(obj.Value))
                return;

            var status = obj.StatusCode ?? 200;
            if (status >= 200 && status < 300)
            {
                if (status == 204)
                {
                    var msg204 = BuildSuccessMessage(context, 204);
                    context.Result = NewSuccess(200, msg204, obj.Value);
                    return;
                }

                var message = BuildSuccessMessage(context, status);
                object? data = obj.Value;
                if (obj.Value is HttpMessage hm)
                {
                    message = hm.message ?? message;
                    data = null;
                }

                context.Result = NewSuccess(status, message, data);
                return;
            }

            if (status >= 400)
            {
                var message = ExtractErrorMessage(obj.Value, status);
                context.Result = NewError(status, message);
                return;
            }

            return;
        }

        if (result is StatusCodeResult sc && sc.StatusCode >= 400)
        {
            context.Result = NewError(sc.StatusCode, HttpReason(sc.StatusCode));
            return;
        }
    }

    private static ObjectResult NewSuccess(int code, string message, object? data) =>
        new(new ApiSuccessResponse<object?> { Code = code, Message = message, Data = data })
        {
            StatusCode = code,
        };

    private static ObjectResult NewError(int code, string message) =>
        new(new ApiErrorResponse { Code = code, Message = message, Data = null }) { StatusCode = code };

    private static bool ShouldSkip(ResultExecutingContext context)
    {
        var path = context.HttpContext.Request.Path.Value ?? "";
        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
            return true;
        if (path.StartsWith("/images", StringComparison.OrdinalIgnoreCase))
            return true;
        if (path.StartsWith("/uploads", StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    private static bool IsAlreadyEnvelope(object? value)
    {
        if (value is null)
            return false;
        var n = value.GetType().Name;
        return n.StartsWith("ApiSuccessResponse", StringComparison.Ordinal)
            || n.StartsWith("ApiErrorResponse", StringComparison.Ordinal);
    }

    private static string BuildSuccessMessage(ResultExecutingContext context, int statusCode)
    {
        var resource =
            context.RouteData.Values.TryGetValue("controller", out var c)
            && c != null
                ? c.ToString() ?? "Resource"
                : "Resource";

        var action =
            context.RouteData.Values.TryGetValue("action", out var a) && a != null
                ? a.ToString() ?? ""
                : "";

        if (statusCode == 201 || action.Contains("Create", StringComparison.OrdinalIgnoreCase))
            return $"{resource} created successfully.";

        if (action.Contains("Delete", StringComparison.OrdinalIgnoreCase))
            return $"{resource} deleted successfully.";

        if (
            action.Contains("Update", StringComparison.OrdinalIgnoreCase)
            || action.Contains("Patch", StringComparison.OrdinalIgnoreCase)
            || action.Contains("Put", StringComparison.OrdinalIgnoreCase)
        )
            return $"{resource} updated successfully.";

        if (statusCode == 204)
            return $"{resource} deleted successfully.";

        if (string.Equals(action, "Login", StringComparison.OrdinalIgnoreCase))
            return "Login successful.";

        if (statusCode == 200 || statusCode == 201)
            return $"{resource} retrieved successfully.";

        return "Request completed successfully.";
    }

    private static string ExtractErrorMessage(object? value, int statusCode)
    {
        if (value == null)
            return HttpReason(statusCode);

        switch (value)
        {
            case ValidationProblemDetails vpd:
                // Los errores por campo van antes que Title ("One or more validation errors occurred.").
                return vpd.Detail
                    ?? FlattenValidationErrors(vpd.Errors)
                    ?? vpd.Title
                    ?? HttpReason(statusCode);
            case ProblemDetails pd:
                return pd.Detail ?? pd.Title ?? HttpReason(statusCode);
            case SerializableError err:
                return FlattenSerializableError(err) ?? HttpReason(statusCode);
            case HttpMessage hm when !string.IsNullOrEmpty(hm.message):
                return hm.message;
        }

        var t = value.GetType();
        foreach (
            var propName in new[] { "message", "Message", "error", "Error", "title", "Title" }
        )
        {
            var p = t.GetProperty(
                propName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
            );
            if (p?.GetValue(value) is string s && !string.IsNullOrWhiteSpace(s))
                return s;
        }

        return value.ToString() ?? HttpReason(statusCode);
    }

    private static string? FlattenSerializableError(SerializableError err)
    {
        var parts = new List<string>();
        foreach (var kv in err)
        {
            if (kv.Value is string[] arr)
                parts.AddRange(arr);
            else if (kv.Value is IEnumerable<string> en)
                parts.AddRange(en);
        }

        return parts.Count > 0 ? string.Join(" ", parts) : null;
    }

    private static string? FlattenValidationErrors(IDictionary<string, string[]>? errors)
    {
        if (errors == null || errors.Count == 0)
            return null;
        return string.Join(" ", errors.Values.SelectMany(v => v));
    }

    private static string HttpReason(int statusCode) =>
        ReasonPhrases.GetReasonPhrase(statusCode) is { Length: > 0 } r ? r : "An error occurred.";
}
