namespace Stemmesystem.Core;


public sealed class StemmeException : Exception
{
    public StemmeException()
    {
    }

    public StemmeException(string message) : base(message)
    {
    }

    public StemmeException(string message, Exception innerException) : base(message, innerException)
    {
    }
}