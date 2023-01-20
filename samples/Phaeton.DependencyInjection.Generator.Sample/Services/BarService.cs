using Phaeton.DependencyInjection;
using SocialMediaTradex.Services.Users.Core.Abstractions.Services;

namespace SocialMediaTradex.Services.Users.Core.Services;

[GenerateInterfaceAndRegisterIt(Lifetime.Singleton)]
public sealed partial class BarService : IBarService
{
    public void DoSomething() { }
}