using Phaeton.DependencyInjection;

namespace Phaeton.AutoDependencyInjection.Generator;

internal sealed class TypesToRegisterMap<T, K> 
    where T : class 
    where K : class
{
    public T? Class { get; }
    public K? Interface { get;  }
    public Lifetime? ServiceLifetime { get; set; }
}