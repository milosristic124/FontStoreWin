using FontInstaller;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TestUtilities;
using TestUtilities.FontManager;
using TestUtilities.Protocol;
using TestUtilities.Storage;

namespace Protocol.Impl.Tests {
  [TestClass]
  public class ConnectionTests {
    [TestMethod]
    [TestCategory("Protocol.Authentication")]
    public void Connect_shouldTriggerOnEstablished_whenTheConnectionIsEstablished() {
      MockedTransport transport = new MockedTransport();
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      MockedStorage storage = new MockedStorage(installer);
      Connection connection = new TestConnection(transport, http, storage);

      bool httpRequestSent = false;
      http.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
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
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      MockedStorage storage = new MockedStorage(installer);
      Connection connection = new TestConnection(transport, http, storage);

      http.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
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
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      MockedStorage storage = new MockedStorage(installer);
      Connection connection = new TestConnection(transport, http, storage, authInterval: 50);

      bool failAuth = true;
      http.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
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
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      MockedStorage storage = new MockedStorage(installer);
      Connection connection = new TestConnection(transport, http, storage, connInterval: 50);

      http.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
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
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      MockedStorage storage = new MockedStorage(installer);
      TestConnection connection = new TestConnection(transport, http, storage);

      connection.Connected(delegate {
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
        transport.SimulateMessage("catalog", "font:description", TestData.Font3_Description);
        transport.SimulateMessage("catalog", "font:deletion", TestData.Font1_Id);

        Assert.IsTrue(catalogUpdateRequested, "Updating the catalog should request an update of the catalog fonts data");
        Assert.AreEqual(2, storage.FamilyCollection.Families.Count, "Font description messages should add fonts to the font storage");
        Assert.IsNull(storage.FindFont(TestData.Font1_Description.UID), "Font deletion message should remove fonts from the font storage");
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Update")]
    public void UpdateCatalog_shouldUpdateTheFontsStatus() {
      MockedTransport transport = new MockedTransport();
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      MockedStorage storage = new MockedStorage(installer);
      TestConnection connection = new TestConnection(transport, http, storage);

      connection.Connected(delegate {
        bool fontUpdateRequested = false;

        AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        transport.OnMessageSent += (MockedBroadcastResponse resp, string evt, dynamic payload) => {
          if (evt == "catalog.update:request") {
            autoResetEvent.Set();
          }
          else if (evt == connection.UserTopicEvent("update:request")) {
            fontUpdateRequested = true;
            autoResetEvent.Set();
          }
        };

        connection.UpdateCatalog();

        autoResetEvent.WaitOne(); // wait for the catalog update request
        transport.SimulateMessage("catalog", "font:description", TestData.Font1_Description);
        transport.SimulateMessage("catalog", "update:complete");
        autoResetEvent.WaitOne(); // wait for the fonts update request
        transport.SimulateMessage(connection.UserTopicEvent(), "font:activation", TestData.Font1_Id);
        transport.SimulateMessage(connection.UserTopicEvent(), "update:complete");

        Assert.IsTrue(fontUpdateRequested, "Updating the catalog should request an update of the catalog fonts status");
        bool? isActivated = storage.FindFont(TestData.Font1_Description.UID)?.Activated;
        Assert.IsTrue(isActivated.HasValue && isActivated.Value, "Font activation message should activate fonts in the font storage");
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Update")]
    public void UpdateCatalog_shouldTriggerEvent_whenUpdateIsFinished() {
      MockedTransport transport = new MockedTransport();
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      MockedStorage storage = new MockedStorage(installer);
      TestConnection connection = new TestConnection(transport, http, storage);

      connection.Connected(delegate {
        AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        transport.OnMessageSent += (MockedBroadcastResponse resp, string evt, dynamic payload) => {
          if (evt == "catalog.update:request") {
            autoResetEvent.Set();
          }
          else if (evt == connection.UserTopicEvent("update:request")) {
            autoResetEvent.Set();
          }
        };

        http.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
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
        transport.SimulateMessage(connection.UserTopicEvent(), "font:activation", TestData.Font1_Id);
        transport.SimulateMessage(connection.UserTopicEvent(), "update:complete");

        int timeout = 500;
        bool eventRaised = autoResetEvent.WaitOne(timeout);

        Assert.IsTrue(eventRaised, "Updating the catalog should trigger an update finished event when terminated");
      });
    }
    
    [TestMethod]
    [TestCategory("Protocol.Update")]
    public void UpdateCatalog_shouldSendInstallationReport() {
      MockedTransport transport = new MockedTransport();
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      MockedStorage storage = new MockedStorage(installer);
      TestConnection connection = new TestConnection(transport, http, storage);

      connection.Connected(delegate {
        AutoResetEvent catalogUpdateRequest = new AutoResetEvent(false);
        AutoResetEvent fontUpdateRequest = new AutoResetEvent(false);

        installer.OnInstallRequest += (string uid, InstallationScope scope) => {
          if (scope == InstallationScope.User && uid == TestData.Font2_Description.UID) {
            return false;
          }
          return true;
        };
        installer.OnUninstallRequest += (string uid, InstallationScope scope) => {
          return uid != TestData.Font3_Description.UID;
        };

        int installationSuccessReport = 0;
        int installationFailureReport = 0;
        int uninstallationSuccessReport = 0;
        int uninstallationFailureReport = 0;
        transport.OnMessageSent += (MockedBroadcastResponse resp, string evt, dynamic payload) => {
          if (evt == "catalog.update:request") {
            catalogUpdateRequest.Set();
          }
          else if (evt == connection.UserTopicEvent("update:request")) {
            fontUpdateRequest.Set();
          }
          else if (evt == connection.UserTopicEvent("font:installation-success")) {
            installationSuccessReport += 1;
          }
          else if (evt == connection.UserTopicEvent("font:installation-failure")) {
            installationFailureReport += 1;
          }
          else if (evt == connection.UserTopicEvent("font:uninstallation-success")) {
            uninstallationSuccessReport += 1;
          }
          else if (evt == connection.UserTopicEvent("font:uninstallation-failure")) {
            uninstallationFailureReport += 1;
          }
        };

        http.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
          if (request.Endpoint.Contains("downloads")) {
            return request.CreateResponse(HttpStatusCode.OK, "BODYBODYBODY");
          }
          return null;
        };

        AutoResetEvent updateFinished = new AutoResetEvent(false);
        connection.OnCatalogUpdateFinished += delegate {
          updateFinished.Set();
        };

        connection.UpdateCatalog();

        catalogUpdateRequest.WaitOne(); // wait for the catalog update request
        transport.SimulateMessage("catalog", "font:description", TestData.Font1_Description); // no report
        transport.SimulateMessage("catalog", "font:description", TestData.Font2_Description); // no report
        transport.SimulateMessage("catalog", "font:description", TestData.Font3_Description); // no report
        transport.SimulateMessage("catalog", "update:complete");
        fontUpdateRequest.WaitOne(); // wait for the fonts update request
        transport.SimulateMessage(connection.UserTopicEvent(), "font:activation", TestData.Font1_Id); // install ok
        transport.SimulateMessage(connection.UserTopicEvent(), "font:activation", TestData.Font3_Id); // install ok
        transport.SimulateMessage(connection.UserTopicEvent(), "font:activation", TestData.Font2_Id); // install ko
        transport.SimulateMessage(connection.UserTopicEvent(), "font:deactivation", TestData.Font1_Id); // uninstall ok
        transport.SimulateMessage(connection.UserTopicEvent(), "font:deactivation", TestData.Font3_Id); // uninstall ko
        transport.SimulateMessage(connection.UserTopicEvent(), "update:complete");
        updateFinished.WaitOne(); // wait for the update to finish

        Assert.AreEqual(2, installationSuccessReport, "Font installations should send messages to the server when updating catalog");
        Assert.AreEqual(1, installationFailureReport, "Font installation failures should send messages to the server when updating catalog");
        Assert.AreEqual(1, uninstallationSuccessReport, "Font uninstallations should send messages to the server when updating catalog");
        Assert.AreEqual(1, uninstallationFailureReport, "Font uninstallation failures should send messages to the server when updating catalog");
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Running")]
    public void FinishingCatalogUpdate_shouldGenerateAClientReadyMessage() {
      MockedTransport transport = new MockedTransport();
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      MockedStorage storage = new MockedStorage(installer);
      TestConnection connection = new TestConnection(transport, http, storage);

      bool clientReadyMessageSent = false;
      transport.OnMessageSent += (MockedBroadcastResponse resp, string evt, dynamic payload) => {
        if (evt == connection.UserTopicEvent("ready")) {
          clientReadyMessageSent = true;
        }
      };

      connection.Updated(delegate {
        Assert.IsTrue(clientReadyMessageSent, "The client should notify the server when it is ready to transition to real-time communication");
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Running")]
    public void FontActivationMessage_shouldActivateFont() {
      MockedTransport transport = new MockedTransport();
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      MockedStorage storage = new MockedStorage(installer);
      TestConnection connection = new TestConnection(transport, http, storage);

      connection.Updated(delegate {
        transport.SimulateMessage(connection.UserTopicEvent(), "font:activation", TestData.Font1_Id);
        storage.Verify("ActivateFont", 1);
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Running")]
    public void FontDeactivationMessage_shouldDeactivateFont() {
      MockedTransport transport = new MockedTransport();
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      MockedStorage storage = new MockedStorage(installer);
      TestConnection connection = new TestConnection(transport, http, storage);

      connection.Updated(delegate {
        transport.SimulateMessage(connection.UserTopicEvent(), "font:deactivation", TestData.Font1_Id);
        storage.Verify("DeactivateFont", 1);
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Running")]
    public void FontDescriptionMessage_shouldAddFontToTheCatalog() {
      MockedTransport transport = new MockedTransport();
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      MockedStorage storage = new MockedStorage(installer);
      TestConnection connection = new TestConnection(transport, http, storage);

      connection.Updated(delegate {
        transport.SimulateMessage("catalog", "font:description", TestData.Font2_Description);
        storage.Verify("AddFont", 2);
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Running")]
    public void FontRemovedMessage_shouldRemoveFontFromCatalog() {
      MockedTransport transport = new MockedTransport();
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      MockedStorage storage = new MockedStorage(installer);
      TestConnection connection = new TestConnection(transport, http, storage);

      connection.Updated(delegate {
        transport.SimulateMessage("catalog", "font:deletion", TestData.Font1_Id);
        storage.Verify("RemoveFont", 1);
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Running")]
    public void FontInstallationEvent_shouldSendFontInstallationReport_whenInstallScopeIsUser() {
      MockedTransport transport = new MockedTransport();
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      MockedStorage storage = new MockedStorage(installer);
      TestConnection connection = new TestConnection(transport, http, storage);

      connection.Updated(delegate {
        int installReport = 0;
        int uninstallReport = 0;
        transport.OnMessageSent += (MockedBroadcastResponse resp, string evt, dynamic payload) => {
          if (evt == connection.UserTopicEvent("font:installation-success")) {
            installReport += 1;
          } else if (evt == connection.UserTopicEvent("font:uninstallation-success")) {
            uninstallReport += 1;
          }
        };

        Task asyncEvents = Task.Run(delegate {
          storage.SimulateFontInstall(TestData.Font1_Description.UID, InstallationScope.Process, true);
          storage.SimulateFontInstall(TestData.Font1_Description.UID, InstallationScope.User, true);
        });
        asyncEvents.Wait();

        Assert.AreEqual(1, installReport, "Font install event should generate font install reports when scope is User");
        Assert.AreEqual(0, uninstallReport, "Font install event should not generate font uninstall reports");
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Running")]
    public void FontUninstallationEvent_shouldSendFontUninstallationReport_whenInstallScopeIsUser() {
      MockedTransport transport = new MockedTransport();
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      MockedStorage storage = new MockedStorage(installer);
      TestConnection connection = new TestConnection(transport, http, storage);

      connection.Updated(delegate {
        int installReport = 0;
        int uninstallReport = 0;
        transport.OnMessageSent += (MockedBroadcastResponse resp, string evt, dynamic payload) => {
          if (evt == connection.UserTopicEvent("font:installation-success")) {
            installReport += 1;
          }
          else if (evt == connection.UserTopicEvent("font:uninstallation-success")) {
            uninstallReport += 1;
          }
        };

        Task asyncEvents = Task.Run(delegate {
          storage.SimulateFontUninstall(TestData.Font1_Description.UID, InstallationScope.Process, true);
          storage.SimulateFontUninstall(TestData.Font1_Description.UID, InstallationScope.User, true);
        });
        asyncEvents.Wait();

        Assert.AreEqual(0, installReport, "Font install event should not generate font install reports");
        Assert.AreEqual(1, uninstallReport, "Font install event should generate font uninstall reports when scope is User");
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Disconnect")]
    public void Disconnect_shouldDisconnectTheTransport_andStopUpdatingFontStorage() {
      MockedTransport transport = new MockedTransport();
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      MockedStorage storage = new MockedStorage(installer);
      TestConnection connection = new TestConnection(transport, http, storage);

      connection.Updated(delegate {
        AutoResetEvent disconnected = new AutoResetEvent(false);
        connection.OnConnectionClosed += () => {
          disconnected.Set();
        };

        transport.OnDisconnectAttempt += () => true;

        connection.Disconnect(DisconnectReason.Quit);

        int timeout = 500;
        bool signaled = disconnected.WaitOne(timeout);
        Assert.IsTrue(signaled, "Disconnect should trigger a connection closed event");

        storage.Verify("EndSynchronization", 1);
        transport.Verify("Disconnect", 1);
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Disconnect")]
    public void DisconnectReception_shouldTriggerTerminationEvent_andStopUpdatingFontStorage() {
      MockedTransport transport = new MockedTransport();
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      MockedStorage storage = new MockedStorage(installer);
      TestConnection connection = new TestConnection(transport, http, storage);

      string reason = null;
      connection.Updated(delegate {
        AutoResetEvent disconnected = new AutoResetEvent(false);
        connection.OnConnectionTerminated += (r) => {
          reason = r;
          disconnected.Set();
        };

        transport.SimulateMessage(connection.UserTopicEvent(), "disconnect", TestData.DisconnectReason);

        int timeout = 500;
        bool signaled = disconnected.WaitOne(timeout);
        Assert.IsTrue(signaled, "Server disconnection should trigger a connection termination event");
        Assert.AreEqual(TestData.DisconnectReason.Reason, reason, "Termination event reason should be the reason the server sent");
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Disconnect")]
    public void TransportTermination_shouldTriggerDisconnectedEvent() {
      MockedTransport transport = new MockedTransport();
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      MockedStorage storage = new MockedStorage(installer);
      TestConnection connection = new TestConnection(transport, http, storage);

      connection.Updated(delegate {
        AutoResetEvent disconnected = new AutoResetEvent(false);
        connection.OnDisconnected += delegate {
          disconnected.Set();
        };

        transport.SimulateTermination();

        int timeout = 500;
        bool signaled = disconnected.WaitOne(timeout);
        Assert.IsTrue(signaled, "Transport termination should trigger a disconnected event");
      });
    }
  }
}