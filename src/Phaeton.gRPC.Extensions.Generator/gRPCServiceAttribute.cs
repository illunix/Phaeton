namespace Phaeton.gRPC;

[AttributeUsage(
    AttributeTargets.Class |
    AttributeTargets.Struct
)]
public sealed class gRPCServiceAttribute : Attribute { }