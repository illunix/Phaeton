using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Phaeton.Framework;
using Phaeton.Shared.Protos;

Thread.Sleep(10000);

var builder = WebApplication
    .CreateBuilder(args)
    .AddPhaetonFramework();

var app = builder.Build();

using var channel = GrpcChannel.ForAddress("https://localhost:7044");
var client = new PingPongService.PingPongServiceClient(channel);

using var call = client.PingAsync(new Empty());
try
{
    var res = await call.ResponseAsync;
    
    Console.WriteLine(res.Value);
}
catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
{
    Console.WriteLine("Stream cancelled.");
}

app.UsePhaetonFramework();

app.Run();