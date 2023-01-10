namespace Phaeton.Observability;

public sealed class SerilogOptions
{
    public string Level { get; set; } = string.Empty;
    public ConsoleOptions Console { get; set; } = new();
    public SeqOptions Seq { get; set; } = new();

    public sealed class ConsoleOptions
    {
        public bool Enabled { get; set; }
    }

    public sealed class SeqOptions
    {
        public bool Enabled { get; set; }
        public string Url { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
}