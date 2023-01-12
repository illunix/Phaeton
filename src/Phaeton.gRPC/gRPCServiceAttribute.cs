namespace Phaeton.gRPC;

[AttributeUsage(
    AttributeTargets.Class |
    AttributeTargets.Struct,
    AllowMultiple = false
)]
public sealed class gRPCServiceAttribute : Attribute { }