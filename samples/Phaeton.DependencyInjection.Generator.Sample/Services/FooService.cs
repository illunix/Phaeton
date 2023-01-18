using Phaeton.DependencyInjection.Generator.Sample.Services.Abstractions;

namespace Phaeton.DependencyInjection.Generator.Sample.Services;

[GenerateInterfaceAndRegisterIt(ServiceLifetime.Singleton)]
public sealed class FooService : IFooService
{
    public void Bar() { }
    public string Elo { get; set; }
}