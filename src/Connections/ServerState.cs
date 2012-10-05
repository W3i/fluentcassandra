using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentCassandra.Connections
{
    /// <summary>
    /// Represents the states a server can be in.
    /// </summary>
    public enum ServerState
    {
        /// <summary>
        /// The server has failed too many times and has been banned.
        /// </summary>
        Blacklisted = 1,

        /// <summary>
        /// The server is available for use.
        /// </summary>
        Whitelisted = 2
    }
}
