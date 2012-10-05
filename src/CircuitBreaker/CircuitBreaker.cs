using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
namespace FluentCassandra.CircuitBreaker
{
    /// <summary>
    /// Implementation of the Circuit Breaker pattern.
    /// </summary>
    /// <remarks>Much of the pattern borrowed from: <a href="http://timross.wordpress.com/2008/02/10/implementing-the-circuit-breaker-pattern-in-c/"></a> </remarks>
    public class CircuitBreaker
    {
        #region Events
        /// <summary>
        /// Event that is fired when the stage of the circuit changes.
        /// </summary>
        public event EventHandler<CircuitStateChangedEventArgs> StateChanged;

        /// <summary>
        /// Event that is fired when the service level changes.
        /// </summary>
        public event EventHandler ServiceLevelChanged;
        #endregion

        #region Fields
        private uint _threshold;
        private int _failureCount;
        private string _node;
        private readonly System.Timers.Timer _timer;
        private CircuitBreakerState _state;
        private IList<Type> _catchExceptionTypes = new List<Type>(); //Add only those that will trip the breaker
        #endregion

        #region Public Properties
        /// <summary>
        /// The name of the node this breaker is for.
        /// </summary>
        public string Node
        {
            get
            { 
                return _node; 
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Node cannot be blank.");
                }

                _node = value;
            }
        }

        /// <summary>
        /// The unique node identifier of the circuit.
        /// </summary>
        public string NodeId
        {
            get;
            private set;
        }

        /// <summary>
        /// Number of failures allowed before the circuit trips.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is less than 1.</exception>
        public uint Threshold
        {
            get { return _threshold; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Threshold must be greater than zero");
                }

                _threshold = value;
            }
        }

        /// <summary>
        /// The time, in milliseconds, before the circuit attempts to close after being tripped.
        /// </summary>
        public uint Timeout
        {
            get
            { 
                return (uint)_timer.Interval;
            }
            set 
            { 
                _timer.Interval = value; 
            }
        }

        /// <summary>
        /// List of operation exception types the circuit breaker ignores.
        /// </summary>
        public IList<Type> CatchExceptionTypes
        {
            get 
            {
                return _catchExceptionTypes;
            }
        }

        /// <summary>
        /// The current service level of the circuit.
        /// </summary>
        public double ServiceLevel
        {
            get 
            {
                return ((_threshold - (double)_failureCount) / _threshold) * 100; 
            }
        }

        /// <summary>
        /// Current state of the circuit breaker.
        /// </summary>
        public CircuitBreakerState State
        {
            get { return _state; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of the circuit breaker for the given node id.
        /// </summary>
        /// <param name="nodeId">Id of the node</param>
        /// <param name="node">Hostname of the node</param>
        /// <param name="threshold">Desired trip threshold count</param>
        /// <param name="timeout">Reset interval in milliseconds.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="nodeId"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="threshold"/> is less than or equal to zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="timeout"/> is less than or equal to zero.</exception>
        public CircuitBreaker(string nodeId, string node, uint threshold, uint timeout)
        {
            #region Preconditions
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                throw new ArgumentNullException("nodeId");
            }
            if(string.IsNullOrWhiteSpace(node))
            {
                throw new ArgumentNullException("node");
            }
            if(threshold <= 0)
            {
                throw new ArgumentOutOfRangeException("threshold", "Threshold must be at least one.");
            }
            if(timeout <= 0)
            {
                throw new ArgumentOutOfRangeException("timeout", "Timeout must be at least one millisecond.");
            }
            #endregion

            NodeId = nodeId;
            _node = node;
            _threshold = threshold;
            _failureCount = 0;
            _state = CircuitBreakerState.Closed;
            _catchExceptionTypes = new List<Type>();
            _catchExceptionTypes.Add(typeof(SocketException));
            _timer = new System.Timers.Timer(timeout);
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Executes the operation.
        /// </summary>
        /// <param name="operation">Operation to execute</param>
        /// <param name="args">Operation arguments</param>
        /// <returns>Result of operation as an object</returns>
        /// <exception cref="OpenCircuitException">Thrown if the breaker is currently open.</exception>
        public object Execute(Delegate operation, params object[] args)
        {
            if (_state == CircuitBreakerState.Open)
            {
                throw new OpenCircuitException("Circuit breaker is currently open");
            }

            object result = null;
            try
            {
                // Execute operation
                result = operation.DynamicInvoke(args);
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    // If there is no inner exception, then the exception was caused by the invoker, so throw
                    throw;
                }

                if (_catchExceptionTypes.Contains(ex.InnerException.GetType()))
                {
                    // If exception is one of the ignored types, then throw original exception
                    throw ex.InnerException;
                }

                HandleStateWhenFailureOccurred();

                throw new OperationFailedException("Operation failed", ex.InnerException);
            }

            HandleStateWhenOperationSucceeded();

            return result;
        }

        /// <summary>
        /// Trips the circuit breaker if not already open.
        /// </summary>
        public void Trip()
        {
            if (_state != CircuitBreakerState.Open)
            {
                ChangeState(CircuitBreakerState.Open);

                _timer.Start();
            }
        }

        /// <summary>
        /// Resets the circuit breaker.
        /// </summary>
        public void Reset()
        {
            if (_state != CircuitBreakerState.Closed)
            {
                ChangeState(CircuitBreakerState.Closed);
                _failureCount = 0;
                _timer.Stop();
            }
        }

        /// <summary>
        /// Used for notifiying this circuit breaker than an external operation has failed.
        /// </summary>
        public void FailureOccurred()
        {
            HandleStateWhenFailureOccurred();
        }

        /// <summary>
        /// Used for notifying this circuit breaker that an external operation has succeeded.
        /// </summary>
        public void OperationSucceeded()
        {
            HandleStateWhenOperationSucceeded();
        }
        #endregion

        #region Private Methods
        private void HandleStateWhenOperationSucceeded()
        {
            if (_state == CircuitBreakerState.HalfOpen)
            {
                // If operation succeeded without error and circuit breaker 
                // is in a half-open state, then reset
                Reset();
            }

            if (_failureCount > 0)
            {
                // Decrement failure count to improve service level
                Interlocked.Decrement(ref _failureCount);

                OnServiceLevelChanged(new EventArgs());
            }
        }

        private void HandleStateWhenFailureOccurred()
        {
            if (_state == CircuitBreakerState.HalfOpen)
            {
                // Operation failed in a half-open state, so reopen circuit
                Trip();
            }
            else if (_failureCount < _threshold)
            {
                // Operation failed in an open state, so increment failure count and throw exception
                Interlocked.Increment(ref _failureCount);

                OnServiceLevelChanged(new EventArgs());
            }
            else if (_failureCount >= _threshold)
            {
                // Failure count has reached threshold, so trip circuit breaker
                Trip();
            }
        }

        /// <summary>
        /// Handles state change logic.
        /// </summary>
        /// <param name="newState"></param>
        private void ChangeState(CircuitBreakerState newState)
        {
            // Change the circuit breaker state
            _state = newState;

            // Raise changed event
            OnCircuitBreakerStateChanged();
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (State == CircuitBreakerState.Open)
            {
                // Attempt to close circuit by switching to a half-open state
                ChangeState(CircuitBreakerState.HalfOpen);

                _timer.Stop();
            }
        }

        private void OnCircuitBreakerStateChanged()
        {
            if (StateChanged != null)
            {
                StateChanged(this, new CircuitStateChangedEventArgs(NodeId, _node, _state, ""));
            }
        }

        private void OnServiceLevelChanged(EventArgs e)
        {
            if (ServiceLevelChanged != null)
            {
                ServiceLevelChanged(this, e);
            }
        }
        #endregion
    }
}
