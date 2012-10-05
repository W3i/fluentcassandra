using System;

namespace FluentCassandra.Connections
{
	public class Server
	{
		public const int DefaultPort = 9160;
		public const int DefaultTimeout = 0;

		public Server(string host = "127.0.0.1", int port = DefaultPort, int timeout = DefaultTimeout)
		{
			Host = host;
			Port = port;
			Timeout = timeout;
		}

		public int Port { get; private set; }

		public string Host { get; private set; }

		public int Timeout { get; private set; }

		public override string ToString()
		{
			return String.Concat(Host, ":", Port, ",", Timeout, " secs");
		}

        /// <summary>
        /// Gets a string that uniquely identifies this server.
        /// </summary>
        public string Id
        {
            get
            {
                return ToString();
            }
        }

        /// <summary>
        /// Determines if this server and the given object are equal.
        /// </summary>
        /// <param name="obj">object to compare to</param>
        /// <returns><c>true</c> if the object is a <see cref="Server"/> and the host and port are the same; <c>false</c> otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj is Server)
            {
                Server objCompare = obj as Server;
                return Port == objCompare.Port && Host.Equals(objCompare.Host, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return Port.GetHashCode() + Host.GetHashCode();
        }
	}
}
