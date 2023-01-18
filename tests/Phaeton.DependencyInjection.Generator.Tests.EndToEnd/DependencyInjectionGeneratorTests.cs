namespace Phaeton.DependencyInjection.Generator.Tests.EndToEnd;

public sealed class DependencyInjectionGeneratorTests
{
    [Fact]
    public void generate_dependency_injection_should_succeed_and_return_proper_source()
    {
        var input = @"
            namespace Void;

            [CreateInterfaceAndRegisterIt(LifeTimes.Singleton)]
            public sealed class CustomersRepository : ICustomersRepository
            {
                public int GetCount() { }
            }
            ";

        var expected = @"
            using Void;

            namespace Phaeton.DependencyInjection
            {
                public static class Extensions
                {
                    public static IServiceCollection RegisterServicesFromAssembly(this IServiceCollection services)
                    {
                        services.AddSingleton<ICustomersRepository, CustomersRepository>();

                        return services;
                    }
                }
            }

            namespace Void
            {
                public interface ICustomersRepository
                {
                    int GetCount();
                }
            }   
            ";
    }
}