using System;
using System.Net.Sockets;

namespace FluentCassandra.Connections
{
	public class NormalConnectionProvider : ConnectionProvider
	{
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

			while (Servers.HasNext)
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
            if (!Servers.HasNext)
            {
                // No server available to create a connection.
                return null;
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
    }
}
