using Phaeton.DependencyInjection.Generator.Sample.Abstractions.Services;

namespace Phaeton.DependencyInjection.Generator.Sample.Services;

[GenerateInterfaceAndRegisterIt(Lifetime.Singleton)]
public sealed partial class BarService : IBarService
{
    public void DoSomething() { }
}