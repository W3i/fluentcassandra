using System;

namespace FluentCassandra.Connections
{
	public abstract class ConnectionProvider : IConnectionProvider
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="builder"></param>
		protected ConnectionProvider(IConnectionBuilder builder)
		{
			ConnectionBuilder = builder;
			Servers = new RoundRobinServerManager(builder);
		}

		/// <summary>
		/// 
		/// </summary>
		public IConnectionBuilder ConnectionBuilder { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public IServerManager Servers { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public abstract IConnection CreateConnection();

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public virtual IConnection Open()
		{
			var conn = CreateConnection();
			conn.Open();

			return conn;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="connection"></param>
		/// <returns></returns>
		public virtual bool Close(IConnection connection)
		{
			if (connection.IsOpen)
				connection.Close();

			return true;
        }

        /// <summary>
        /// Notify this provider that an error occurred with one of the connections.
        /// </summary>
        /// <param name="connection">The connection that caused an error</param>
        /// <param name="exc">the optional exception that happened</param>
        public abstract void ErrorOccurred(IConnection connection, Exception exc = null);

        /// <summary>
        /// Notify this provider that an operation carried out on one of the connections succeeded..
        /// </summary>
        /// <param name="connection">The connection that successfully performed an operation.</param>
        public abstract void OperationSucceeded(IConnection connection);
	}
}
