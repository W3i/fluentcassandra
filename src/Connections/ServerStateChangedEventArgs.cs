using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentCassandra.Connections
{
    /// <summary>
    /// Represents the event data when a server state changes.
    /// </summary>
    public class ServerStateChangedEventArgs : EventArgs
    {
        #region Constructors
        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="serverId">The unique server identifier of whose state is changing.</param>
        /// <param name="host">The server host name.</param>
        /// <param name="newState">The <see cref="ServerState"/> the server has changed to.</param>
        /// <param name="messageData">generic message data to be passed along</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serverId"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="host"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="newState"/> is not one of the defined values.</exception>
        public ServerStateChangedEventArgs(string serverId, string host, ServerState newState, string messageData)
        {
            #region Preconditions
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentNullException("host");
            }
            if (string.IsNullOrWhiteSpace(serverId))
            {
                throw new ArgumentNullException("serverId");
            }
            if (!Enum.IsDefined(typeof(ServerState), newState))
            {
                throw new ArgumentOutOfRangeException("newState", string.Format("Given state '{0}' is not one of the defined values.", newState));
            }
            #endregion

            Message = messageData;
            ServerId = serverId;
            Host = host;
            NewState = newState;
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
        /// The server host name.
        /// </summary>
        public string Host
        {
            get;
            private set;
        }

        /// <summary>
        /// The state the server flipped to.
        /// </summary>
        public ServerState NewState
        {
            get;
            private set;
        }

        /// <summary>
        /// The unique identifier of the server.
        /// </summary>
        public string ServerId
        {
            get;
            private set;
        }
        #endregion
    }
}
