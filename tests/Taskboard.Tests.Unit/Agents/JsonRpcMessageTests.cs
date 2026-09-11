using System.Text.Json;
using Shouldly;
using Xunit;
using Taskboard.Integrations.Agents;

namespace Taskboard.Tests.Unit.Agents;

public class JsonRpcMessageTests
{
    [Fact]
    public void Dado_Requisicao_Quando_SerializarEDesserializar_Entao_MantemContratoJsonRpc()
    {
        // Covers FR-001: Mensagens JSON-RPC
        var request = new JsonRpcRequest(
            "1",
            "execute",
            JsonDocument.Parse("{\"prompt\":\"hello\"}").RootElement);

        var json = JsonSerializer.Serialize(request);
        var roundTrip = JsonSerializer.Deserialize<JsonRpcRequest>(json);

        roundTrip.ShouldNotBeNull();
        roundTrip.JsonRpc.ShouldBe("2.0");
        roundTrip.Id.ShouldBe("1");
        roundTrip.Method.ShouldBe("execute");
        roundTrip.Params.ShouldNotBeNull();
        roundTrip.Params.Value.ValueKind.ShouldBe(JsonValueKind.Object);
    }
}
