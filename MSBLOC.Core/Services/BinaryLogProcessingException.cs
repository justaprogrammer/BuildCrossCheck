using System;
using System.Runtime.Serialization;

namespace MSBLOC.Core.Services
{
    [Serializable]
    public class BinaryLogProcessingException : Exception
    {
        public BinaryLogProcessingException()
        {
        }

        public BinaryLogProcessingException(string message) : base(message)
        {
        }

        public BinaryLogProcessingException(string message, Exception inner) : base(message, inner)
        {
        }

        protected BinaryLogProcessingException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}