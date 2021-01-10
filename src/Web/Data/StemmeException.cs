using System;
using System.Runtime.Serialization;

namespace Stemmesystem.Web.Data
{
    [Serializable]
    internal class StemmeException : Exception
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
}