namespace Phaeton;

public sealed class AppOptions
{
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Project { get; init; } = string.Empty;
    public int GeneratorId { get; set; }
}