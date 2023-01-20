using Phaeton.DependencyInjection;
using SocialMediaTradex.Services.Users.Core.Abstractions.Services;

namespace SocialMediaTradex.Services.Users.Core.Services;

[GenerateInterfaceAndRegisterIt(Lifetime.Singleton)]
public sealed partial class FooService : IFooService
{
    private readonly IBarService _barService;
    private readonly string elo = "";

    public void Bar()
        => _barService.DoSomething();
}