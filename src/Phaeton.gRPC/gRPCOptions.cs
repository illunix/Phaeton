namespace Phaeton.gRPC;

public sealed class gRPCOptions
{
    public bool Enabled { get; init; }
    public string? NodeType { get; init; }
    public string? Url { get; init; }
}