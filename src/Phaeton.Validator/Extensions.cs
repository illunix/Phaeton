using System.Linq.Expressions;

namespace Phaeton.Validator;

public static class AbstractValidator
{
    public static void RuleFor<T, K>(Expression<Func<T, K>> expression)
    {

    }
}