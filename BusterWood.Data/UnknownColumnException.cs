using System;
using System.Runtime.Serialization;

namespace BusterWood.Data
{
    [Serializable]
    public class UnknownColumnException : Exception
    {
        public UnknownColumnException()
        {
        }

        public UnknownColumnException(string message) : base(message)
        {
        }

        public UnknownColumnException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnknownColumnException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}