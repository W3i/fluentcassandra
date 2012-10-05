using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentCassandra.CircuitBreaker
{
    /// <summary>
    /// Exception thrown when an operation is being called on an open circuit.
    /// </summary>
    public class OpenCircuitException : ApplicationException
    {
        #region Constructors
        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        public OpenCircuitException() 
            : base()
        {
        }

        /// <summary>
        /// Creates a new instance of the class with the given exception message.
        /// </summary>
        /// <param name="message">Message to be included in the exception</param>
        public OpenCircuitException(string message) 
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the class with the given message and innerException.
        /// </summary>
        /// <param name="message">Message to be included in the exception</param>
        /// <param name="innerException">The exception this instance is wrapping.</param>
        public OpenCircuitException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        #endregion
    }
}


