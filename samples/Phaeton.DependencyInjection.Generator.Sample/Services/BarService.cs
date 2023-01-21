using Phaeton.DependencyInjection.Generator.Sample.Abstractions.Services;

namespace Phaeton.DependencyInjection.Generator.Sample.Services;

public interface IEssa
{

}

[GenerateInterfaceAndRegisterIt(
    Lifetime.Singleton,
    typeof(IEssa)
)]
public sealed partial class BarService : IBarService
{
    public void DoSomething() { }
}