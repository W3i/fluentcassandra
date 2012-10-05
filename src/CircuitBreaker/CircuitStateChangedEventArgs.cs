using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentCassandra.CircuitBreaker
{
    /// <summary>
    /// Represents the event data when the circuit state changes.
    /// </summary>
    public class CircuitStateChangedEventArgs : EventArgs
    {
        #region Constructors
        /// <summary>
        /// Creates a new instance of the class
        /// </summary>
        /// <param name="host">The host whos circuit has tripped.</param>
        /// <param name="messageData">any message data</param>
        /// <param name="newState">The new state of the circuit.</param>
        /// <param name="nodeId">The unique node identifier.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="nodeId"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="host"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="newState"/> is not one of the defined values.</exception>
        public CircuitStateChangedEventArgs(string nodeId, string host, CircuitBreakerState newState, string messageData)
        {
            #region Preconditions
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentNullException("host");
            }
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                throw new ArgumentNullException("nodeId");
            }
            if (!Enum.IsDefined(typeof(CircuitBreakerState), newState))
            {
                throw new ArgumentOutOfRangeException("newState", string.Format("Given state '{0}' is not one of the defined values.", newState));
            }
            #endregion

            Host = host;
            Message = messageData;
            NewState = newState;
            NodeId = nodeId;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Generic message data to be passed along.
        /// </summary>
        public string Message
        {
            get;
            private set;
        }

        /// <summary>
        /// The circuit's server host name.
        /// </summary>
        public string Host
        {
            get;
            private set;
        }

        /// <summary>
        /// The state the circuit flipped to.
        /// </summary>
        public CircuitBreakerState NewState
        {
            get;
            private set;
        }

        /// <summary>
        /// The unique node identifier of the circuit.
        /// </summary>
        public string NodeId
        {
            get;
            private set;
        }
        #endregion
    }
}