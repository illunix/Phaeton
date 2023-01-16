using Phaeton.Abstractions;
using System.ComponentModel;

namespace Phaeton.Hangfire.Redis.EventDispatcher;

internal sealed class MediatorHangfireBridge
{
    private readonly IMediator _mediator;

    public MediatorHangfireBridge(IMediator mediator)
        => _mediator = mediator;

    public async Task Publish(IEvent @event)
        => await _mediator.Publish(@event);

    [DisplayName("{0}")]
    public async Task Publish(
        string jobName,
        IEvent @event
    )
        => await _mediator.Publish(@event);
}