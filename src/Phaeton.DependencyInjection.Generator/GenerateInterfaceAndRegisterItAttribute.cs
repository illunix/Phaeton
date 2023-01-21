namespace Phaeton.DependencyInjection;

[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateInterfaceAndRegisterItAttribute : Attribute 
{
    public GenerateInterfaceAndRegisterItAttribute(
        Lifetime lifeTime,
        params Type[] baseTypes
    ) { }
}