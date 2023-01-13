namespace Phaeton.gRPC;

public sealed record gRPCOptions
{
    public bool Enabled { get; init; }
    public string? NodeType { get; init; }
    public IEnumerable<string>? Addresses { get; init; }
}