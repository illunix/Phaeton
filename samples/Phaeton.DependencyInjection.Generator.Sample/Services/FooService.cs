using Phaeton.DependencyInjection;
using Phaeton.DependencyInjection.Generator.Sample.Abstractions.Services;

namespace Phaeton.DependencyInjection.Generator.Sample.Services;

[GenerateInterfaceAndRegisterIt(Lifetime.Singleton)]
public sealed partial class FooService : IFooService
{
    private readonly IBarService _barService;
    private readonly string elo = "";

    public void Bar()
        => _barService.DoSomething();
}