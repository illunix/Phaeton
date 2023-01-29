using Phaeton.DependencyInjection.Generator.Sample.Services.Abstractions;
using Phaeton.Mediator;

namespace Phaeton.DependencyInjection.Generator.Sample.Commands;

[GenerateMediator]
public sealed partial class SignUp
{
    public partial record Command(
        string Email,
        string Password,
        string ConfirmPassword
    )
    {
        public static void Validate(Command req)
        {
            if (string.IsNullOrEmpty(req.Email))
                throw new ArgumentException(req.Email);
        }
    }

    public static async Task Handler(
        Command req,
        ITokenStorage tokenStorage,
        IHttpContextAccessor http
    )
    {

    }
}