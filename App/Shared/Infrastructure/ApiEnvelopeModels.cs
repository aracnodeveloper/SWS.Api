using Newtonsoft.Json;

namespace App.Shared.Infrastructure;

/// <summary>
/// Formato unificado de respuesta exitosa: <c>{ "code", "message", "data" }</c>.
/// </summary>
public sealed class ApiSuccessResponse<T>
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; } = "";

    [JsonProperty("data")]
    public T? Data { get; set; }
}

/// <summary>
/// Formato unificado de error: <c>{ "code", "message", "data": null }</c> (misma forma que el éxito).
/// </summary>
public sealed class ApiErrorResponse
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; } = "";

    [JsonProperty("data", NullValueHandling = NullValueHandling.Include)]
    public object? Data { get; set; }
}
