using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentCassandra.CircuitBreaker
{
    /// <summary>
    /// Exception thrown when an attempted operation has failed.
    /// </summary>
    public class OperationFailedException : ApplicationException
    {
        #region Constructors
        /// <summary>
        /// Creates a new default instance of the class
        /// </summary>
        public OperationFailedException() 
            : base()
        {
        }

        /// <summary>
        /// Creates a new instance of the class with the given message.
        /// </summary>
        /// <param name="message">Message to be included in the exception</param>
        public OperationFailedException(string message) 
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the class with the given message and innerException.
        /// </summary>
        /// <param name="message">Message to be included in the exception</param>
        /// <param name="innerException">The exception this instances is wrapping.</param>
        public OperationFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        #endregion
    }
}
