using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentCassandra.CircuitBreaker
{
    /// <summary>
    /// Represents the states a circuit can be in.
    /// </summary>
    public enum CircuitBreakerState
    {
        /// <summary>
        /// Current allowed to flow through.
        /// </summary>
        Closed,

        /// <summary>
        /// Nothing allowed to flow through.
        /// </summary>
        Open,

        /// <summary>
        /// Reset; in danger of closing if any more errors occur.
        /// </summary>
        HalfOpen
    }
}
