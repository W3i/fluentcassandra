using FluentCassandra.CircuitBreaker;
using Xunit;
using System;

namespace FluentCassandra.CircuitBreaker.Tests
{
    
    
    /// <summary>
    ///This is a test class for CircuitStateChangedEventArgsTest and is intended
    ///to contain all CircuitStateChangedEventArgsTest Unit Tests
    ///</summary>
    public class CircuitStateChangedEventArgsTest
    {
        /// <summary>
        ///A test for CircuitStateChangedEventArgs Constructor
        ///</summary>
        [Fact]
        public void CircuitStateChangedEventArgsConstructorTest()
        {
            string serverNodeId = "test server id";
            string host = "test host";
            CircuitBreakerState newState = CircuitBreakerState.Closed;
            string messageData = "test data";
            CircuitStateChangedEventArgs target = new CircuitStateChangedEventArgs(serverNodeId, host, newState, messageData);

            Assert.NotNull(target);
            Assert.Equal(serverNodeId, target.NodeId);
            Assert.Equal(host, target.Host);
            Assert.Equal(newState, target.NewState);
            Assert.Equal(messageData, target.Message);
        }

        /// <summary>
        ///A test for CircuitStateChangedEventArgs Constructor
        ///</summary>
        [Fact]
        public void CircuitStateChangedEventArgsConstructorNullHostTest()
        {
            var argNE = Assert.Throws<ArgumentNullException>(delegate()
            {
                string serverNodeId = "test server id";
                string host = null;
                CircuitBreakerState newState = CircuitBreakerState.Closed;
                string messageData = "test data";
                CircuitStateChangedEventArgs target = new CircuitStateChangedEventArgs(serverNodeId, host, newState, messageData);
            });

            Assert.Equal("host", argNE.ParamName);
        }

        /// <summary>
        ///A test for CircuitStateChangedEventArgs Constructor
        ///</summary>
        [Fact]
        public void CircuitStateChangedEventArgsConstructorNullServerIdTest()
        {
            var argNE = Assert.Throws<ArgumentNullException>(delegate()
            {
                string serverNodeId = null;
                string host = "test host";
                CircuitBreakerState newState = CircuitBreakerState.Closed;
                string messageData = "test data";
                CircuitStateChangedEventArgs target = new CircuitStateChangedEventArgs(serverNodeId, host, newState, messageData);
            });

            Assert.Equal("nodeId", argNE.ParamName);
        }

        /// <summary>
        ///A test for CircuitStateChangedEventArgs Constructor
        ///</summary>
        [Fact]
        public void CircuitStateChangedEventArgsConstructorEmptyHostTest()
        {
            var argNE = Assert.Throws<ArgumentNullException>(delegate()
            {
                string serverNodeId = "test server id";
                string host = string.Empty;
                CircuitBreakerState newState = CircuitBreakerState.Closed;
                string messageData = "test data";
                CircuitStateChangedEventArgs target = new CircuitStateChangedEventArgs(serverNodeId, host, newState, messageData);
             });

            Assert.Equal("host", argNE.ParamName);
        }

        /// <summary>
        ///A test for CircuitStateChangedEventArgs Constructor
        ///</summary>
        [Fact]
        public void CircuitStateChangedEventArgsConstructorEmptyServerIdTest()
        {
            var argNE = Assert.Throws<ArgumentNullException>(delegate()
            {
                string serverNodeId = string.Empty;
                string host = "test host";
                CircuitBreakerState newState = CircuitBreakerState.Closed;
                string messageData = "test data";
                CircuitStateChangedEventArgs target = new CircuitStateChangedEventArgs(serverNodeId, host, newState, messageData);
            });
            Assert.Equal("nodeId", argNE.ParamName);
        }

        /// <summary>
        ///A test for CircuitStateChangedEventArgs Constructor
        ///</summary>
        [Fact]
        public void CircuitStateChangedEventArgsConstructorStateOutOfRangeTest()
        {
            var thrown = Assert.Throws<ArgumentOutOfRangeException>(delegate()
            {
                string serverNodeId = "test server id";
                string host = "test host";
                CircuitBreakerState newState = (CircuitBreakerState)100;
                string messageData = "test data";
                CircuitStateChangedEventArgs target = new CircuitStateChangedEventArgs(serverNodeId, host, newState, messageData);
            });

            Assert.True(thrown.Message.Contains("Given state '100' is not one of the defined values."));
            Assert.Equal("newState", thrown.ParamName);
        }
    }
}
