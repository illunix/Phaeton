namespace Phaeton.Observability;

public sealed class SerilogOptions
{
    public string? Level { get; init; }
    public ConsoleOptions? Console { get; init; }
    public SeqOptions? Seq { get; init; }

    public sealed class ConsoleOptions
    {
        public bool Enabled { get; init; }
    }

    public sealed class SeqOptions
    {
        public bool Enabled { get; init; }
        public string? Url { get; init; } 
        public string? ApiKey { get; init; }
    }
}