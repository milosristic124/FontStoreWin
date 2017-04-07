using FontInstaller;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Storage.Data;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TestUtilities;
using TestUtilities.FontManager;
using TestUtilities.Protocol;

namespace Storage.Impl.Tests {
  [TestClass]
  public class FontStorageTests {
    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void FontStorage_shouldBeEmpty_whenCreated() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);
      Assert.IsTrue(storage.FamilyCollection.Families.Count == 0, "FontStorage has no data when created");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void FindFont_shouldReturnNull_whenTheFontDoesNotExist() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);
      Assert.IsNull(storage.FindFont(TestData.Font1_Description.UID), "FindFont return null for unexisting fonts");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void FindFont_shouldReturnTheSearchedFont() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);
      storage.LoadFonts().Wait();

      storage.AddFont(TestData.Font1_Description);

      Assert.IsNotNull(storage.FindFont(TestData.Font1_Description.UID), "FindFont should find an existing font");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void AddFont_shouldUpdateFontStorage() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);
      storage.LoadFonts().Wait();

      storage.AddFont(TestData.Font1_Description);

      Assert.IsTrue(storage.FamilyCollection.Families.Count == 1, "AddFont should add a font to the FontStorage");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void AddFont_shouldReplaceObsoleteData() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);
      storage.LoadFonts().Wait();

      storage.AddFont(TestData.Font1_Description);
      storage.AddFont(TestData.Font1_Description2);

      Assert.AreSame(TestData.Font1_Description2.Name, storage.FamilyCollection.Families[0].Fonts[0].Name,
        "AddFont should replace obsolete font in the FontStorage");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void RemoveFont_shouldUpdateFontStorage() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);
      storage.LoadFonts().Wait();

      storage.AddFont(TestData.Font1_Description);
      storage.RemoveFont(TestData.Font1_Id);

      Assert.IsTrue(storage.FamilyCollection.Families.Count == 0, "RemoveFont should remove font from the FontStorage");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void ActivateFont_shouldActivateFont() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);
      storage.LoadFonts().Wait();

      storage.AddFont(TestData.Font1_Description);
      storage.ActivateFont(TestData.Font1_Id);

      Assert.IsTrue(storage.FindFont(TestData.Font1_Description.UID).Activated, "ActivateFont shoudl activate the corresponding font in the FontStorage");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void DeactivateFont_shouldDeactivateFont() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);
      storage.LoadFonts().Wait();

      storage.AddFont(TestData.Font1_Description);
      storage.ActivateFont(TestData.Font1_Id);
      storage.DeactivateFont(TestData.Font1_Id);

      Assert.IsFalse(storage.FindFont(TestData.Font1_Description.UID).Activated, "DeactivateFont should deactivate the corresponding font in the FontStorage");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void Load_shouldLoadSavedData() {
      string storagePath = TestPath;
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, storagePath);
      storage.SessionID = "session1";

      storage.LoadFonts().Wait();
      storage.AddFont(TestData.Font1_Description);
      storage.ActivateFont(TestData.Font1_Id);
      storage.SaveFonts().Wait();

      storage = new Storage(transport, installer, storagePath);
      storage.SessionID = "session1";

      int timeout = 5000;
      bool signaled = storage.LoadFonts().Wait(timeout);

      Assert.IsTrue(signaled, "Load should not timeout during tests...");
      Assert.IsTrue(storage.FamilyCollection.Families.Count == 1, "Load should load saved catalog data from file system");
      Assert.IsTrue(storage.FindFont(TestData.Font1_Description.UID).Activated, "Load should load saved fonts data from file system");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void Load_shouldNotLoadAnotherUserSavedData() {
      string storagePath = TestPath;
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, storagePath);
      storage.SessionID = "session1";

      storage.LoadFonts().Wait();
      storage.AddFont(TestData.Font1_Description);
      storage.ActivateFont(TestData.Font1_Id);
      storage.SaveFonts().Wait();

      storage = new Storage(transport, installer, storagePath);
      storage.SessionID = "session2";

      int timeout = 5000;
      bool signaled = storage.LoadFonts().Wait(timeout);

      Assert.IsTrue(signaled, "Load should not timeout during tests...");
      Assert.IsTrue(storage.FamilyCollection.Families.Count == 0, "Load should not load saved catalog data of another user");
    }

    [TestMethod]
    [TestCategory("Storage.FullSynchronization")]
    public void SynchronizeWithSystem_shouldDownloadAndInstallFonts() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);

      storage.AddFont(TestData.Font1_Description);
      storage.AddFont(TestData.Font2_Description);
      storage.AddFont(TestData.Font3_Description);
      storage.ActivateFont(TestData.Font1_Id);

      int downloadCount = 0;
      transport.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
        if (request.Endpoint.Contains("downloads")) {
          downloadCount += 1;
          return request.CreateResponse(System.Net.HttpStatusCode.OK, "FONTFONTFONTFONT");
        }
        return null;
      };

      installer.OnInstallRequest += (string uid, InstallationScope scope) => true;

      AutoResetEvent evt = new AutoResetEvent(false);
      storage.SynchronizeWithSystem(delegate {
        evt.Set();
      });

      int timeout = 1000;
      if (!evt.WaitOne(timeout)) {
        Assert.Fail("SynchronizeWithSystem should finish synchronizing in less than {0} ms", timeout);
      }

      Assert.AreEqual(3, downloadCount, "SynchronizeWithSystem should download all the fonts in the catalog");
      installer.Verify("InstallFont", 4);
      installer.Verify("UnsintallFont", 0);

      Assert.AreEqual(InstallationScope.Process, installer.FontInstallationScope(TestData.Font2_Description.UID),
        "SynchronizeWithSystem should install deactivated fonts for process only");
      Assert.AreEqual(InstallationScope.User | InstallationScope.Process, installer.FontInstallationScope(TestData.Font1_Description.UID),
        "SynchronizeWithSystem should install activated fonts for process and user");
    }

    [TestMethod]
    [TestCategory("Storage.FullSynchronization")]
    public void SynchronizeWithSystem_shouldNotReactToRealTimeEventsAfterward() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);

      storage.AddFont(TestData.Font1_Description);

      int downloadCount = 0;
      transport.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
        if (request.Endpoint.Contains("downloads")) {
          downloadCount += 1;
          return request.CreateResponse(System.Net.HttpStatusCode.OK, "FONTFONTFONTFONT");
        }
        return null;
      };

      installer.OnInstallRequest += (string uid, InstallationScope scope) => true;

      AutoResetEvent evt = new AutoResetEvent(false);
      storage.SynchronizeWithSystem(delegate {
        evt.Set();
      });

      int timeout = 1000;
      if (!evt.WaitOne(timeout)) {
        Assert.Fail("SynchronizeWithSystem should finish synchronizing in less than {0} ms", timeout);
      }

      storage.AddFont(TestData.Font2_Description);

      Assert.AreEqual(1, downloadCount, "SynchronizeWithSystem should not download fonts in realtime");
      installer.Verify("InstallFont", 1);
      installer.Verify("UnsintallFont", 0);
    }

    [TestMethod]
    [TestCategory("Storage.FullSynchronization")]
    public void SynchronizeWithSystem_shouldExecuteCallbackImmediately_whenNoActionsAreQueued() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);

      bool callbackExecuted = false;
      storage.SynchronizeWithSystem(delegate {
        callbackExecuted = true;
      });

      Assert.IsTrue(callbackExecuted, "SynchronizeWithSystem should execute callback immediately if there is nothing to do");
    }

    [TestMethod]
    [TestCategory("Storage.RealTimeSynchronization")]
    public void BeginSynchronization_shouldDownloadAndInstallFonts() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);

      storage.AddFont(TestData.Font1_Description);
      storage.AddFont(TestData.Font2_Description);
      storage.AddFont(TestData.Font3_Description);
      storage.ActivateFont(TestData.Font1_Id);

      int downloadCount = 0;
      transport.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
        if (request.Endpoint.Contains("downloads")) {
          downloadCount += 1;
          return request.CreateResponse(System.Net.HttpStatusCode.OK, "FONTFONTFONTFONT");
        }
        return null;
      };

      installer.OnInstallRequest += (string uid, InstallationScope scope) => true;

      storage.BeginSynchronization();

      int timeout = 1000;
      if (!Task.Delay(10).Wait(timeout)) {
        Assert.Fail("BeginSynchronization should finish synchronizing in less than {0} ms", timeout);
      }


      Assert.AreEqual(3, downloadCount, "BeginSynchronization should download all the buffered updates");
      installer.Verify("InstallFont", 4);
      installer.Verify("UnsintallFont", 0);

      Assert.AreEqual(InstallationScope.Process, installer.FontInstallationScope(TestData.Font2_Description.UID),
        "BeginSynchronization should install deactivated fonts for process only");
      Assert.AreEqual(InstallationScope.User | InstallationScope.Process, installer.FontInstallationScope(TestData.Font1_Description.UID),
        "BeginSynchronization should install activated fonts for process and user");
    }

    [TestMethod]
    [TestCategory("Storage.RealTimeSynchronization")]
    public void BeginSynchronization_shouldReactToRealTimeEvents() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);

      int downloadCount = 0;
      transport.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
        if (request.Endpoint.Contains("downloads")) {
          downloadCount += 1;
          return request.CreateResponse(System.Net.HttpStatusCode.OK, "FONTFONTFONTFONT");
        }
        return null;
      };

      installer.OnInstallRequest += (string uid, InstallationScope scope) => true;

      storage.BeginSynchronization();

      Task asyncEvents = Task.Run(delegate {
        storage.AddFont(TestData.Font1_Description);
        storage.AddFont(TestData.Font2_Description);
        storage.ActivateFont(TestData.Font1_Id);
        Thread.Sleep(10);
      });

      int timeout = 1000;
      if (!asyncEvents.Wait(timeout)) {
        Assert.Fail("BeginSynchronization should finish synchronizing in less than {0} ms", timeout);
      }


      Assert.AreEqual(2, downloadCount, "BeginSynchronization should download fonts in realtime");
      installer.Verify("InstallFont", 3);
      installer.Verify("UnsintallFont", 0);

      Assert.AreEqual(InstallationScope.Process, installer.FontInstallationScope(TestData.Font2_Description.UID),
        "BeginSynchronization should install deactivated fonts for process only");
      Assert.AreEqual(InstallationScope.User | InstallationScope.Process, installer.FontInstallationScope(TestData.Font1_Description.UID),
        "BeginSynchronization should install activated fonts for process and user");
    }

    [TestMethod]
    [TestCategory("Storage.RealTimeSynchronization")]
    public void EndSynchronization_shouldNotReactToRealTimeEventsAfterward() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);

      int downloadCount = 0;
      transport.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
        if (request.Endpoint.Contains("downloads")) {
          downloadCount += 1;
          return request.CreateResponse(System.Net.HttpStatusCode.OK, "FONTFONTFONTFONT");
        }
        return null;
      };

      installer.OnInstallRequest += (string uid, InstallationScope scope) => true;

      storage.EndSynchronization();

      Task asyncEvents = Task.Run(delegate {
        storage.AddFont(TestData.Font1_Description);
        storage.AddFont(TestData.Font2_Description);
        storage.ActivateFont(TestData.Font1_Id);
        Thread.Sleep(10);
      });

      int timeout = 1000;
      if (!asyncEvents.Wait(timeout)) {
        Assert.Fail("EndSynchronization should finish synchronizing in less than {0} ms", timeout);
      }


      Assert.AreEqual(0, downloadCount, "EndSynchronization should not download fonts in realtime");
      installer.Verify("InstallFont", 0);
      installer.Verify("UnsintallFont", 0);
    }

    [TestMethod]
    [TestCategory("Storage.Events")]
    public void ActivateFont_shouldTriggerUserScopeInstallationEvent() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);

      transport.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
        return request.CreateResponse(System.Net.HttpStatusCode.OK, "blabla");
      };

      bool eventTriggered = false;
      storage.OnFontInstall += (Font font, InstallationScope scope, bool succeed) => {
        if (font.UID == TestData.Font1_Description.UID && scope == InstallationScope.User) {
          eventTriggered = true;
        }
      };

      storage.AddFont(TestData.Font1_Description);
      storage.ActivateFont(TestData.Font1_Id);

      AutoResetEvent syncDone = new AutoResetEvent(false);
      storage.SynchronizeWithSystem(delegate {
        syncDone.Set();
      });
      int timeout = 500;
      Assert.IsTrue(syncDone.WaitOne(timeout), "Storage.SynchronizeWithSystem should finish in less than {0} ms", timeout);

      Assert.IsTrue(eventTriggered, "Storage.ActivateFont should trigger an installation in user scope event for activated font");
    }

    [TestMethod]
    [TestCategory("Storage.Events")]
    public void DeactivateFont_shouldTriggerUserScopeUninstallationEvent() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);

      transport.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
        return request.CreateResponse(System.Net.HttpStatusCode.OK, "blabla");
      };

      bool eventTriggered = false;
      storage.OnFontUninstall += (Font font, InstallationScope scope, bool succeed) => {
        if (font.UID == TestData.Font1_Description.UID && scope == InstallationScope.User) {
          eventTriggered = true;
        }
      };

      storage.AddFont(TestData.Font1_Description);
      storage.ActivateFont(TestData.Font1_Id);
      storage.DeactivateFont(TestData.Font1_Id);

      AutoResetEvent syncDone = new AutoResetEvent(false);
      storage.SynchronizeWithSystem(delegate {
        syncDone.Set();
      });
      int timeout = 500;
      Assert.IsTrue(syncDone.WaitOne(timeout), "Storage.SynchronizeWithSystem should finish in less than {0} ms", timeout);

      Assert.IsTrue(eventTriggered, "Storage.DeactivateFont should trigger an uninstallation in user scope event for deactivated font");
    }

    [TestMethod]
    [TestCategory("Storage.Events")]
    public void RemoveFont_shouldTriggerAllScopeUninstallationEvent() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);

      transport.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
        return request.CreateResponse(System.Net.HttpStatusCode.OK, "blabla");
      };

      bool userScopeTriggered = false;
      bool procScopeTriggered = false;
      storage.OnFontUninstall += (Font font, InstallationScope scope, bool succeed) => {
        if (font.UID == TestData.Font1_Description.UID && scope == InstallationScope.Process) {
          procScopeTriggered = true;
        } else if (font.UID == TestData.Font1_Description.UID && scope == InstallationScope.User) {
          userScopeTriggered = true;
        }
      };

      storage.AddFont(TestData.Font1_Description);
      storage.ActivateFont(TestData.Font1_Id);
      storage.RemoveFont(TestData.Font1_Id);

      AutoResetEvent syncDone = new AutoResetEvent(false);
      storage.SynchronizeWithSystem(delegate {
        syncDone.Set();
      });
      int timeout = 500;
      Assert.IsTrue(syncDone.WaitOne(timeout), "Storage.SynchronizeWithSystem should finish in less than {0} ms", timeout);

      Assert.IsTrue(procScopeTriggered && userScopeTriggered,
        "Storage.RemoveFont should trigger an uninstallation in Process and User scope event for uninstalled font");
    }

    [TestMethod]
    [TestCategory("Storage.Events")]
    public void AddFont_shouldTriggerProcessScopeInstallationEvent() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);

      transport.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
        return request.CreateResponse(System.Net.HttpStatusCode.OK, "blabla");
      };

      bool eventTriggered = false;
      storage.OnFontInstall += (Font font, InstallationScope scope, bool succeed) => {
        if (font.UID == TestData.Font1_Description.UID && scope == InstallationScope.Process) {
          eventTriggered = true;
        }
      };

      storage.AddFont(TestData.Font1_Description);

      AutoResetEvent syncDone = new AutoResetEvent(false);
      storage.SynchronizeWithSystem(delegate {
        syncDone.Set();
      });
      int timeout = 500;
      Assert.IsTrue(syncDone.WaitOne(timeout), "Storage.SynchronizeWithSystem should finish in less than {0} ms", timeout);

      Assert.IsTrue(eventTriggered, "Storage.AddFont should trigger an installation in process scope event for added font");
    }

    [TestMethod]
    [TestCategory("Storage.Events")]
    public void AddFont_shouldTriggerProcessScopeInstallationEvents_whenFontIsUpdated_andWasDeactivated() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);

      transport.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
        return request.CreateResponse(System.Net.HttpStatusCode.OK, "blabla");
      };

      storage.AddFont(TestData.Font1_Description);

      AutoResetEvent syncDone = new AutoResetEvent(false);
      storage.SynchronizeWithSystem(delegate {
        syncDone.Set();
      });
      int timeout = 500;
      Assert.IsTrue(syncDone.WaitOne(timeout), "Storage.SynchronizeWithSystem should finish in less than {0} ms", timeout);
      // ensure font is downloaded/installed

      bool procUninstall = false;
      bool userUninstall = false;
      storage.OnFontUninstall += (Font font, InstallationScope scope, bool succeed) => {
        if (font.UID == TestData.Font1_Description.UID && scope == InstallationScope.Process) {
          procUninstall = true;
        } else if (font.UID == TestData.Font1_Description.UID && scope == InstallationScope.User) {
          userUninstall = true;
        }
      };
      bool procInstall = false;
      bool userInstall = false;
      storage.OnFontInstall += (Font font, InstallationScope scope, bool succeed) => {
        if (font.UID == TestData.Font1_Description.UID && scope == InstallationScope.Process) {
          procInstall = true;
        } else if (font.UID == TestData.Font1_Description.UID && scope == InstallationScope.User) {
          userInstall = true;
        }
      };

      storage.AddFont(TestData.Font1_Description2);

      storage.SynchronizeWithSystem(delegate {
        syncDone.Set();
      });
      Assert.IsTrue(syncDone.WaitOne(timeout), "Storage.SynchronizeWithSystem should finish in less than {0} ms", timeout);

      Assert.IsTrue(procUninstall, "Updating a font should trigger Process scope uninstall");
      Assert.IsTrue(procInstall, "Updating a font should trigger Process scope install");

      Assert.IsFalse(userUninstall, "Updating a deactivated font should not trigger a User scope uninstall");
      Assert.IsFalse(userInstall, "Updating a deactivated font should not trigger a User scope install");
    }

    [TestMethod]
    [TestCategory("Storage.Events")]
    public void AddFont_shouldTriggerAllScopeInstallationEvents_whenFontIdUpdated_andWasActivated() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);

      transport.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
        return request.CreateResponse(System.Net.HttpStatusCode.OK, "blabla");
      };

      storage.AddFont(TestData.Font1_Description);
      storage.ActivateFont(TestData.Font1_Id);

      AutoResetEvent syncDone = new AutoResetEvent(false);
      storage.SynchronizeWithSystem(delegate {
        syncDone.Set();
      });
      int timeout = 500;
      Assert.IsTrue(syncDone.WaitOne(timeout), "Storage.SynchronizeWithSystem should finish in less than {0} ms", timeout);
      // ensure font is downloaded/installed

      bool procUninstall = false;
      bool userUninstall = false;
      storage.OnFontUninstall += (Font font, InstallationScope scope, bool succeed) => {
        if (font.UID == TestData.Font1_Description.UID && scope == InstallationScope.Process) {
          procUninstall = true;
        }
        else if (font.UID == TestData.Font1_Description.UID && scope == InstallationScope.User) {
          userUninstall = true;
        }
      };
      bool procInstall = false;
      bool userInstall = false;
      storage.OnFontInstall += (Font font, InstallationScope scope, bool succeed) => {
        if (font.UID == TestData.Font1_Description.UID && scope == InstallationScope.Process) {
          procInstall = true;
        }
        else if (font.UID == TestData.Font1_Description.UID && scope == InstallationScope.User) {
          userInstall = true;
        }
      };

      storage.AddFont(TestData.Font1_Description2);

      storage.SynchronizeWithSystem(delegate {
        syncDone.Set();
      });
      Assert.IsTrue(syncDone.WaitOne(timeout), "Storage.SynchronizeWithSystem should finish in less than {0} ms", timeout);

      Assert.IsTrue(procUninstall, "Updating a font should trigger Process scope uninstall");
      Assert.IsTrue(procInstall, "Updating a font should trigger Process scope install");

      Assert.IsTrue(userUninstall, "Updating an activated font should trigger a User scope uninstall");
      Assert.IsTrue(userInstall, "Updating an activated font should trigger a User scope install");
    }

    [TestMethod]
    [TestCategory("Storage.Events")]
    public void FontRequestActivation_shouldTriggerFontActivationRequestEvent() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);

      storage.AddFont(TestData.Font1_Description);
      Font font = storage.FindFont(TestData.Font1_Description.UID);

      bool eventTriggered = false;
      storage.OnFontActivationRequest += delegate {
        eventTriggered = true;
      };

      font.RequestActivation();

      Assert.IsTrue(eventTriggered, "Storage should trigger font activation request event when font activation is requested");
    }

    [TestMethod]
    [TestCategory("Storage.Events")]
    public void FontRequestDeactivation_shouldTriggerFontDeactivationRequestEvent() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);

      storage.AddFont(TestData.Font1_Description);
      Font font = storage.FindFont(TestData.Font1_Description.UID);

      bool eventTriggered = false;
      storage.OnFontDeactivationRequest += delegate {
        eventTriggered = true;
      };

      font.RequestDeactivation();

      Assert.IsTrue(eventTriggered, "Storage should trigger font deactivation request event when font deactivation is requested");
    }

    [TestMethod]
    [TestCategory("Storage.Credentials")]
    public void SaveCredentials_shouldCreateCredentialFile() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      string storagePath = TestPath;
      Storage storage = new Storage(transport, installer, storagePath);

      storage.SaveCredentials("creds").Wait();

      Assert.IsTrue(File.Exists(storagePath + "creds"), "SaveCredentials should create credentials file on file system");
    }

    [TestMethod]
    [TestCategory("Storage.Credentials")]
    public void LoadCredentials_shouldLoadSavedCredentials() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      Storage storage = new Storage(transport, installer, TestPath);

      string savedCreds = "creds";
      storage.SaveCredentials(savedCreds).Wait();

      string creds = storage.LoadCredentials().Result;
      Assert.AreEqual(savedCreds, creds, "LoadCredentials should load saved credentials");
    }

    [TestMethod]
    [TestCategory("Storage.Credentials")]
    public void CleanSavedCredentials_shouldRemoveCredentialsFile() {
      MockedHttpTransport transport = new MockedHttpTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      string storagePath = TestPath;
      Storage storage = new Storage(transport, installer, storagePath);

      storage.SaveCredentials("creds").Wait();
      storage.CleanCredentials().Wait();

      Assert.IsFalse(File.Exists(storagePath + "creds"), "CleanCredentials should remove credentials file from file system");
    }


    private string TestPath {
      get {
        string tempDir = Path.GetTempPath();
        return string.Format("{0}{1}\\", tempDir, Guid.NewGuid().ToString());
      }
    }
  }
}