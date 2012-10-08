using System;
using System.Collections.Generic;
using System.Diagnostics;
using FluentCassandra.CircuitBreaker;
using System.Linq;

namespace FluentCassandra.Connections
{
	public class RoundRobinServerManager : IServerManager
	{
		private readonly object _lock = new object();

		private List<Server> _servers;
		private Queue<Server> _serverQueue;
		private HashSet<Server> _blackListed;
        private CircuitBreakerManager _circuitBreakerManager;
        private uint _serverCircuitBreakerErrorThresholdCount;
        private uint _serverCircuitBreakerRetryIntervalMs;

		public RoundRobinServerManager(IConnectionBuilder builder)
		{
			_servers = new List<Server>(builder.Servers);
			_serverQueue = new Queue<Server>(_servers);
			_blackListed = new HashSet<Server>();
            _serverCircuitBreakerErrorThresholdCount = builder.ServerCircuitBreakerErrorThresholdCount;
            _serverCircuitBreakerRetryIntervalMs = builder.ServerCircuitBreakerRetryIntervalMs;
            _circuitBreakerManager = new CircuitBreakerManager(onStateChanged: CircuitStateChanged);
            InitializeCircuitBreakers(_serverCircuitBreakerErrorThresholdCount, _serverCircuitBreakerRetryIntervalMs);
		}

		private bool IsBlackListed(Server server)
		{
			return _blackListed.Contains(server);
		}

        /// <summary>
        /// Generates a circuit breaker for each server in this manager.
        /// </summary>
        private void InitializeCircuitBreakers(uint serverErrorThresholdCount, uint serverRetryIntervalMs)
        {
            foreach (Server server in _servers)
            {
                CreateAndAddCircuitBreaker(serverErrorThresholdCount, serverRetryIntervalMs, server);
            }
        }

        /// <summary>
        /// Creates a new circuit breaker for the given server and adds it to the collection of breakers for this manager.
        /// </summary>
        /// <param name="serverErrorThresholdCount">Breaker error threshold</param>
        /// <param name="serverRetryIntervalMs">Breaker retry interval</param>
        /// <param name="server">Server the breaker is for</param>
        private void CreateAndAddCircuitBreaker(uint serverErrorThresholdCount, uint serverRetryIntervalMs, Server server)
        {
            if (!_circuitBreakerManager.AddCircuitBreakerByKey(server.Id, server.Host, serverErrorThresholdCount, serverRetryIntervalMs))
            {
                System.Diagnostics.Trace.TraceError(string.Format("Could not add duplicate circuit breaker key '{0}'.", server.ToString()));
            }
        }

        /// <summary>
        /// Hanles the circuit breaker StateChanged event.
        /// </summary>
        /// <param name="source">source of the event</param>
        /// <param name="args">info about the state of the circuit breaker when the even happened.</param>
        private void CircuitStateChanged(object source, CircuitStateChangedEventArgs args)
        {
            if (args != null)
            {
                Server server = _servers.SingleOrDefault(s => s.Id.Equals(args.NodeId, StringComparison.OrdinalIgnoreCase));
                if (server != null)
                {
                    switch (args.NewState)
                    {
                        case CircuitBreakerState.Open:
                            BlackList(server);
                            break;
                        case CircuitBreakerState.Closed:
                        case CircuitBreakerState.HalfOpen:
                            WhiteList(server); // let the system try it again.
                            break;
                    }
                }
            }
            else
            {
                System.Diagnostics.Trace.TraceError("RoundRobinServerManager did not respond to a circuit breaker state change because args was null.");
            }
        }

        /// <summary>
        /// Let all subscribers know the state of one of the servers has changed.
        /// </summary>
        private void DispatchStateChangedEvent(ServerStateChangedEventArgs args)
        {
            if (ServerStateChanged != null)
            {
                ServerStateChanged(this, args);
            }
        }

        #region IServerManager Members

        public bool HasNext
		{
            get { lock (_lock) { return (_serverQueue.Count - _blackListed.Count) > 0; } }
		}

		public Server Next()
		{
			Server server;

			lock (_lock)
			{
				do
				{
					server = _serverQueue.Dequeue();

					if (IsBlackListed(server))
						server = null;
					else
						_serverQueue.Enqueue(server);
				}
				while (_serverQueue.Count > 0 && server == null);	
			}

			return server;
		}

		public void Add(Server server)
		{
			lock (_lock)
			{
				_servers.Add(server);
				_serverQueue.Enqueue(server);

                // Add a new circuit breaker for the server
                CreateAndAddCircuitBreaker(_serverCircuitBreakerErrorThresholdCount, _serverCircuitBreakerRetryIntervalMs, server);
			}
		}

        /// <summary>
        /// Notify the server manager that an error has occurred for the given server.
        /// </summary>
        /// <param name="server">The server that was being used when the error occurred.</param>
        /// <param name="exc">The exception that accompanies the error, or null if not available.</param>
		public void ErrorOccurred(Server server, Exception exc = null)
		{
            Debug.WriteLineIf(exc != null, string.Format("RoundRobinServerManager error: {0}\n\tStackTrace: {1}", exc.Message, exc.StackTrace), "connection");
            _circuitBreakerManager.ForwardErrorOccurredToBreaker(server.Id);
		}

		public void BlackList(Server server)
		{
			lock (_lock)
			{
                if (_blackListed.Add(server))
                {
                    System.Diagnostics.Trace.TraceWarning("Blacklisted server: {0}.", server);
                    DispatchStateChangedEvent(new ServerStateChangedEventArgs(server.Id, server.Host, ServerState.Blacklisted, string.Empty));
                }
			}
		}

        /// <summary>
        /// Notify the server manager to immediately whitelist the given server.
        /// </summary>
        /// <param name="server">The server that should be whitelisted..</param>
        public void WhiteList(Server server)
        {
            lock (_lock)
            {
                if (_blackListed.Remove(server))
                {
                    // Server was blacklisted, so double check the queue and add it back in! Here we go.
                    if (!_serverQueue.Contains(server))
                    {
                        _serverQueue.Enqueue(server);
                        System.Diagnostics.Trace.TraceInformation("RRSMgr Whitelisted server: {0}.", server);
                        DispatchStateChangedEvent(new ServerStateChangedEventArgs(server.Id, server.Host, ServerState.Whitelisted, string.Empty));
                    }
                }
            }
        }

        /// <summary>
        /// Notifies this manager that an operation succeeded on the managed server specified by <paramref name="server"/>.
        /// </summary>
        /// <param name="server">The server that the operation was performed on.</param>
        public void OperationSucceeded(Server server)
        {
            _circuitBreakerManager.ForwardOperationSuccessToBreaker(server.Id);
        }

        /// <summary>
        /// Dispatches an event when the state of one of the managed servers changes.
        /// </summary>
        public event EventHandler<ServerStateChangedEventArgs> ServerStateChanged;

		public void Remove(Server server)
		{
			lock (_lock)
			{
				_servers.Remove(server);
				_serverQueue = new Queue<Server>();
				_blackListed.RemoveWhere(x => x == server);

				foreach (var s in _servers)
				{
					if (!_blackListed.Contains(s))
						_serverQueue.Enqueue(s);
				}

                // Clean up the circuit breaker for this removed node.
                CircuitBreaker.CircuitBreaker removed;
                _circuitBreakerManager.RemoveCircuitBreakerByKey(server.Id, out removed);
			}
		}

		#endregion

		#region IEnumerable<Server> Members

		public IEnumerator<Server> GetEnumerator()
		{
			return _servers.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}