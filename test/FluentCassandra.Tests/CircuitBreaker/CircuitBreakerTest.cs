using FluentCassandra.CircuitBreaker;
using Xunit;
using System;
using System.Timers;
using System.Collections.Generic;
using System.Threading;

namespace FluentCassandra.CircuitBreaker.Tests
{
    
    
    /// <summary>
    ///This is a test class for CircuitBreakerTest and is intended
    ///to contain all CircuitBreakerTest Unit Tests
    ///</summary>
    ///<remarks>Much of the pattern and these tests borrowed from: <a href="http://timross.wordpress.com/2008/02/10/implementing-the-circuit-breaker-pattern-in-c/"></a> </remarks>
    public class CircuitBreakerTest
    {
        private delegate int TestDelegate(int a, int b);

        public int ValidOperation(int a, int b)
        {
            return a + b;
        }

        public void FailedOperation()
        {
            throw new TimeoutException("Network not available");
        }


        [Fact]
        public void CanCreateCircuitBreaker()
        {
            CircuitBreaker cb = new CircuitBreaker("test_id", "test", 5, 60000);
            Assert.Equal(5U, cb.Threshold);
            Assert.Equal(60000U, cb.Timeout);
            Assert.Equal(100, cb.ServiceLevel);
            Assert.Equal("test", cb.Node);
            Assert.Equal("test_id", cb.NodeId);
        }

        [Fact]
        public void CircuitBreakerCtrNullNodeTest()
        {
            var thrown = Assert.Throws<ArgumentNullException>(delegate()
            {
            new CircuitBreaker("test_id", null, 5, 60000);
            });

            Assert.Equal("node", thrown.ParamName);
        }

        [Fact]
        public void CircuitBreakerCtrEmptyNodeTest()
        {
            var thrown = Assert.Throws<ArgumentNullException>(delegate()
            {
            new CircuitBreaker("test_id", string.Empty, 5, 60000);
                });

            Assert.Equal("node", thrown.ParamName);
        }

        [Fact]
        public void CircuitBreakerCtrNullNodeIdTest()
        {
            var thrown = Assert.Throws<ArgumentNullException>(delegate()
            {
            new CircuitBreaker(null, "test", 5, 60000);});

            Assert.Equal("nodeId", thrown.ParamName);
        }

        [Fact]
        public void CircuitBreakerCtrEmptyNodeIdTest()
        {
            var thrown = Assert.Throws<ArgumentNullException>(delegate()
            {
            new CircuitBreaker(string.Empty, "test", 5, 60000);
                });

            Assert.Equal("nodeId", thrown.ParamName);
        }

        [Fact]
        public void CircuitBreakerCtrInvalidThresholdTest()
        {
            var thrown = Assert.Throws<ArgumentOutOfRangeException>(delegate()
            {
            new CircuitBreaker("testid", "test", 0, 60000);
                });

            Assert.Equal("threshold", thrown.ParamName);
            Assert.Contains("Threshold must be at least one", thrown.Message);
        }

        [Fact]
        public void CircuitBreakerCtrInvalidTimeoutTest()
        {
            var thrown = Assert.Throws<ArgumentOutOfRangeException>(delegate()
            {
            new CircuitBreaker("testid", "test", 5, 0);
            });

            Assert.Equal("timeout", thrown.ParamName);
            Assert.Contains("Timeout must be at least one", thrown.Message);
        }

        [Fact]
        public void CanSetCircuitBreakerProperties()
        {
            CircuitBreaker cb = new CircuitBreaker("test_id", "test", 5, 60000);
            cb.Threshold = 10;
            cb.Timeout = 120000;
            cb.Node = "test2";

            Assert.Equal(10U, cb.Threshold);
            Assert.Equal(120000U, cb.Timeout);
            Assert.Equal("test2", cb.Node);
        }

        [Fact]
        public void CannotSetInvalidThreshold()
        {
            CircuitBreaker cb = new CircuitBreaker("test_id", "test", 5, 60000);
            var thrown = Assert.Throws<ArgumentOutOfRangeException>(delegate()
            {
            cb.Threshold = 0;
            });

            Assert.Equal("value", thrown.ParamName);
            Assert.Contains("Threshold must be greater than zero", thrown.Message);
        }

        [Fact]
        public void CannotSetNodeEmptyTest()
        {
            CircuitBreaker cb = new CircuitBreaker("test_id", "test", 5, 60000);
            var thrown = Assert.Throws<ArgumentException>(delegate()
            {
                cb.Node = string.Empty;
            });

            Assert.Contains("Node cannot be blank", thrown.Message);
        }

        [Fact]
        public void CannotSetNodeNullTest()
        {
            CircuitBreaker cb = new CircuitBreaker("test_id", "test", 5, 60000);
            var thrown = Assert.Throws<ArgumentException>(delegate()
            {
            cb.Node = null;
            });

            Assert.Contains("Node cannot be blank", thrown.Message);
        }

        [Fact]
        public void CanExecuteOperation()
        {
            CircuitBreaker cb = new CircuitBreaker("test_id", "test", 5, 60000);
            object result = cb.Execute(new TestDelegate(ValidOperation), 1, 2);

            Assert.NotNull(result);
            Assert.Equal(3, (int)result);
        }

        [Fact]
        public void CanGetFailureCount()
        {
            CircuitBreaker cb = new CircuitBreaker("test_id", "test", 5, 60000);
            try
            {
                cb.Execute(new ThreadStart(FailedOperation));
            }
            catch (OperationFailedException) { }
            Assert.Equal(80, cb.ServiceLevel);

            try
            {
                cb.Execute(new ThreadStart(FailedOperation));
            }
            catch (OperationFailedException) { }
            Assert.Equal(60, cb.ServiceLevel);


            cb.Execute(new TestDelegate(ValidOperation), 1, 2);

            Assert.Equal(80, cb.ServiceLevel);
        }

        [Fact]
        public void CanGetOriginalException()
        {
            CircuitBreaker cb = new CircuitBreaker("test_id", "test", 5, 60000);
            Exception innerException = null;
            try
            {
                cb.Execute(new ThreadStart(FailedOperation));
            }
            catch (OperationFailedException ex)
            {
                innerException = ex.InnerException;
            }
            Assert.IsType(typeof(TimeoutException), innerException);
        }

        [Fact]
        public void CanTripBreaker()
        {
            CircuitBreaker cb = new CircuitBreaker("test_id", "test", 5, 60000);
            var thrown = Assert.Throws<OpenCircuitException>(delegate()
            {
                for (int i = 0; i < cb.Threshold + 5; i++)
                {
                    try
                    {
                        cb.Execute(new ThreadStart(FailedOperation));
                    }
                    catch (OperationFailedException) { }
                    catch (OpenCircuitException)
                    {
                        Assert.Equal(0, cb.ServiceLevel);
                        throw;
                    }
                }
            });
        }

        [Fact]
        public void CanTripBreakerFromExternalFailure()
        {
            CircuitBreaker cb = new CircuitBreaker("test_id", "test", 5, 60000);
            for (int i = 0; i < cb.Threshold + 5; i++)
            {
                cb.FailureOccurred();
                if (i+1 < cb.Threshold)
                {
                    Assert.Equal(CircuitBreakerState.Closed, cb.State);
                }
                if (i+1 == cb.Threshold + 1) // Closes on next failure after threshold.
                {
                    Assert.Equal(CircuitBreakerState.Open, cb.State);
                }
            }
        }

        [Fact]
        public void CanResetBreaker()
        {
            CircuitBreaker cb = new CircuitBreaker("test_id", "test", 5, 60000);
            try
            {
                cb.Execute(new ThreadStart(FailedOperation));
            }
            catch (OperationFailedException) { }
            catch (OpenCircuitException)
            {
                cb.Reset();
                Assert.Equal(CircuitBreakerState.Closed, cb.State);
                Assert.Equal(0, cb.ServiceLevel);
            }
        }

        [Fact]
        public void CanForceTripBreaker()
        {
            CircuitBreaker cb = new CircuitBreaker("test_id", "test", 5, 60000);
            Assert.Equal(CircuitBreakerState.Closed, cb.State);

            cb.Trip();

            Assert.Equal(CircuitBreakerState.Open, cb.State);

            // Calling execute when circuit is tripped should throw an OpenCircuitException
            var thrown = Assert.Throws<OpenCircuitException>(delegate()
            {
                cb.Execute(new TestDelegate(ValidOperation), 1, 2);
            });
        }

        [Fact]
        public void CanForceResetBreaker()
        {
            CircuitBreaker cb = new CircuitBreaker("test_id", "test", 5, 60000);
            Assert.Equal(CircuitBreakerState.Closed, cb.State);

            cb.Trip();

            Assert.Equal(CircuitBreakerState.Open, cb.State);

            cb.Reset();

            Assert.Equal(CircuitBreakerState.Closed, cb.State);
            Assert.Equal(100, cb.ServiceLevel);

            object result = cb.Execute(new TestDelegate(ValidOperation), 1, 2);

            Assert.NotNull(result);
            Assert.Equal(3, (int)result);
        }

        [Fact]
        public void CanCloseBreakerAfterTimeout()
        {
            CircuitBreaker cb = new CircuitBreaker("test_id", "test", 5, 60000);
            cb.Timeout = 500; // Shorten timeout to 500 milliseconds

            cb.Trip();

            Assert.Equal(CircuitBreakerState.Open, cb.State);

            Thread.Sleep(1000);

            Assert.Equal(CircuitBreakerState.HalfOpen, cb.State);

            try
            {
                // Attempt failed operation
                cb.Execute(new ThreadStart(FailedOperation));
            }
            catch (OperationFailedException) { }

            Assert.Equal(CircuitBreakerState.Open, cb.State);

            Thread.Sleep(1000);

            Assert.Equal(CircuitBreakerState.HalfOpen, cb.State);

            // Attempt successful operation
            cb.Execute(new TestDelegate(ValidOperation), 1, 2);

            Assert.Equal(CircuitBreakerState.Closed, cb.State);
        }

        [Fact]
        public void CanCloseBreakerAfterTimeoutWithExternalMethods()
        {
            CircuitBreaker cb = new CircuitBreaker("test_id", "test", 5, 60000);
            cb.Timeout = 500; // Shorten timeout to 500 milliseconds

            cb.Trip();

            Assert.Equal(CircuitBreakerState.Open, cb.State);

            Thread.Sleep(1000);

            Assert.Equal(CircuitBreakerState.HalfOpen, cb.State);

            cb.FailureOccurred(); // create another error.

            Assert.Equal(CircuitBreakerState.Open, cb.State);

            Thread.Sleep(1000);

            Assert.Equal(CircuitBreakerState.HalfOpen, cb.State);

            // Attempt successful operation
            cb.OperationSucceeded();

            Assert.Equal(CircuitBreakerState.Closed, cb.State);
        }

        [Fact]
        public void CanRaiseStateChangedEvent()
        {
            CircuitBreaker cb = new CircuitBreaker("test_id", "test", 5, 60000);
            bool stateChangedEventFired = false;
            cb.StateChanged += (sender, e) =>
            {
                if (cb.State == CircuitBreakerState.Closed)
                {
                    stateChangedEventFired = true;
                }
            };

            cb.Trip();

            Assert.Equal(CircuitBreakerState.Open, cb.State);

            cb.Reset();

            Assert.Equal(CircuitBreakerState.Closed, cb.State);
            Assert.True(stateChangedEventFired, "StateChanged event should be fired on reset");

            stateChangedEventFired = false;

            // Reset an already closed circuit
            cb.Reset();

            Assert.False(stateChangedEventFired, "StateChanged event should be only be fired when state changes");
        }

        [Fact]
        public void CanRaiseServiceLevelChangedEvent()
        {
            CircuitBreaker cb = new CircuitBreaker("test_id", "test", 5, 60000);
            bool serviceLevelChangedEventFired = false;
            cb.ServiceLevelChanged += (sender, e) => { serviceLevelChangedEventFired = true; };

            try
            {
                cb.Execute(new ThreadStart(FailedOperation));
            }
            catch (OperationFailedException) { }

            Assert.True(serviceLevelChangedEventFired, "ServiceLevelChanged event should be fired on failure");
        }

        [Fact]
        public void CanThrowInvokerException()
        {
            CircuitBreaker cb = new CircuitBreaker("test_id", "test", 5, 60000);
            Exception verifyException = null;
            try
            {
                // Cause the DynamicInvoke method to throw an exception
                cb.Execute(null);
            }
            catch (Exception ex)
            {
                verifyException = ex;
            }

            Assert.NotNull(verifyException);
            Assert.IsType(typeof(NullReferenceException), verifyException);
            Assert.Null(verifyException.InnerException);
            Assert.Equal(100, cb.ServiceLevel);
        }
    }
}
