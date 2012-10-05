using System;

namespace FluentCassandra.Connections
{
	public interface IConnectionProvider
	{
		IConnectionBuilder ConnectionBuilder { get; }

		IServerManager Servers { get; }

		IConnection CreateConnection();

		IConnection Open();

		bool Close(IConnection connection);

        /// <summary>
        /// Notify this provider that an error occurred with one of the connections.
        /// </summary>
        /// <param name="connection">The connection that caused an error</param>
        /// <param name="exc">the optional exception that happened</param>
        void ErrorOccurred(IConnection connection, Exception exc = null);

        /// <summary>
        /// Notify this provider that an operation carried out on one of the connections succeeded..
        /// </summary>
        /// <param name="connection">The connection that successfully performed an operation.</param>
        void OperationSucceeded(IConnection connection);
	}
}
