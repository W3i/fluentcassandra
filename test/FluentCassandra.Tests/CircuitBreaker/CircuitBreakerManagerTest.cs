using FluentCassandra.CircuitBreaker;
using Xunit;
using System;

namespace FluentCassandra.CircuitBreakerTests
{


    /// <summary>
    ///This is a test class for CircuitBreakerManagerTest and is intended
    ///to contain all CircuitBreakerManagerTest Unit Tests
    ///</summary>
    public class CircuitBreakerManagerTest
    {
        /// <summary>
        ///A test for CircuitBreakerManager Constructor
        ///</summary>
        [Fact]
        public void CircuitBreakerManagerConstructorNullArgsTest()
        {
            CircuitBreakerManager target = new CircuitBreakerManager();
            Assert.NotNull(target);
        }

        /// <summary>
        ///A test for AddCircuitBreakerByKey
        ///</summary>
        [Fact]
        public void AddCircuitBreakerByKeyTest()
        {
            EventHandler<CircuitStateChangedEventArgs> onStateChanged = new EventHandler<CircuitStateChangedEventArgs>((obj, evt) => { });
            EventHandler onServiceLevelChanged = new EventHandler((obj, args) => { });
            CircuitBreakerManager defaultTarget = new CircuitBreakerManager(onStateChanged, onServiceLevelChanged);
            string key = "test 1";
            string nodeName = "test";
            uint failureThresholdCount = 5;
            uint breakerResetIntervalMs = 1000;
            bool expected = true;
            bool actual;
            actual = defaultTarget.AddCircuitBreakerByKey(key, nodeName, failureThresholdCount, breakerResetIntervalMs);
            Assert.Equal(expected, actual);

            // Try to add the same one again.
            expected = false;
            actual = defaultTarget.AddCircuitBreakerByKey(key, nodeName, failureThresholdCount, breakerResetIntervalMs);
            Assert.Equal(expected, actual);
        }

        /// <summary>
        ///A test for ForwardErrorOccurred/SuccessToBreaker
        ///</summary>
        [Fact]
        public void CantForwardToNonExistentBreakerTest()
        {
            EventHandler<CircuitStateChangedEventArgs> onStateChanged = new EventHandler<CircuitStateChangedEventArgs>((obj, evt) => { });
            EventHandler onServiceLevelChanged = new EventHandler((obj, args) => { });
            CircuitBreakerManager defaultTarget = new CircuitBreakerManager(onStateChanged, onServiceLevelChanged);
            bool success = defaultTarget.ForwardErrorOccurredToBreaker("bogus");
            Assert.False(success, "Should not be able to forward error to non-existent breaker.");

            success = defaultTarget.ForwardOperationSuccessToBreaker("bogus");
            Assert.False(success, "Should not be able to forward success to non-existent breaker.");
        }

        /// <summary>
        ///A test for ForwardErrorOccurredToBreaker
        ///</summary>
        [Fact]
        public void ForwardErrorOccurredToBreakerTest()
        {
            bool eventFired = false;
            EventHandler<CircuitStateChangedEventArgs> onStateChanged = new EventHandler<CircuitStateChangedEventArgs>((obj, args) => { eventFired = true; });
            EventHandler onServiceLevelChanged = null;
            CircuitBreakerManager target = new CircuitBreakerManager(onStateChanged, onServiceLevelChanged);
            string key = "test 1";
            string nodeName = "test";
            uint failureThresholdCount = 2;
            uint breakerResetIntervalMs = 10000;
            target.AddCircuitBreakerByKey(key, nodeName, failureThresholdCount, breakerResetIntervalMs);
            bool expectedSuccessfulForward = false;
            for (int i = 0; i <= failureThresholdCount; i++)
            {
                expectedSuccessfulForward = target.ForwardErrorOccurredToBreaker(key);
                Assert.True(expectedSuccessfulForward, "Should have been able to forward the error.");
                if (i < failureThresholdCount)
                {
                    Assert.False(eventFired, string.Format("Event prematurely fired on iteration: {0}", i));
                }
            }

            Assert.True(eventFired, "Changed state event should have fired.");
        }

        /// <summary>
        ///A test for ForwardOperationSuccessToBreaker
        ///</summary>
        [Fact]
        public void ForwardOperationSuccessToBreakerTest()
        {
            bool eventFired = false;
            EventHandler<CircuitStateChangedEventArgs> onStateChanged = new EventHandler<CircuitStateChangedEventArgs>((obj, args) => { if (args.NewState == CircuitBreakerState.Closed) { eventFired = true; } });
            EventHandler onServiceLevelChanged = null;
            CircuitBreakerManager target = new CircuitBreakerManager(onStateChanged, onServiceLevelChanged);
            string key = "test 1";
            string nodeName = "test";
            uint failureThresholdCount = 2;
            uint breakerResetIntervalMs = 1000;
            target.AddCircuitBreakerByKey(key, nodeName, failureThresholdCount, breakerResetIntervalMs);

            CircuitBreaker.CircuitBreaker cb = target.GetCircuitBreakerByKey(key);
            cb.Trip(); // manually open
            Assert.Equal(CircuitBreakerState.Open, cb.State);
            System.Threading.Thread.Sleep(1500);
            Assert.Equal(CircuitBreakerState.HalfOpen, cb.State);
            Assert.False(eventFired); // should not have closed yet.
            bool actual = target.ForwardOperationSuccessToBreaker(key);
            Assert.True(actual, "Should have been able to forward the success message.");
            Assert.True(eventFired, "Success operation should have closed the half-open circuit.");
        }

        /// <summary>
        ///A test for GetCircuitBreakerByKey
        ///</summary>
        [Fact]
        public void GetCircuitBreakerByEmptyKeyTest()
        {
            var thrown = Assert.Throws<ArgumentNullException>(delegate()
            {
                EventHandler<CircuitStateChangedEventArgs> onStateChanged = new EventHandler<CircuitStateChangedEventArgs>((obj, evt) => { });
                EventHandler onServiceLevelChanged = new EventHandler((obj, args) => { });
                CircuitBreakerManager defaultTarget = new CircuitBreakerManager(onStateChanged, onServiceLevelChanged);
                defaultTarget.GetCircuitBreakerByKey(string.Empty);
            });

            Assert.Equal("key", thrown.ParamName);
        }

        /// <summary>
        ///A test for GetCircuitBreakerByKey
        ///</summary>
        [Fact]
        public void GetCircuitBreakerByNullKeyTest()
        {
            var thrown = Assert.Throws<ArgumentNullException>(delegate()
            {
                EventHandler<CircuitStateChangedEventArgs> onStateChanged = new EventHandler<CircuitStateChangedEventArgs>((obj, evt) => { });
                EventHandler onServiceLevelChanged = new EventHandler((obj, args) => { });
                CircuitBreakerManager defaultTarget = new CircuitBreakerManager(onStateChanged, onServiceLevelChanged);
                defaultTarget.GetCircuitBreakerByKey(null);
            });

            Assert.Equal("key", thrown.ParamName);
        }

        /// <summary>
        ///A test for RemoveCircuitBreakerByKey
        ///</summary>
        [Fact]
        public void RemoveCircuitBreakerByEmptyKeyTest()
        {
            var thrown = Assert.Throws<ArgumentNullException>(delegate()
            {
                EventHandler<CircuitStateChangedEventArgs> onStateChanged = new EventHandler<CircuitStateChangedEventArgs>((obj, evt) => { });
                EventHandler onServiceLevelChanged = new EventHandler((obj, args) => { });
                CircuitBreakerManager defaultTarget = new CircuitBreakerManager(onStateChanged, onServiceLevelChanged);
                CircuitBreaker.CircuitBreaker remove;
                defaultTarget.RemoveCircuitBreakerByKey(string.Empty, out remove);
            });

            Assert.Equal("key", thrown.ParamName);
        }

        /// <summary>
        ///A test for RemoveCircuitBreakerByKey
        ///</summary>
        [Fact]
        public void RemoveCircuitBreakerByNullKeyTest()
        {
            var thrown = Assert.Throws<ArgumentNullException>(delegate()
            {
                EventHandler<CircuitStateChangedEventArgs> onStateChanged = new EventHandler<CircuitStateChangedEventArgs>((obj, evt) => { });
                EventHandler onServiceLevelChanged = new EventHandler((obj, args) => { });
                CircuitBreakerManager defaultTarget = new CircuitBreakerManager(onStateChanged, onServiceLevelChanged);
                CircuitBreaker.CircuitBreaker remove;
                defaultTarget.RemoveCircuitBreakerByKey(null, out remove);
            });

            Assert.Equal("key", thrown.ParamName);
        }

        /// <summary>
        ///A test for GetCircuitBreakerByKey
        ///</summary>
        [Fact]
        public void GetCircuitBreakerByKeyTest()
        {
            EventHandler<CircuitStateChangedEventArgs> onStateChanged = new EventHandler<CircuitStateChangedEventArgs>((obj, evt) => { });
            EventHandler onServiceLevelChanged = new EventHandler((obj, args) => { });
            CircuitBreakerManager defaultTarget = new CircuitBreakerManager(onStateChanged, onServiceLevelChanged);
            string key = "test 1";
            string nodeName = "test";
            uint failureThresholdCount = 2;
            uint breakerResetIntervalMs = 1000;
            defaultTarget.AddCircuitBreakerByKey(key, nodeName, failureThresholdCount, breakerResetIntervalMs);
            CircuitBreaker.CircuitBreaker actual;
            actual = defaultTarget.GetCircuitBreakerByKey(key);
            Assert.NotNull(actual);
            Assert.Equal(key, actual.NodeId);
            Assert.Equal(nodeName, actual.Node);
            Assert.Equal(failureThresholdCount, actual.Threshold);
            Assert.Equal(breakerResetIntervalMs, actual.Timeout);

            actual = defaultTarget.GetCircuitBreakerByKey("bogus key");
            Assert.Null(actual);
        }

        /// <summary>
        ///A test for RemoveCircuitBreakerByKey
        ///</summary>
        [Fact]
        public void RemoveCircuitBreakerByKeyTest()
        {
            EventHandler<CircuitStateChangedEventArgs> onStateChanged = new EventHandler<CircuitStateChangedEventArgs>((obj, evt) => { });
            EventHandler onServiceLevelChanged = new EventHandler((obj, args) => { });
            CircuitBreakerManager defaultTarget = new CircuitBreakerManager(onStateChanged, onServiceLevelChanged);
            string key = "test 1";
            string nodeName = "test";
            uint failureThresholdCount = 2;
            uint breakerResetIntervalMs = 1000;
            defaultTarget.AddCircuitBreakerByKey(key, nodeName, failureThresholdCount, breakerResetIntervalMs);
            CircuitBreaker.CircuitBreaker addedCB;
            CircuitBreaker.CircuitBreaker removedBreaker = null;
            addedCB = defaultTarget.GetCircuitBreakerByKey(key);
            Assert.NotNull(addedCB);

            defaultTarget.RemoveCircuitBreakerByKey(key, out removedBreaker);
            Assert.Same(addedCB, removedBreaker);

            // Try to get it again.
            Assert.Null(defaultTarget.GetCircuitBreakerByKey(key));

            // Try to remove it again.
            defaultTarget.RemoveCircuitBreakerByKey(key, out removedBreaker);
            Assert.Null(removedBreaker);
        }
    }
}
