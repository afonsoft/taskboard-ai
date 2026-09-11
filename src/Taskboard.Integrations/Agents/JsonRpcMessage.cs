using System.Text.Json;
using System.Text.Json.Serialization;

namespace Taskboard.Integrations.Agents;

public abstract record JsonRpcMessage(
    [property: JsonPropertyName("jsonrpc")] string JsonRpc = "2.0");

public sealed record JsonRpcRequest(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("params")] JsonElement? Params)
    : JsonRpcMessage;

public sealed record JsonRpcResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("result")] JsonElement? Result,
    [property: JsonPropertyName("error")] JsonRpcError? Error)
    : JsonRpcMessage;

public sealed record JsonRpcNotification(
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("params")] JsonElement? Params)
    : JsonRpcMessage;

public sealed record JsonRpcError(
    [property: JsonPropertyName("code")] int Code,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("data")] JsonElement? Data);
