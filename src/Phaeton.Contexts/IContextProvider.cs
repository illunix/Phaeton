namespace Phaeton.Contexts;

public interface IContextProvider
{
    IContext Current();
}