using System;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace FluentCassandra.Connections
{
	public class NormalConnectionProvider : ConnectionProvider
	{
	    private ConcurrentQueue<IConnection> _retryQueue;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="builder"></param>
		public NormalConnectionProvider(IConnectionBuilder builder)
			: base(builder)
		{
			if (builder.Servers.Count > 1 && builder.ConnectionTimeout == TimeSpan.Zero)
				throw new CassandraException("You must specify a timeout when using multiple servers.");

			ConnectionTimeout = builder.ConnectionTimeout;
            _retryQueue = new ConcurrentQueue<IConnection>();
            Servers.ServerStateChanged += HandleServerStateChanged;
		}

		/// <summary>
		/// 
		/// </summary>
		public TimeSpan ConnectionTimeout { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override IConnection Open()
		{
			IConnection conn = null;

			while (Servers.HasNext || _retryQueue.Count > 0)
			{
				try
				{
					conn = CreateConnection();
					conn.Open();
					break;
				}
				catch (SocketException exc)
				{
                    if (conn != null)
                    {
                        if (conn.Server != null)
                        {
                            Servers.ErrorOccurred(conn.Server, exc);
                        }
                        Close(conn);
                        conn = null;
                    }
				}
			}

			if (conn == null)
				throw new CassandraException("No connection could be made because all servers have failed.");

			return conn;
		}

        /// <summary>
        /// Obtain a connection from next available server.
        /// </summary>
        /// <returns>A connection</returns>
        /// <remarks>Will return <c>null</c> if no servers are available.</remarks>
        public override IConnection CreateConnection()
		{
            if (!Servers.HasNext && _retryQueue.Count == 0)
            {
                // No server available to create a connection.
                return null;
            }

            if(_retryQueue.Count > 0)
            {
                IConnection connection;
                if(_retryQueue.TryDequeue(out connection) && connection != null)
                {
                    // Just return the retry connection.
                    return connection;
                }
            }

            var server = Servers.Next();

            if (server == null)
            {
                // If for some reason the server was blacklisted or became unavailable after we have checked HasNext, return null
                return null;
            }

			var conn = new Connection(server, ConnectionBuilder);

			return conn;
        }

        /// <summary>
        /// Notify this provider that an error occurred with one of the connections.
        /// </summary>
        /// <param name="connection">The connection that caused an error</param>
        /// <param name="exc">the optional exception that happened</param>
        public override void ErrorOccurred(IConnection connection, Exception exc = null)
        {
            if (connection != null)
            {
                if (connection.Server != null)
                {
                    // Let the server manager know that a connection from the given server had an error.
                    Servers.ErrorOccurred(connection.Server, exc);
                }

                if (connection.IsOpen)
                {
                    Close(connection);
                }
            }
        }

        /// <summary>
        /// Notify this provider that an operation carried out on one of the connections succeeded..
        /// </summary>
        /// <param name="connection">The connection that successfully performed an operation.</param>
        public override void OperationSucceeded(IConnection connection)
        {
            if (connection != null)
            {
                if (connection.Server != null)
                {
                    // Let the server manager know that a connection from the given server had a successful operation.
                    Servers.OperationSucceeded(connection.Server);
                }
            }
        }

        /// <summary>
        /// Handles ServerStateChanged events coming from the server manager.
        /// </summary>
        /// <param name="source">The event source.</param>
        /// <param name="args">The data associated with the event.</param>
        private void HandleServerStateChanged(object source, ServerStateChangedEventArgs args)
        {
            if (args != null)
            {
                if(args.NewState == ServerState.Greylisted)
                {
                    HandleServerGreylisted(args.Server);
                }
            }
            else
            {
                System.Diagnostics.Trace.TraceWarning("Normal connection provider did not respond to a server state change because passed event args was null.");
            }
        }

        /// <summary>
        /// Takes any neccessary action to retry the greylisted server just once.
        /// </summary>
        /// <param name="server">The server that was greylisted and needs to be retried.</param>
        private void HandleServerGreylisted(Server server)
        {
            // Force a single connection into the mix so that the app tries the machine again. It should only be used once or at most a handful of times because
            // a single success will result in whitelisting, and failure in re-blacklisting.
            IConnection conn = null;
            try
            {
                System.Diagnostics.Trace.TraceInformation("Attempting to create a new connection to greylisted server [{0]} to retry it.");
                conn = new Connection(server, ConnectionBuilder);
                _retryQueue.Enqueue(conn);
            }
            catch (SocketException exc)
            {
                System.Diagnostics.Trace.TraceWarning("Unable to reconnect to greylisted server [{0}]; Message: {1}",
                                                      server, exc.Message);
                // Opening the connection failed.  Notify the server manager, which will result in a blacklist.
                Servers.ErrorOccurred(server, exc);
                if (conn != null)
                {
                    Close(conn);
                }
            }
        }
    }
}
