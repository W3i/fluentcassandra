using System;
using Apache.Cassandra;
using System.Collections.Generic;

namespace FluentCassandra.Connections
{
	public interface IConnectionBuilder
	{
		string Keyspace { get; }
		IList<Server> Servers { get; }

		bool Pooling { get; }
		int MinPoolSize { get; }
		int MaxPoolSize { get; }
		TimeSpan ConnectionTimeout { get; }
		TimeSpan ConnectionLifetime { get; }
		ConnectionType ConnectionType { get; }

		int BufferSize { get; }
		ConsistencyLevel ReadConsistency { get; }
		ConsistencyLevel WriteConsistency { get; }

		string CqlVersion { get; }
		bool CompressCqlQueries { get; }

		string Username { get; }
		string Password { get; }

		string Uuid { get; }

        /// <summary>
        /// The number of errors a server can incur before the circuit breaker trips.
        /// </summary>
        uint ServerCircuitBreakerErrorThresholdCount { get; }

        /// <summary>
        /// The interval (in ms) to retry a tripped server.
        /// </summary>
        uint ServerCircuitBreakerRetryIntervalMs { get; }
    }
}
