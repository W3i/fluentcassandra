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
        Whitelisted = 2,

        /// <summary>
        /// The server is still unavailable for general use, but can be tested
        /// with a small sample to determine re-eligibility for the whitelist.
        /// </summary>
        Greylisted = 3
    }
}
