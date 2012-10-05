using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace FluentCassandra.CircuitBreaker
{
    /// <summary>
    /// Manages a set of node specific circuit breakers.
    /// </summary>
    public sealed class CircuitBreakerManager
    {
        #region Fields
        private EventHandler<CircuitStateChangedEventArgs> _onStateChanged;
        private EventHandler _onServiceLevelChanged;
        private ConcurrentDictionary<string, CircuitBreaker> _breakers;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of the class with the given event handlers, if provided, for events dispatched by any
        /// breaker managed in this instance.
        /// </summary>
        public CircuitBreakerManager(EventHandler<CircuitStateChangedEventArgs> onStateChanged = null, EventHandler onServiceLevelChanged = null)
        {
            _breakers = new ConcurrentDictionary<string, CircuitBreaker>();
            _onServiceLevelChanged = onServiceLevelChanged;
            _onStateChanged = onStateChanged;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Finds a circuit breaker by the given key.
        /// </summary>
        /// <param name="key">The key that the desired CircuitBreaker was inserted with.</param>
        /// <returns>The <see cref="CircuitBreaker"/> for the given <paramref name="key"/> if it exists. <c>null</c>, otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null or empty.</exception>
        public CircuitBreaker GetCircuitBreakerByKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key");
            }

            CircuitBreaker result = null;
            if (!_breakers.TryGetValue(key, out result))
            {
                result = null; // should have been, but just to make sure
            }
            return result;
        }

        /// <summary>
        /// Attempts to add a new <see cref="CircuitBreaker"/> to this manager using the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The unique key by which the circuit breaker is to be identified.</param>
        /// <param name="nodeName">The host name of the node for the breaker.</param>
        /// <param name="failureThresholdCount">The failure threshold for the breaker.</param>
        /// <param name="breakerResetIntervalMs">The number of milliseconds until an open breaker attempts to reset.</param>
        /// <returns><c>true</c> if the key/value pair was added to the manager successfully. If the key already exists, this method returns false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="nodeName"/> is null or empty.</exception>
        public bool AddCircuitBreakerByKey(string key, string nodeName, uint failureThresholdCount, uint breakerResetIntervalMs)
        {
            CircuitBreaker cbToAdd = new CircuitBreaker(key, nodeName, failureThresholdCount, breakerResetIntervalMs);
            if(_onStateChanged != null)
            {
                cbToAdd.StateChanged += _onStateChanged;
            }
            if(_onServiceLevelChanged != null)
            {
                cbToAdd.ServiceLevelChanged += _onServiceLevelChanged;
            }
            return _breakers.TryAdd(key, cbToAdd);
        }

        /// <summary>
        /// Attempts to remove and return a given <see cref="CircuitBreaker"/> by <paramref name="key"/> from this manager.
        /// </summary>
        /// <param name="key">The key that the desired CircuitBreaker was inserted with.</param>
        /// <param name="removedBreaker">The breaker that was removed from the manager, if it existed. <c>null</c>, otherwise.</param>
        /// <returns><c>true</c> if a <see cref="CircuitBreaker"/> was removed successfully; otherwise, <c>false</c>.</returns>
        public bool RemoveCircuitBreakerByKey(string key, out CircuitBreaker removedBreaker)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key");
            }
            return _breakers.TryRemove(key, out removedBreaker);
        }

        /// <summary>
        /// Notifies the breaker for node specified by <paramref name="nodeId"/> that an error has occurred.
        /// </summary>
        /// <param name="nodeId">The id of the node that the failure occurred on.</param>
        /// <returns><c>true</c> if the given node is managed by this instance and was able to notify its breaker. <c>false</c>, otherwise</returns>
        public bool ForwardErrorOccurredToBreaker(string nodeId)
        {
            CircuitBreaker breaker = GetCircuitBreakerByKey(nodeId);
            if (breaker != null)
            {
                breaker.FailureOccurred();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Notifies the breaker for node specified by <paramref name="nodeId"/> that an operation succeeded.
        /// </summary>
        /// <param name="nodeId">The id of the node that the operation was performed on.</param>
        /// <returns><c>true</c> if the given node is managed by this instance and was able to notify its breaker. <c>false</c>, otherwise</returns>
        public bool ForwardOperationSuccessToBreaker(string nodeId)
        {
            CircuitBreaker breaker = GetCircuitBreakerByKey(nodeId);
            if (breaker != null)
            {
                breaker.OperationSucceeded();
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
