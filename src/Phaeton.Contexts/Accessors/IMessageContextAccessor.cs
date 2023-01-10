using Phaeton.Contexts;

namespace Phaeton.Contexts.Accessors;

public interface IMessageContextAccessor
{
    MessageContext? MessageContext { get; set; }
}
