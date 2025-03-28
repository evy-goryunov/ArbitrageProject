namespace Application.Exceptions;

public class FuturesDataNotFoundException : Exception
{
    public FuturesDataNotFoundException(string message) : base(message)
    {
    }
}