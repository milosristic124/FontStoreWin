using Microsoft.VisualStudio.TestTools.UnitTesting;
using Protocol.Transport;
using Storage;
using System;
using System.Net;
using System.Threading;
using TestUtilities;
using TestUtilities.Protocol;
using TestUtilities.Storage;

namespace Protocol.Impl.Tests {
  class TestConnection : Connection {
    public TestConnection(IConnectionTransport transport,
                          IFontStorage storage = null,
                          int authInterval = 1000,
                          int connInterval = 1000) : base(transport, storage) {
      AuthenticationRetryInterval = TimeSpan.FromMilliseconds(authInterval);
      ConnectionRetryInterval = TimeSpan.FromMilliseconds(connInterval);
    }
  }

  [TestClass]
  public class ConnectionTests {
    [TestMethod]
    [TestCategory("Protocol.Authentication")]
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
      Assert.IsNotNull(connection.UserData, "Connecting should populate the connection UserData property");
    }

    [TestMethod]
    [TestCategory("Protocol.Authentication")]
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
    [TestCategory("Protocol.Authentication")]
    public void Connect_shouldRetryAuthentication_whenAuthenticationRequestFails() {
      MockedTransport transport = new MockedTransport();
      Connection connection = new TestConnection(transport, authInterval: 50);

      bool failAuth = true;
      transport.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
        if (failAuth) {
          failAuth = false;
          return null;
        }
        else {
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
    [TestCategory("Protocol.Authentication")]
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

    [TestMethod]
    [TestCategory("Protocol.Update")]
    public void UpdateCatalog_shouldUpdateTheFontCatalog() {
      MockedTransport transport = new MockedTransport();
      MockedStorage storage = new MockedStorage();
      Connection connection = new TestConnection(transport, storage);

      connected(transport, connection, delegate {
        bool catalogUpdateRequested = false;

        AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        transport.OnMessageSent += (MockedBroadcastResponse resp, string evt, dynamic payload) => {
          switch (evt) {
            case "catalog.update:request":
              catalogUpdateRequested = true;
              autoResetEvent.Set();
              break;

            default: break;
          }
        };

        connection.UpdateCatalog();

        autoResetEvent.WaitOne(); // wait for a catalog update request to 'send' the catalog update
        transport.SimulateMessage("catalog", "font:description", TestData.Font1_Description);
        transport.SimulateMessage("catalog", "font:description", TestData.Font2_Description);
        transport.SimulateMessage("catalog", "font:deletion", TestData.Font1_Id);

        Assert.IsTrue(catalogUpdateRequested, "Updating the catalog should request an update of the catalog fonts data");
        Assert.AreEqual(1, storage.FamilyCollection.Families.Count, "Font description messages should add fonts to the font storage");
        Assert.IsNull(storage.FindFont(TestData.Font1_Description.UID), "Font deletion message should remove fonts from the font storage");
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Update")]
    public void UpdateCatalog_shouldUpdateTheFontsStatus() {
      MockedTransport transport = new MockedTransport();
      MockedStorage storage = new MockedStorage();
      Connection connection = new TestConnection(transport, storage);

      connected(transport, connection, delegate {
        bool fontUpdateRequested = false;

        AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        transport.OnMessageSent += (MockedBroadcastResponse resp, string evt, dynamic payload) => {
          if (evt == "catalog.update:request") {
            autoResetEvent.Set();
          }
          else if (evt == UserTopicEvent(connection, "update:request")) {
            fontUpdateRequested = true;
            autoResetEvent.Set();
          }
        };

        connection.UpdateCatalog();

        autoResetEvent.WaitOne(); // wait for the catalog update request
        transport.SimulateMessage("catalog", "font:description", TestData.Font1_Description);
        transport.SimulateMessage("catalog", "update:complete");
        autoResetEvent.WaitOne(); // wait for the fonts update request
        transport.SimulateMessage(UserTopicEvent(connection), "font:activation", TestData.Font1_Id);
        transport.SimulateMessage(UserTopicEvent(connection), "update:complete");

        Assert.IsTrue(fontUpdateRequested, "Updating the catalog should request an update of the catalog fonts status");
        bool? isActivated = storage.FindFont(TestData.Font1_Description.UID)?.Activated;
        Assert.IsTrue(isActivated.HasValue && isActivated.Value, "Font activation message should activate fonts in the font storage");
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Update")]
    public void UpdateCatalog_shouldTriggerEvent_whenUpdateIsFinished() {
      MockedTransport transport = new MockedTransport();
      MockedStorage storage = new MockedStorage();
      Connection connection = new TestConnection(transport, storage);

      connected(transport, connection, delegate {
        AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        transport.OnMessageSent += (MockedBroadcastResponse resp, string evt, dynamic payload) => {
          if (evt == "catalog.update:request") {
            autoResetEvent.Set();
          }
          else if (evt == UserTopicEvent(connection, "update:request")) {
            autoResetEvent.Set();
          }
        };

        transport.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
          if (request.Endpoint.Contains("downloads")) {
            return request.CreateResponse(HttpStatusCode.OK, "BODYBODYBODY");
          }
          return null;
        };

        connection.OnCatalogUpdateFinished += delegate {
          autoResetEvent.Set();
        };

        connection.UpdateCatalog();

        autoResetEvent.WaitOne(); // wait for the catalog update request
        transport.SimulateMessage("catalog", "font:description", TestData.Font1_Description);
        transport.SimulateMessage("catalog", "update:complete");
        autoResetEvent.WaitOne(); // wait for the fonts update request
        transport.SimulateMessage(UserTopicEvent(connection), "font:activation", TestData.Font1_Id);
        transport.SimulateMessage(UserTopicEvent(connection), "update:complete");

        int timeout = 500;
        bool eventRaised = autoResetEvent.WaitOne(timeout);

        Assert.IsTrue(eventRaised, "Updating the catalog should trigger an update finished event when terminated");
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Update")]
    public void UpdateCatalog_shouldDownloadNewFonts_andInstallThem() {
      MockedTransport transport = new MockedTransport();
      MockedStorage storage = new MockedStorage();
      Connection connection = new TestConnection(transport, storage);

      connected(transport, connection, delegate {
        AutoResetEvent catalogUpdateRequested = new AutoResetEvent(false);
        AutoResetEvent fontUpdateRequested = new AutoResetEvent(false);
        transport.OnMessageSent += (MockedBroadcastResponse resp, string evt, dynamic payload) => {
          if (evt == "catalog.update:request") {
            catalogUpdateRequested.Set();
          }
          else if (evt == UserTopicEvent(connection, "update:request")) {
            fontUpdateRequested.Set();
          }
        };

        bool downloadRequested = false;
        transport.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
          if (request.Endpoint.Contains("downloads")) {
            downloadRequested = true;
            return request.CreateResponse(HttpStatusCode.OK, "BODYBODYBODY");
          }
          return null;
        };

        AutoResetEvent updateFinished = new AutoResetEvent(false);
        connection.OnCatalogUpdateFinished += delegate {
          updateFinished.Set();
        };

        connection.UpdateCatalog();

        catalogUpdateRequested.WaitOne(); // wait for the catalog update request
        transport.SimulateMessage("catalog", "font:description", TestData.Font1_Description);
        transport.SimulateMessage("catalog", "update:complete");

        fontUpdateRequested.WaitOne(); // wait for the fonts update request
        transport.SimulateMessage(UserTopicEvent(connection), "font:activation", TestData.Font1_Id);
        transport.SimulateMessage(UserTopicEvent(connection), "update:complete");

        updateFinished.WaitOne(); // wait for the update to complete

        Assert.IsTrue(downloadRequested, "Updating the catalog should download fonts");
        storage.Verify("SaveFontFile", 1); // one font was saved
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Running")]
    public void FontActivationMessage_shouldActivateFont() {
      MockedTransport transport = new MockedTransport();
      MockedStorage storage = new MockedStorage();
      Connection connection = new TestConnection(transport, storage);

      updated(transport, connection, delegate {
        transport.SimulateMessage(UserTopicEvent(connection), "font:activation", TestData.Font1_Id);
        storage.Verify("ActivateFont", 1);
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Running")]
    public void FontDeactivationMessage_shouldDeactivateFont() {
      MockedTransport transport = new MockedTransport();
      MockedStorage storage = new MockedStorage();
      Connection connection = new TestConnection(transport, storage);

      updated(transport, connection, delegate {
        transport.SimulateMessage(UserTopicEvent(connection), "font:activation", TestData.Font1_Id);
        transport.SimulateMessage(UserTopicEvent(connection), "font:deactivation", TestData.Font1_Id);
        storage.Verify("DeactivateFont", 1);
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Running")]
    public void FontRemovedMessage_shouldRemoveFontFromCatalog() {
      MockedTransport transport = new MockedTransport();
      MockedStorage storage = new MockedStorage();
      Connection connection = new TestConnection(transport, storage);

      updated(transport, connection, delegate {
        transport.SimulateMessage("catalog", "font:description", TestData.Font2_Description);
        storage.Verify("AddFont", 1);
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Running")]
    public void FontDescriptionMessage_shouldAddFontToTheCatalog() {
      MockedTransport transport = new MockedTransport();
      MockedStorage storage = new MockedStorage();
      Connection connection = new TestConnection(transport, storage);

      updated(transport, connection, delegate {
        transport.SimulateMessage("catalog", "font:deletion", TestData.Font1_Id);
        storage.Verify("RemoveFont", 1);
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Disconnect")]
    public void Disconnect_shouldDisconnectTheTransport() {
      MockedTransport transport = new MockedTransport();
      MockedStorage storage = new MockedStorage();
      Connection connection = new TestConnection(transport, storage);

      connected(transport, connection, delegate {
        AutoResetEvent disconnected = new AutoResetEvent(false);
        connection.OnDisconnected += () => {
          disconnected.Set();
        };

        transport.OnDisconnectAttempt += () => true;

        connection.Disconnect(DisconnectReason.Quit);

        int timeout = 500;
        bool signaled = disconnected.WaitOne(timeout);
        Assert.IsTrue(signaled, "Disconnect should trigger a disconnected event");

        transport.Verify("Disconnect", 1);
      });
    }

    #region supporting DSL
    private string UserTopicEvent(Connection connection, string evt = null) {
      if (evt == null)
        return string.Format("users:{0}", connection.UserData.UID);
      else
        return string.Format("users:{0}.{1}", connection.UserData.UID, evt);
    }

    private void connected(MockedTransport transport,
                           Connection connection,
                           Action test,
                           int timeout = 1000) {
      MockedTransport.HttpRequestSentHandler httpRequestCallback = (MockedHttpRequest request, string body) => {
        if (request.Endpoint.Contains("session")) {
          return request.CreateResponse(HttpStatusCode.OK, TestData.Serialize(TestData.UserData));
        }
        return null;
      };
      MockedTransport.ConnectionAttemptHandler connectionAttemptCallback = () => {
        return true;
      };

      transport.OnHttpRequestSent += httpRequestCallback;
      transport.OnConnectionAttempt += connectionAttemptCallback;

      AutoResetEvent testDoneEvent = new AutoResetEvent(false);
      Exception error = null;
      connection.OnEstablished += delegate {
        try {
          test?.Invoke();
        }
        catch (Exception e) {
          error = e;
        }
        testDoneEvent.Set();
      };

      connection.Connect("test_email", "test_password");

      // DEBUG unit tests
      //testDoneEvent.WaitOne();
      if (!testDoneEvent.WaitOne(timeout)) {
        Assert.Fail("Test should execute in less than {0}ms", timeout);
      }

      if (error != null) {
        throw new Exception("Test failed", error);
      }
    }

    private void updated(MockedTransport transport,
                         Connection connection,
                         Action test,
                         int timeout = 1000) {
      connected(transport, connection, delegate {
        AutoResetEvent catalogUpdateRequest = new AutoResetEvent(false);
        AutoResetEvent fontUpdateRequest = new AutoResetEvent(false);
        AutoResetEvent updateFinished = new AutoResetEvent(false);

        transport.OnMessageSent += (MockedBroadcastResponse resp, string evt, dynamic payload) => {
          if (evt == "catalog.update:request") {
            catalogUpdateRequest.Set();
          }
          else if (evt == UserTopicEvent(connection, "update:request")) {
            fontUpdateRequest.Set();
          }
        };

        transport.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
          if (request.Endpoint.Contains("downloads")) {
            return request.CreateResponse(HttpStatusCode.OK, "BODYBODYBODY");
          }
          return null;
        };

        connection.OnCatalogUpdateFinished += delegate {
          updateFinished.Set();
        };

        connection.UpdateCatalog();

        // wait for the catalog update request
        if (!catalogUpdateRequest.WaitOne(timeout)) {
          Assert.Fail("Catalog update request should be send in less than {0}ms", timeout);
        }
        transport.SimulateMessage("catalog", "font:description", TestData.Font1_Description);
        transport.SimulateMessage("catalog", "update:complete");
        // wait for the fonts update request
        if (!fontUpdateRequest.WaitOne(timeout)) {
          Assert.Fail("Font update request should be send in less than {0}ms", timeout);
        }
        transport.SimulateMessage(UserTopicEvent(connection), "update:complete");

        if (!updateFinished.WaitOne(timeout)) {
          Assert.Fail("Update should finish in less than {0}ms", timeout);
        }

        try {
          test?.Invoke();
        }
        catch (Exception) { }

      }, timeout);
    }
    #endregion
  }
}