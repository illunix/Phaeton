using Phaeton.Mediator;

namespace Phaeton.Sample.API.Features;

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

    public static async Task Handler(Command req)
    {

    }
}