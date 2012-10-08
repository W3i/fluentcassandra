using FluentCassandra.Connections;
using Xunit;
using System;

namespace FluentCassandra.Connections.Tests
{
    /// <summary>
    ///This is a test class for ServerStateChangedEventArgsTest and is intended
    ///to contain all ServerStateChangedEventArgsTest Unit Tests
    ///</summary>
    public class ServerStateChangedEventArgsTest
    {
        /// <summary>
        ///A test for ServerStateChangedEventArgs Constructor
        ///</summary>
        [Fact]
        public void ServerStateChangedEventArgsConstructorTest()
        {
            string serverId = "test server id";
            string host = "test host";
            ServerState newState = ServerState.Blacklisted;
            string messageData = "test data";
            ServerStateChangedEventArgs target = new ServerStateChangedEventArgs(serverId, host, newState, messageData);

            Assert.NotNull(target);
            Assert.Equal(serverId, target.ServerId);
            Assert.Equal(host, target.Host);
            Assert.Equal(newState, target.NewState);
            Assert.Equal(messageData, target.Message);
        }

        /// <summary>
        ///A test for ServerStateChangedEventArgs Constructor
        ///</summary>
        [Fact]
        public void ServerStateChangedEventArgsConstructorNullHostTest()
        {
            var thrown = Assert.Throws<ArgumentNullException>(delegate()
            {
                string serverId = "test server id";
                string host = null;
                ServerState newState = ServerState.Blacklisted;
                string messageData = "test data";
                ServerStateChangedEventArgs target = new ServerStateChangedEventArgs(serverId, host, newState, messageData);
            });

            Assert.Equal("host", thrown.ParamName);
        }

        /// <summary>
        ///A test for ServerStateChangedEventArgs Constructor
        ///</summary>
        [Fact]
        public void ServerStateChangedEventArgsConstructorNullServerIdTest()
        {
            var thrown = Assert.Throws<ArgumentNullException>(delegate()
            {
                string serverId = null;
                string host = "test host";
                ServerState newState = ServerState.Blacklisted;
                string messageData = "test data";
                ServerStateChangedEventArgs target = new ServerStateChangedEventArgs(serverId, host, newState, messageData);
            });

            Assert.Equal("serverId", thrown.ParamName);
        }

        /// <summary>
        ///A test for ServerStateChangedEventArgs Constructor
        ///</summary>
        [Fact]
        public void ServerStateChangedEventArgsConstructorEmptyHostTest()
        {
            var thrown = Assert.Throws<ArgumentNullException>(delegate()
            {
                string serverId = "test server id";
                string host = string.Empty;
                ServerState newState = ServerState.Blacklisted;
                string messageData = "test data";
                ServerStateChangedEventArgs target = new ServerStateChangedEventArgs(serverId, host, newState, messageData);
            });

            Assert.Equal("host", thrown.ParamName);
        }

        /// <summary>
        ///A test for ServerStateChangedEventArgs Constructor
        ///</summary>
        [Fact]
        public void ServerStateChangedEventArgsConstructorEmptyServerIdTest()
        {
            var thrown = Assert.Throws<ArgumentNullException>(delegate()
            {
                string serverId = string.Empty;
                string host = "test host";
                ServerState newState = ServerState.Blacklisted;
                string messageData = "test data";
                ServerStateChangedEventArgs target = new ServerStateChangedEventArgs(serverId, host, newState, messageData);
            });

            Assert.Equal("serverId", thrown.ParamName);
        }

        /// <summary>
        ///A test for ServerStateChangedEventArgs Constructor
        ///</summary>
        [Fact]
        public void ServerStateChangedEventArgsConstructorStateOutOfRangeTest()
        {
            var thrown = Assert.Throws<ArgumentOutOfRangeException>(delegate()
            {
                string serverId = "test server id";
                string host = "test host";
                ServerState newState = (ServerState)100;
                string messageData = "test data";
                ServerStateChangedEventArgs target = new ServerStateChangedEventArgs(serverId, host, newState, messageData);
            });

            Assert.Equal("newState", thrown.ParamName);
            Assert.True(thrown.Message.Contains("Given state '100' is not one of the defined values."));
        }
    }
}
