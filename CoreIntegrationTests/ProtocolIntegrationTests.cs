using Microsoft.VisualStudio.TestTools.UnitTesting;
using Protocol.Impl.Tests;
using Storage;
using Storage.Impl;
using System.Threading;
using System.Threading.Tasks;
using TestUtilities;
using TestUtilities.FontManager;
using TestUtilities.Protocol;

namespace ProtocolImplTests {
  [TestClass]
  public class Tmp {
    [TestMethod]
    [TestCategory("Protocol.Integration")]
    public void FontActivationMessages_shouldGenerateFontInstallationReports() {
      MockedTransport transport = new MockedTransport();
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      IFontStorage storage = new FontStorage(http, installer);
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

        transport.SimulateMessage(connection.UserTopicEvent(), "font:activation", TestData.Font1_Id);

        Thread.Sleep(50);
        Assert.AreEqual(1, installReport, "Real-time server activation should generate installation reports");
        Assert.AreEqual(0, uninstallReport, "Real-time server activation should not generate uninstallation reports");
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Integration")]
    public void FontDeactivationMessages_shouldGenerateFontUninstallationReports() {
      MockedTransport transport = new MockedTransport();
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      IFontStorage storage = new FontStorage(http, installer);
      TestConnection connection = new TestConnection(transport, http, storage);

      connection.Updated(delegate {
        storage.ActivateFont(TestData.Font1_Description.UID);
        Thread.Sleep(10);

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

        transport.SimulateMessage(connection.UserTopicEvent(), "font:deactivation", TestData.Font1_Id); // uninstall ok

        Thread.Sleep(50);
        Assert.AreEqual(0, installReport, "Real-time server deactivation should not generate installation reports");
        Assert.AreEqual(1, uninstallReport, "Real-time server deactivation should generate uninstallation reports");
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Integration")]
    public void FontDescriptionMessages_shouldNotGenerateFontInstallationReports_whenFontIsNew() {
      MockedTransport transport = new MockedTransport();
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      IFontStorage storage = new FontStorage(http, installer);
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

        transport.SimulateMessage("catalog", "font:description", TestData.Font2_Description);

        Thread.Sleep(50);
        Assert.AreEqual(0, installReport, "Real-time server font description should not generate installation reports");
        Assert.AreEqual(0, uninstallReport, "Real-time server font description should not generate uninstallation reports");
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Integration")]
    public void FontDescriptionMessages_shouldNotGenerateFontInstallationReports_whenFontIsUpdated_andDeactivated() {
      MockedTransport transport = new MockedTransport();
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      IFontStorage storage = new FontStorage(http, installer);
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

        transport.SimulateMessage("catalog", "font:description", TestData.Font1_Description2);

        Thread.Sleep(50);

        Assert.AreEqual(0, installReport, "Real-time server font update should not generate installation reports when font is deactivated");
        Assert.AreEqual(0, uninstallReport, "Real-time server font update should not generate uninstallation reports when font is deactivated");
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Integration")]
    public void FontDescriptionMessages_shouldGenerateFontInstallationReports_whenFontIsUpdated_andActivated() {
      MockedTransport transport = new MockedTransport();
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      IFontStorage storage = new FontStorage(http, installer);
      TestConnection connection = new TestConnection(transport, http, storage);

      connection.Updated(delegate {
        storage.ActivateFont(TestData.Font1_Description.UID);
        Thread.Sleep(10);

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

        transport.SimulateMessage("catalog", "font:description", TestData.Font1_Description2);

        Thread.Sleep(50);

        Assert.AreEqual(1, installReport, "Real-time server font update should generate installation reports when font is activated");
        Assert.AreEqual(1, uninstallReport, "Real-time server font update should generate uninstallation reports when font is activated");
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Integration")]
    public void FontDeletionMessages_shouldNotGenerateFontInstallationReports_whenFontIsDeactivated() {
      MockedTransport transport = new MockedTransport();
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      IFontStorage storage = new FontStorage(http, installer);
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

        transport.SimulateMessage("catalog", "font:deletion", TestData.Font2_Id);

        Thread.Sleep(50);

        Assert.AreEqual(0, installReport, "Real-time server font deletion should not generate installation reports when font is deactivated");
        Assert.AreEqual(0, uninstallReport, "Real-time server font deletion should not generate uninstallation reports when font is deactivated");
      });
    }

    [TestMethod]
    [TestCategory("Protocol.Integration")]
    public void FontDeletionMessages_shouldGenerateFontInstallationReports_whenFontIsActivated() {
      MockedTransport transport = new MockedTransport();
      MockedHttpTransport http = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      IFontStorage storage = new FontStorage(http, installer);
      TestConnection connection = new TestConnection(transport, http, storage);

      connection.Updated(delegate {
        storage.ActivateFont(TestData.Font1_Description.UID);
        Thread.Sleep(10);

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

        transport.SimulateMessage("catalog", "font:deletion", TestData.Font1_Id);

        Thread.Sleep(50);

        Assert.AreEqual(0, installReport, "Real-time server font deletion should not generate installation reports when font is activated");
        Assert.AreEqual(1, uninstallReport, "Real-time server font deletion should generate uninstallation reports when font is activated");
      });
    }
  }
}
