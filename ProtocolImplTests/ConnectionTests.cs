using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading;
using TestUtilities;
using TestUtilities.Protocol;
using Protocol.Transport;

namespace Protocol.Impl.Tests {
  class TestConnection : Connection {
    public TestConnection(IConnectionTransport transport, int authInterval = 1000, int connInterval = 1000) : base(transport, null) {
      AuthenticationRetryInterval = TimeSpan.FromMilliseconds(authInterval);
      ConnectionRetryInterval = TimeSpan.FromMilliseconds(connInterval);
    }
  }

  [TestClass]
  public class ConnectionTests {
    [TestMethod]
    public void Connect_shouldTriggerOnEstablished_whenTheConnectionIsEstablished() {
      MockedTransport transport = new MockedTransport();
      Connection connection = new TestConnection(transport);

      bool httpRequestSent = false;
      transport.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
        httpRequestSent = true;
        string json = TestData.Serialize(TestData.UserData);
        return request.CreateResponse(HttpStatusCode.OK, json);
      };

      transport.OnConnectionAttempt += () => {
        return true;
      };

      AutoResetEvent evt = new AutoResetEvent(false);
      connection.OnEstablished += delegate {
        evt.Set();
      };

      connection.Connect("email", "password");

      int timeout = 1000;
      bool signaled = evt.WaitOne(timeout);

      Assert.IsTrue(signaled, "Connecting should trigger a connection established event");
      Assert.IsTrue(httpRequestSent, "Connecting should send an http authentication request to the server");
      transport.Verify("Connect", 1);
    }

    [TestMethod]
    public void Connect_shouldTriggerValidationFailure_whenAuthenticationFails() {
      MockedTransport transport = new MockedTransport();
      Connection connection = new TestConnection(transport);

      transport.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
        return request.CreateResponse(HttpStatusCode.BadRequest, TestData.AuthenticationErrorReason);
      };

      AutoResetEvent evt = new AutoResetEvent(false);
      string error = null;
      connection.OnValidationFailure += (string reason) => {
        error = reason;
        evt.Set();
      };

      connection.Connect("email", "password");

      int timeout = 1000;
      bool signaled = evt.WaitOne(timeout);
      Assert.IsTrue(signaled, "Connecting should trigger a validation failure event when authentication fails");
      Assert.AreEqual(TestData.AuthenticationErrorReason, error, false, "Validation failure reason should be the message received from the server");
    }

    [TestMethod]
    public void Connect_shouldRetryAuthentication_whenAuthenticationRequestFails() {
      MockedTransport transport = new MockedTransport();
      Connection connection = new TestConnection(transport, authInterval: 50);

      bool failAuth = true;
      transport.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
        if (failAuth) {
          failAuth = false;
          return null;
        } else {
          return request.CreateResponse(HttpStatusCode.OK, TestData.Serialize(TestData.UserData));
        }
      };

      transport.OnConnectionAttempt += () => {
        return true;
      };

      bool failureCalled = false;
      connection.OnValidationFailure += (string reason) => {
        failureCalled = true;
      };

      AutoResetEvent evt = new AutoResetEvent(false);
      connection.OnEstablished += delegate {
        evt.Set();
      };

      connection.Connect("email", "password");

      int timeout = 1000;
      bool signaled = evt.WaitOne(timeout);
      Assert.IsTrue(signaled, "Connecting retry authentication until success when authentication request fails");
      Assert.IsFalse(failureCalled, "No validation failure event is raised when the authentication request fails");
    }

    [TestMethod]
    public void Connect_shouldRetryConnecting_whenSocketConnectionFails() {
      MockedTransport transport = new MockedTransport();
      Connection connection = new TestConnection(transport, connInterval: 50);

      transport.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
        return request.CreateResponse(HttpStatusCode.OK, TestData.Serialize(TestData.UserData));
      };

      bool failConection = true;
      transport.OnConnectionAttempt += () => {
        failConection = !failConection;
        return failConection;
      };

      AutoResetEvent evt = new AutoResetEvent(false);
      connection.OnEstablished += delegate {
        evt.Set();
      };

      connection.Connect("email", "password");

      int timeout = 1000;
      bool signaled = evt.WaitOne(timeout);
      Assert.IsTrue(signaled, "Connecting retry until success when the websocket connection fails");
    }

    private void connected(MockedTransport transport,
                           Connection connection,
                           Action test,
                           int timeout = 1000)
    {
      MockedTransport.HttpRequestSentHandler httpRequestCallback = (MockedHttpRequest request, string body) => {
        return request.CreateResponse(HttpStatusCode.OK, TestData.Serialize(TestData.UserData));
      };

      MockedTransport.ConnectionAttemptHandler connectionAttemptCallback = () => {
        return true;
      };

      transport.OnHttpRequestSent += httpRequestCallback;
      transport.OnConnectionAttempt += connectionAttemptCallback;

      AutoResetEvent testDoneEvent = new AutoResetEvent(false);
      connection.OnEstablished += delegate {
        test?.Invoke();
        testDoneEvent.Set();
      };

      connection.Connect("test_email", "test_password");

      bool signaled = testDoneEvent.WaitOne(timeout);
      Assert.IsTrue(signaled, "Test should execute in less than {0}ms", timeout);
    }
  }
}