namespace Tesseract.Abstractions;

public abstract class ExceptionBase : Exception
{
    protected ExceptionBase(string msg) : base(msg) { }
}