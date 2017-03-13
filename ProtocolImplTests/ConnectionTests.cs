using Microsoft.VisualStudio.TestTools.UnitTesting;
using Protocol.Payloads;
using Protocol.Transport;
using Storage;
using Storage.Data;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TestUtilities;
using TestUtilities.Protocol;

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

    [TestMethod]
    public void UpdateCatalog_shouldUpdateTheFontCatalog() {
      MockedTransport transport = new MockedTransport();
      MockedStorage storage = new MockedStorage();
      Connection connection = new TestConnection(transport, storage);

      connected(transport, connection, delegate {
        bool catalogUpdateRequested = false;

        AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        transport.OnMessageSent += (MockedBroadcastResponse resp, string evt, dynamic payload) => {
          switch(evt) {
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
        Assert.AreEqual(1, storage.Families.Count, "Font description messages should add fonts to the font storage");
        Assert.IsNull(storage.FindFont(TestData.Font1_Description.UID), "Font deletion message should remove fonts from the font storage");
      });
    }

    [TestMethod]
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
          } else if (evt == UserTopicEvent(connection, "update:request")) {
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

        Assert.IsTrue(fontUpdateRequested, "Updating the catalog should request an update of the catalog fonts status");
        bool? isActivated = storage.FindFont(TestData.Font1_Description.UID)?.Activated;
        Assert.IsTrue(isActivated.HasValue && isActivated.Value, "Font activation message should activate fonts in the font storage");
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
      Exception error = null;
      connection.OnEstablished += delegate {
        try {
          test?.Invoke();
        } catch(Exception e) {
          error = e;
        }
        testDoneEvent.Set();
      };

      connection.Connect("test_email", "test_password");

      // DEBUG unit tests
      //testDoneEvent.WaitOne();
      bool signaled = testDoneEvent.WaitOne(timeout);
      Assert.IsTrue(signaled, "Test should execute in less than {0}ms", timeout);

      if (error != null) {
        throw error;
      }
    }
    #endregion
  }

  public class MockedStorage : CallTracer, IFontStorage {
    #region properties
    public List<Family> Families { get; private set; }
    public bool HasChanged { get; private set; }
    public bool Loaded { get; private set; }
    public DateTime? LastCatalogUpdate { get; set; }
    public DateTime? LastFontStatusUpdate { get; set; }
    #endregion

    #region ctor
    public MockedStorage() {
      LastCatalogUpdate = DateTime.Now;
      LastFontStatusUpdate = DateTime.Now;
      Families = new List<Family>();
    }
    #endregion

    #region methods
    public Font AddFont(FontDescription description) {
      RegisterCall("AddFont");
      Family family = FindFamilyByName(description.FamilyName);
      Font newFont = new Font(description);

      if (family == null) {
        family = new Family(description.FamilyName);
        Families.Add(family);
      }
      family.Add(newFont);

      HasChanged = true;
      return newFont;
    }

    public void RemoveFont(string uid) {
      RegisterCall("RemoveFont");
      Family family = FindFamilyByFontUID(uid);
      if (family != null) {
        family.Remove(uid);
        if (family.Fonts.Count == 0) {
          Families.Remove(family);
        }
        HasChanged = true;
      }
    }

    public void ActivateFont(string uid) {
      RegisterCall("ActivateFont");
      Font font = FindFont(uid);
      if (font != null) {
        font.Activated = true;
        HasChanged = true;
      }
    }

    public void DeactivateFont(string uid) {
      RegisterCall("DeactivateFont");
      Font font = FindFont(uid);
      if (font != null) {
        font.Activated = false;
        HasChanged = true;
      }
    }

    public Font FindFont(string uid) {
      RegisterCall("FindFont");
      Family family = FindFamilyByFontUID(uid);
      if (family != null) {
        return family.FindFont(uid);
      }

      return null;
    }

    public Task Load() {
      RegisterCall("Load");
      return Task.Factory.StartNew(() => {
        Loaded = true;
      });
    }

    public Task Save() {
      RegisterCall("Save");
      return Task.Factory.StartNew(() => {
        HasChanged = false;
      });
    }
    #endregion

    #region private methods
    private Family FindFamilyByName(string name) {
      return Families.Find((family) => {
        return family.Name == name;
      });
    }

    private Family FindFamilyByFontUID(string uid) {
      return Families.Find((family) => {
        return family.FindFont(uid) != null;
      });
    }
    #endregion
  }
}