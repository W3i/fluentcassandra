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
        /// <param name="server">The server instance whose state is changing.</param>
        /// <param name="newState">The <see cref="ServerState"/> the server has changed to.</param>
        /// <param name="messageData">generic message data to be passed along</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="server"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="newState"/> is not one of the defined values.</exception>
        public ServerStateChangedEventArgs(Server server, ServerState newState, string messageData)
        {
            #region Preconditions
            if (server == null)
            {
                throw new ArgumentNullException("server");
            }
            if (!Enum.IsDefined(typeof(ServerState), newState))
            {
                throw new ArgumentOutOfRangeException("newState", string.Format("Given state '{0}' is not one of the defined values.", newState));
            }
            #endregion

            Message = messageData;
            Server = server;
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
        /// The state the server flipped to.
        /// </summary>
        public ServerState NewState
        {
            get;
            private set;
        }

        /// <summary>
        /// The server whose state is changing.
        /// </summary>
        public Server Server
        {
            get;
            private set;
        }
        #endregion
    }
}
