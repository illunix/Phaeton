using Phaeton.Auth.JWT;

namespace Phaeton.DependencyInjection.Generator.Sample.Services;

[GenerateInterfaceAndRegisterIt(Lifetime.Singleton)]
public sealed partial class HttpContextTokenStorage : ITokenStorage
{
    private readonly string TokenKey = "jwt";
    private readonly IHttpContextAccessor _httpCtxAccessor;

    public void Set(JsonWebToken jwt)
        => _httpCtxAccessor.HttpContext?.Items.TryAdd(
            TokenKey,
            jwt
        );

    public JsonWebToken? Get()
    {
        if (_httpCtxAccessor.HttpContext is null)
            return null;

        if (_httpCtxAccessor.HttpContext.Items.TryGetValue(
            TokenKey,
            out var jwt
        ))
            return jwt as JsonWebToken;

        return null;
    }
}
