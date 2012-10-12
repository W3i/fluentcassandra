using System;
using System.Collections.Generic;
using System.Threading;

namespace FluentCassandra.Connections
{
    public class PooledConnectionProvider : NormalConnectionProvider
    {
        private readonly object _lock = new object();

        private readonly Queue<IConnection> _freeConnections = new Queue<IConnection>();
        private readonly List<IConnection> _usedConnections = new List<IConnection>();
        private readonly Timer _maintenanceTimer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        public PooledConnectionProvider(IConnectionBuilder builder)
            : base(builder)
        {
            MinPoolSize = builder.MinPoolSize;
            MaxPoolSize = builder.MaxPoolSize;
            ConnectionLifetime = builder.ConnectionLifetime;

            _maintenanceTimer = new Timer(o => Cleanup(), null, 30000L, 30000L);
            //Servers.ServerStateChanged event handler assigned in base class
        }

        /// <summary>
        /// 
        /// </summary>
        public int MinPoolSize { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int MaxPoolSize { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan ConnectionLifetime { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override IConnection CreateConnection()
        {
            IConnection conn = null;

            lock (_lock)
            {
                if (_freeConnections.Count > 0)
                {
                    conn = _freeConnections.Dequeue();
                    _usedConnections.Add(conn);
                }
                else if (_freeConnections.Count + _usedConnections.Count >= MaxPoolSize)
                {
                    if (!Monitor.Wait(_lock, TimeSpan.FromSeconds(30)))
                        throw new CassandraException("No connection could be made, timed out trying to aquire a connection from the connection pool.");

                    return CreateConnection();
                }
                else
                {
                    conn = base.CreateConnection();
                    _usedConnections.Add(conn);
                }
            }

            return conn;
        }

        /// <summary>
        /// Release the connection from the used state and return it to the pool if the
        /// connection is still open.
        /// </summary>
        /// <param name="connection">The connection to be released.</param>
        /// <returns><c>true</c></returns>
        public override bool Close(IConnection connection)
        {
            lock (_lock)
            {
                // Check that the connection hasn't already been removed by failure
                bool connFound = _usedConnections.Remove(connection);

                if (connFound)
                {
                    if (IsAlive(connection))
                    {
                        _freeConnections.Enqueue(connection);
                    }
                }
                else
                {
                    // Close it if it is not going to be used again.
                    if (IsAlive(connection))
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("Closing orphaned connection {0}", connection.Server.ToString()));
                        connection.Close();
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Cleans up this instance.
        /// </summary>
        public void Cleanup()
        {
            CheckFreeConnectionsAlive();
        }

        /// <summary>
        /// Determines whether the connection is alive.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>True if alive; otherwise false.</returns>
        private bool IsAlive(IConnection connection)
        {
            if (ConnectionLifetime > TimeSpan.Zero && connection.Created.Add(ConnectionLifetime) < DateTime.UtcNow)
                return false;

            return connection.IsOpen;
        }

        /// <summary>
        /// The check free connections alive.
        /// </summary>
        private void CheckFreeConnectionsAlive()
        {
            lock (_lock)
            {
                var freeConnections = _freeConnections.ToArray();
                _freeConnections.Clear();

                foreach (var free in freeConnections)
                {
                    if (IsAlive(free))
                        _freeConnections.Enqueue(free);
                    else
                        base.Close(free);
                }
            }
        }

        /// <summary>
        /// Handles ServerStateChanged events coming from the server manager.
        /// </summary>
        /// <param name="source">The event source.</param>
        /// <param name="args">The data associated with the event.</param>
        protected override void HandleServerStateChanged(object source, ServerStateChangedEventArgs args)
        {
            if (args != null)
            {
                switch (args.NewState)
                {
                    case ServerState.Whitelisted:
                        HandleServerWhitelisted(args.Server);
                        break;
                    case ServerState.Blacklisted:
                        HandleServerBlacklisted(args.Server);
                        break;
                    case ServerState.Greylisted:
                        HandleServerGreylisted(args.Server);
                        break;
                }
            }
            else
            {
                System.Diagnostics.Trace.TraceWarning("Pooled connection provider did not respond to a server state change because passed event args was null.");
            }
        }

        /// <summary>
        /// Takes any neccessary action on the pool to retry the greylisted server just once.
        /// </summary>
        /// <param name="server">The server that was greylisted and needs to be retried.</param>
        private void HandleServerGreylisted(Server server)
        {
            // Force a single connection into the pool so that the app tries the machine again. It should only be used once or at most a handful of times because
            // a single success will result in whitelisting, and failure in re-blacklisting.

            System.Diagnostics.Trace.TraceInformation(
                    "Pool attempting to create a new connection to greylisted server [{0}] to retry it.", server);
            IConnection conn = new Connection(server, ConnectionBuilder);
            _freeConnections.Enqueue(conn);
        }

        /// <summary>
        /// Takes any neccessary action on the pool to free up resources and make the whitelisted server usable again.
        /// </summary>
        /// <param name="server">Instance of the whitelisted server.</param>
        private void HandleServerWhitelisted(Server server)
        {
            // Free up resources in the pool and make the server usable again.  Only needed when ConnectionLifetime = 0 (infinity), otherwise connections will eventually
            // die and this server will be re-used again.
            if (ConnectionLifetime <= TimeSpan.Zero)
            {
                lock (_lock)
                {
                    System.Diagnostics.Trace.TraceInformation("Clearing the connection pool because server [{0}] has been whitelisted and ConnectionLifetime is infinity.", server);
                    _usedConnections.Clear();
                    _freeConnections.Clear();
                }
            }
        }

        /// <summary>
        /// Cleans up the connections in the pools for the given blacklisted server id.
        /// </summary>
        /// <param name="blackListedServer">Instance of the blacklisted server.</param>
        private void HandleServerBlacklisted(Server blackListedServer)
        {
            lock (_lock)
            {
                int usedCount = _usedConnections.Count;
                List<IConnection> usedConnsToRemove = new List<IConnection>();
                for (int u = 0; u < usedCount; u++)
                {
                    if (_usedConnections[u].Server.Equals(blackListedServer))
                    {
                        usedConnsToRemove.Add(_usedConnections[u]);
                    }
                }
                System.Diagnostics.Debug.WriteLine("Removing {0} used connections for failed server {1}", usedConnsToRemove.Count, blackListedServer);
                usedConnsToRemove.ForEach(conn => _usedConnections.Remove(conn));

                int freeCount = _freeConnections.Count;
                for (int i = 0; i < freeCount; i++)
                {
                    // Clean up the free queue by only requeing those not from the blacklisted server
                    IConnection freeConn = _freeConnections.Dequeue();
                    if (freeConn.Server.Equals(blackListedServer))
                    {
                        System.Diagnostics.Debug.WriteLine("Closing free connection {0} for failed server {1}", i + 1, blackListedServer);
                        freeConn.Close();
                        Close(freeConn);
                    }
                    else
                    {
                        _freeConnections.Enqueue(freeConn);
                    }
                }
            }
        }
    }
}
