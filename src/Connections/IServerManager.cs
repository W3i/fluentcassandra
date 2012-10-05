using System;
using System.Collections.Generic;

namespace FluentCassandra.Connections
{
	public interface IServerManager : IEnumerable<Server>
	{
		bool HasNext { get; }
		Server Next();

        /// <summary>
        /// Notify the server manager that an error has occurred for the given server.
        /// </summary>
        /// <param name="server">The server that was being used when the error occurred.</param>
        /// <param name="exc">The exception that accompanies the error, or null if not available.</param>
		void ErrorOccurred(Server server, Exception exc = null);

        /// <summary>
        /// Notify the server manager that an operation succeeded for the given server.
        /// </summary>
        /// <param name="server">The server that was used successfully.</param>
        void OperationSucceeded(Server server);

		
		void BlackList(Server server);
        void WhiteList(Server server);

		void Add(Server server);
		void Remove(Server server);
		
		/// <summary>
        /// Manager dispatches an event when the state of one of the managed servers changes.
        /// </summary>
        event EventHandler<ServerStateChangedEventArgs> ServerStateChanged;
	}
}
