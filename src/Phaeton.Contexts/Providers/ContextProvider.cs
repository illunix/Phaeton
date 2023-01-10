using Microsoft.AspNetCore.Http;
using Phaeton.Contexts.Accessors;
using System.Diagnostics;
using Phaeton.Contexts;

namespace Phaeton.Contexts.Providers;

internal sealed class ContextProvider : IContextProvider
{
    private readonly IHttpContextAccessor _httpCtxAccessor;
    private readonly IContextAccessor _contextAccessor;

    public ContextProvider(
        IHttpContextAccessor httpCtxAccessor,
        IContextAccessor contextAccessor
    )
    {
        _httpCtxAccessor = httpCtxAccessor;
        _contextAccessor = contextAccessor;
    }

    public IContext Current()
    {
        if (_contextAccessor.Context is not null)
        {
            return _contextAccessor.Context;
        }

        IContext ctx;
        var httpCtx = _httpCtxAccessor.HttpContext;
        if (httpCtx is not null)
        {
            var traceId = httpCtx.TraceIdentifier;
            var userId = httpCtx.User.Identity?.Name;

            ctx = new Context(
                Activity.Current?.Id ?? ActivityTraceId.CreateRandom().ToString(),
                traceId,
                string.Empty,
                string.Empty,
                userId
            );
        }
        else
            ctx = new Context(Activity.Current?.Id ?? ActivityTraceId.CreateRandom().ToString());

        _contextAccessor.Context = ctx;

        return ctx;
    }
}