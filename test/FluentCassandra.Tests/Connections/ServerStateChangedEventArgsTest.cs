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
            Server server = new Server("test host", 1111);
            ServerState newState = ServerState.Blacklisted;
            string messageData = "test data";
            ServerStateChangedEventArgs target = new ServerStateChangedEventArgs(server, newState, messageData);

            Assert.NotNull(target);
            Assert.Equal(server, target.Server);
            Assert.Equal(newState, target.NewState);
            Assert.Equal(messageData, target.Message);
        }

        /// <summary>
        ///A test for ServerStateChangedEventArgs Constructor
        ///</summary>
        [Fact]
        public void ServerStateChangedEventArgsConstructorNullServerTest()
        {
            var thrown = Assert.Throws<ArgumentNullException>(delegate()
            {
                ServerState newState = ServerState.Blacklisted;
                string messageData = "test data";
                ServerStateChangedEventArgs target = new ServerStateChangedEventArgs(null, newState, messageData);
            });

            Assert.Equal("server", thrown.ParamName);
        }

        /// <summary>
        ///A test for ServerStateChangedEventArgs Constructor
        ///</summary>
        [Fact]
        public void ServerStateChangedEventArgsConstructorStateOutOfRangeTest()
        {
            var thrown = Assert.Throws<ArgumentOutOfRangeException>(delegate()
            {
                Server server = new Server();
                ServerState newState = (ServerState)100;
                string messageData = "test data";
                ServerStateChangedEventArgs target = new ServerStateChangedEventArgs(server, newState, messageData);
            });

            Assert.Equal("newState", thrown.ParamName);
            Assert.True(thrown.Message.Contains("Given state '100' is not one of the defined values."));
        }
    }
}
