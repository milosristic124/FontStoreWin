using FontInstaller;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;
using TestUtilities;
using TestUtilities.Protocol;
using System.Threading.Tasks;
using System.Collections.Generic;
using TestUtilities.FontManager;
using Storage.Data;

namespace Storage.Impl.Tests {
  [TestClass]
  public class FontStorageTests {
    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void FontStorage_shouldBeEmpty_whenCreated() {
      MockedTransport transport = new MockedTransport();
      FontStorage storage = new FontStorage(transport, null, TestPath);
      Assert.IsTrue(storage.FamilyCollection.Families.Count == 0, "FontStorage has no data when created");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void FindFont_shouldReturnNull_whenTheFontDoesNotExist() {
      MockedTransport transport = new MockedTransport();
      FontStorage storage = new FontStorage(transport, null, TestPath);
      Assert.IsNull(storage.FindFont(TestData.Font1_Description.UID), "FindFont return null for unexisting fonts");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void FindFont_shouldReturnTheSearchedFont() {
      MockedTransport transport = new MockedTransport();
      FontStorage storage = new FontStorage(transport, null, TestPath);
      storage.Load().Wait();

      storage.AddFont(TestData.Font1_Description);

      Assert.IsNotNull(storage.FindFont(TestData.Font1_Description.UID), "FindFont should find an existing font");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void AddFont_shouldUpdateFontStorage() {
      MockedTransport transport = new MockedTransport();
      FontStorage storage = new FontStorage(transport, null, TestPath);
      storage.Load().Wait();

      storage.AddFont(TestData.Font1_Description);

      Assert.IsTrue(storage.FamilyCollection.Families.Count == 1, "AddFont should add a font to the FontStorage");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void AddFont_shouldReplaceObsoleteData() {
      MockedTransport transport = new MockedTransport();
      FontStorage storage = new FontStorage(transport, null, TestPath);
      storage.Load().Wait();

      storage.AddFont(TestData.Font1_Description);
      storage.AddFont(TestData.Font1_Description2);

      Assert.AreSame(TestData.Font1_Description2, storage.FamilyCollection.Families[0].Fonts[0].Description,
        "AddFont should replace obsolete font in the FontStorage");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void RemoveFont_shouldUpdateFontStorage() {
      MockedTransport transport = new MockedTransport();
      FontStorage storage = new FontStorage(transport, null, TestPath);
      storage.Load().Wait();

      storage.AddFont(TestData.Font1_Description);
      storage.RemoveFont(TestData.Font1_Description.UID);

      Assert.IsTrue(storage.FamilyCollection.Families.Count == 0, "RemoveFont should remove font from the FontStorage");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void ActivateFont_shouldActivateFont() {
      MockedTransport transport = new MockedTransport();
      FontStorage storage = new FontStorage(transport, null, TestPath);
      storage.Load().Wait();

      storage.AddFont(TestData.Font1_Description);
      storage.ActivateFont(TestData.Font1_Description.UID);

      Assert.IsTrue(storage.FindFont(TestData.Font1_Description.UID).Activated, "ActivateFont shoudl activate the corresponding font in the FontStorage");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void DeactivateFont_shouldDeactivateFont() {
      MockedTransport transport = new MockedTransport();
      FontStorage storage = new FontStorage(transport, null, TestPath);
      storage.Load().Wait();

      storage.AddFont(TestData.Font1_Description);
      storage.ActivateFont(TestData.Font1_Description.UID);
      storage.DeactivateFont(TestData.Font1_Description.UID);

      Assert.IsFalse(storage.FindFont(TestData.Font1_Description.UID).Activated, "DeactivateFont should deactivate the corresponding font in the FontStorage");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void Load_shouldLoadSavedData() {
      string storagePath = TestPath;
      MockedTransport transport = new MockedTransport();
      FontStorage storage = new FontStorage(transport, null, storagePath);

      storage.Load().Wait();
      storage.AddFont(TestData.Font1_Description);
      storage.ActivateFont(TestData.Font1_Description.UID);
      storage.Save().Wait();

      storage = new FontStorage(transport, null, storagePath);

      int timeout = 5000;
      bool signaled = storage.Load().Wait(timeout);

      Assert.IsTrue(signaled, "Load should not timeout during tests...");
      Assert.IsTrue(storage.FamilyCollection.Families.Count == 1, "Load should load saved catalog data from file system");
      Assert.IsTrue(storage.FindFont(TestData.Font1_Description.UID).Activated, "Load should load saved fonts data from file system");
    }

    [TestMethod]
    [TestCategory("Storage.FullSynchronization")]
    public void SynchronizeWithSystem_shouldDownloadAndInstallFonts() {
      MockedTransport transport = new MockedTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      FontStorage storage = new FontStorage(transport, installer, TestPath);

      storage.AddFont(TestData.Font1_Description);
      storage.AddFont(TestData.Font2_Description);
      storage.AddFont(TestData.Font3_Description);
      storage.ActivateFont(TestData.Font1_Description.UID);

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
      Assert.AreEqual(InstallationScope.All, installer.FontInstallationScope(TestData.Font1_Description.UID),
        "SynchronizeWithSystem should install activated fonts for process and user");
    }

    [TestMethod]
    [TestCategory("Storage.FullSynchronization")]
    public void SynchronizeWithSystem_shouldNotReactToRealTimeEventsAfterward() {
      MockedTransport transport = new MockedTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      FontStorage storage = new FontStorage(transport, installer, TestPath);

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
      MockedTransport transport = new MockedTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      FontStorage storage = new FontStorage(transport, installer, TestPath);

      bool callbackExecuted = false;
      storage.SynchronizeWithSystem(delegate {
        callbackExecuted = true;
      });

      Assert.IsTrue(callbackExecuted, "SynchronizeWithSystem should execute callback immediately if there is nothing to do");
    }

    [TestMethod]
    [TestCategory("Storage.RealTimeSynchronization")]
    public void BeginSynchronization_shouldDownloadAndInstallFonts() {
      MockedTransport transport = new MockedTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      FontStorage storage = new FontStorage(transport, installer, TestPath);

      storage.AddFont(TestData.Font1_Description);
      storage.AddFont(TestData.Font2_Description);
      storage.AddFont(TestData.Font3_Description);
      storage.ActivateFont(TestData.Font1_Description.UID);

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
      Assert.AreEqual(InstallationScope.All, installer.FontInstallationScope(TestData.Font1_Description.UID),
        "BeginSynchronization should install activated fonts for process and user");
    }

    [TestMethod]
    [TestCategory("Storage.RealTimeSynchronization")]
    public void BeginSynchronization_shouldReactToRealTimeEvents() {
      MockedTransport transport = new MockedTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      FontStorage storage = new FontStorage(transport, installer, TestPath);

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
        storage.ActivateFont(TestData.Font1_Description.UID);
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
      Assert.AreEqual(InstallationScope.All, installer.FontInstallationScope(TestData.Font1_Description.UID),
        "BeginSynchronization should install activated fonts for process and user");
    }

    [TestMethod]
    [TestCategory("Storage.RealTimeSynchronization")]
    public void EndSynchronization_shouldNotReactToRealTimeEventsAfterward() {
      MockedTransport transport = new MockedTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      FontStorage storage = new FontStorage(transport, installer, TestPath);

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
        storage.ActivateFont(TestData.Font1_Description.UID);
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
      MockedTransport transport = new MockedTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      FontStorage storage = new FontStorage(transport, installer, TestPath);

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
      storage.ActivateFont(TestData.Font1_Description.UID);

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
      MockedTransport transport = new MockedTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      FontStorage storage = new FontStorage(transport, installer, TestPath);

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
      storage.ActivateFont(TestData.Font1_Description.UID);
      storage.DeactivateFont(TestData.Font1_Description.UID);

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
      MockedTransport transport = new MockedTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      FontStorage storage = new FontStorage(transport, installer, TestPath);

      transport.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
        return request.CreateResponse(System.Net.HttpStatusCode.OK, "blabla");
      };

      bool eventTriggered = false;
      storage.OnFontUninstall += (Font font, InstallationScope scope, bool succeed) => {
        if (font.UID == TestData.Font1_Description.UID && scope == InstallationScope.All) {
          eventTriggered = true;
        }
      };

      storage.AddFont(TestData.Font1_Description);
      storage.ActivateFont(TestData.Font1_Description.UID);
      storage.RemoveFont(TestData.Font1_Description.UID);

      AutoResetEvent syncDone = new AutoResetEvent(false);
      storage.SynchronizeWithSystem(delegate {
        syncDone.Set();
      });
      int timeout = 500;
      Assert.IsTrue(syncDone.WaitOne(timeout), "Storage.SynchronizeWithSystem should finish in less than {0} ms", timeout);

      Assert.IsTrue(eventTriggered, "Storage.RemoveFont should trigger an uninstallation in all scopes event for uninstalled font");
    }

    [TestMethod]
    [TestCategory("Storage.Events")]
    public void AddFont_shouldTriggerProcessScopeInstallationEvent() {
      MockedTransport transport = new MockedTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      FontStorage storage = new FontStorage(transport, installer, TestPath);

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
    public void AddFont_shouldTriggerAllScopeUninstallationEvent_whenFontIsUpdated() {
      MockedTransport transport = new MockedTransport();
      MockedFontInstaller installer = new MockedFontInstaller();
      FontStorage storage = new FontStorage(transport, installer, TestPath);

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

      bool uninstallTriggered = false;
      storage.OnFontUninstall += (Font font, InstallationScope scope, bool succeed) => {
        if (font.UID == TestData.Font1_Description.UID && scope == InstallationScope.All) {
          uninstallTriggered = true;
        }
      };
      bool installTriggered = false;
      storage.OnFontInstall += (Font font, InstallationScope scope, bool succeed) => {
        if (font.UID == TestData.Font1_Description.UID && scope == InstallationScope.Process) {
          installTriggered = true;
        }
      };

      storage.AddFont(TestData.Font1_Description2);

      storage.SynchronizeWithSystem(delegate {
        syncDone.Set();
      });
      Assert.IsTrue(syncDone.WaitOne(timeout), "Storage.SynchronizeWithSystem should finish in less than {0} ms", timeout);

      Assert.IsTrue(uninstallTriggered, "Storage.AddFont should trigger an uninstallation in all scope event for updated font");
      Assert.IsTrue(installTriggered, "Storage.AddFont should trigger an installation in process scope event for updated font");
    }

    private string TestPath {
      get {
        string tempDir = Path.GetTempPath();
        return string.Format("{0}{1}\\", tempDir, Guid.NewGuid().ToString());
      }
    }
  }
}