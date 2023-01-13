using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using Phaeton.Shared.Protos;

namespace Phaeton.gRPC.Server.Sample.gRPC;

[gRPCService]
public sealed class PingPongService : Shared.Protos.PingPongService.PingPongServiceBase
{
    public override Task<PingResponse> Ping(
        Empty req,
        ServerCallContext ctx
    )
        => Task.FromResult(new PingResponse { Value = "Pong" });
}