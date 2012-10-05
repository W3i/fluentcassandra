using FluentCassandra.Connections;
using Xunit;
using System;

namespace FluentCassandra.Connections.Tests
{
    
    
    ///// <summary>
    /////This is a test class for ServerStateChangedEventArgsTest and is intended
    /////to contain all ServerStateChangedEventArgsTest Unit Tests
    /////</summary>
    //[TestClass()]
    //public class ServerStateChangedEventArgsTest
    //{


    //    private TestContext testContextInstance;

    //    /// <summary>
    //    ///Gets or sets the test context which provides
    //    ///information about and functionality for the current test run.
    //    ///</summary>
    //    public TestContext TestContext
    //    {
    //        get
    //        {
    //            return testContextInstance;
    //        }
    //        set
    //        {
    //            testContextInstance = value;
    //        }
    //    }

    //    #region Additional test attributes
    //    // 
    //    //You can use the following additional attributes as you write your tests:
    //    //
    //    //Use ClassInitialize to run code before running the first test in the class
    //    //[ClassInitialize()]
    //    //public static void MyClassInitialize(TestContext testContext)
    //    //{
    //    //}
    //    //
    //    //Use ClassCleanup to run code after all tests in a class have run
    //    //[ClassCleanup()]
    //    //public static void MyClassCleanup()
    //    //{
    //    //}
    //    //
    //    //Use TestInitialize to run code before running each test
    //    //[TestInitialize()]
    //    //public void MyTestInitialize()
    //    //{
    //    //}
    //    //
    //    //Use TestCleanup to run code after each test has run
    //    //[TestCleanup()]
    //    //public void MyTestCleanup()
    //    //{
    //    //}
    //    //
    //    #endregion


    //    /// <summary>
    //    ///A test for ServerStateChangedEventArgs Constructor
    //    ///</summary>
    //    [Fact]
    //    public void ServerStateChangedEventArgsConstructorTest()
    //    {
    //        string serverId = "test server id";
    //        string host = "test host";
    //        ServerState newState = ServerState.Blacklisted;
    //        string messageData = "test data";
    //        ServerStateChangedEventArgs target = new ServerStateChangedEventArgs(serverId, host, newState, messageData);

    //        Assert.NotNull(target);
    //        Assert.Equal(serverId, target.ServerId, "Server");
    //        Assert.Equal(host, target.Host, "Host");
    //        Assert.Equal(newState, target.NewState, "State");
    //        Assert.Equal(messageData, target.Message, "Message");
    //    }

    //    /// <summary>
    //    ///A test for ServerStateChangedEventArgs Constructor
    //    ///</summary>
    //    [Fact]
    //    [ExpectedArgumentNullException("host")]
    //    public void ServerStateChangedEventArgsConstructorNullHostTest()
    //    {
    //        string serverId = "test server id";
    //        string host = null;
    //        ServerState newState = ServerState.Blacklisted;
    //        string messageData = "test data";
    //        ServerStateChangedEventArgs target = new ServerStateChangedEventArgs(serverId, host, newState, messageData);
    //    }

    //    /// <summary>
    //    ///A test for ServerStateChangedEventArgs Constructor
    //    ///</summary>
    //    [Fact]
    //    [ExpectedArgumentNullException("serverId")]
    //    public void ServerStateChangedEventArgsConstructorNullServerIdTest()
    //    {
    //        string serverId = null;
    //        string host = "test host";
    //        ServerState newState = ServerState.Blacklisted;
    //        string messageData = "test data";
    //        ServerStateChangedEventArgs target = new ServerStateChangedEventArgs(serverId, host, newState, messageData);
    //    }

    //    /// <summary>
    //    ///A test for ServerStateChangedEventArgs Constructor
    //    ///</summary>
    //    [Fact]
    //    [ExpectedArgumentNullException("host")]
    //    public void ServerStateChangedEventArgsConstructorEmptyHostTest()
    //    {
    //        string serverId = "test server id";
    //        string host = string.Empty;
    //        ServerState newState = ServerState.Blacklisted;
    //        string messageData = "test data";
    //        ServerStateChangedEventArgs target = new ServerStateChangedEventArgs(serverId, host, newState, messageData);
    //    }

    //    /// <summary>
    //    ///A test for ServerStateChangedEventArgs Constructor
    //    ///</summary>
    //    [Fact]
    //    [ExpectedArgumentNullException("serverId")]
    //    public void ServerStateChangedEventArgsConstructorEmptyServerIdTest()
    //    {
    //        string serverId = string.Empty;
    //        string host = "test host";
    //        ServerState newState = ServerState.Blacklisted;
    //        string messageData = "test data";
    //        ServerStateChangedEventArgs target = new ServerStateChangedEventArgs(serverId, host, newState, messageData);
    //    }

    //    /// <summary>
    //    ///A test for ServerStateChangedEventArgs Constructor
    //    ///</summary>
    //    [Fact]
    //    [ExpectedArgumentOutOfRangeException("newState", StringTheMessageMustContain = "Given state '100' is not one of the defined values.")]
    //    public void ServerStateChangedEventArgsConstructorStateOutOfRangeTest()
    //    {
    //        string serverId = "test server id";
    //        string host = "test host";
    //        ServerState newState = (ServerState)100;
    //        string messageData = "test data";
    //        ServerStateChangedEventArgs target = new ServerStateChangedEventArgs(serverId, host, newState, messageData);
    //    }
  //  }
}
