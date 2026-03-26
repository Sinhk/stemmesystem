namespace Stemmesystem.Shared
{
    [Serializable]
    public class VotingException : Exception
    {
        public VotingException()
        {
        }

        public VotingException(string message) : base(message)
        {
        }

        public VotingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}