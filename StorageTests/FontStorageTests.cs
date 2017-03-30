using FontInstaller;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;
using TestUtilities;
using TestUtilities.Protocol;
using System.Threading.Tasks;
using System.Collections.Generic;

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
      FontStorage storage = new FontStorage(transport, installer);

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

      installer.OnInstallRequest += (string uid, InstallationScope scope) => {
        return true;
      };

      AutoResetEvent evt = new AutoResetEvent(false);
      storage.SynchronizeWithSystem(delegate {
        evt.Set();
      });

      int timeout = 10000;
      if (!evt.WaitOne(/*timeout*/)) {
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

    public void SynchronizeWithSystem_shouldNotReactToRealTimeEventsAfterward() {
    }

    public void BeginSynchronization_shouldDownloadAndInstallFonts() {
    }

    public void BeginSynchronization_shouldReactToRealTimeEvents() {
    }

    public void EndSynchronization_shouldNotReactToRealTimeEventsAfterward() {
    }

    private string TestPath {
      get {
        string tempDir = Path.GetTempPath();
        return string.Format("{0}{1}\\", tempDir, Guid.NewGuid().ToString());
      }
    }
  }

  public class MockedFontInstaller : CallTracer, IFontInstaller {
    #region private data
    private Dictionary<string, InstallationScope> _installedFonts;
    #endregion

    #region ctor
    public MockedFontInstaller() {
      _installedFonts = new Dictionary<string, InstallationScope>();
    }
    #endregion

    #region test
    public delegate bool InstallationRequestHandler(string uid, InstallationScope scope);
    public event InstallationRequestHandler OnInstallRequest;
    public InstallationScope? FontInstallationScope(string uid) {
      if (_installedFonts.ContainsKey(uid)) {
        return _installedFonts[uid];
      }
      return null;
    }
    #endregion

    #region methods
    public Task InstallFont(string uid, InstallationScope scope, MemoryStream fontData) {
      RegisterCall("InstallFont");
      return Task.Factory.StartNew(() => {
        bool? shouldInstall = OnInstallRequest?.Invoke(uid, scope);
        if (shouldInstall.HasValue && !shouldInstall.Value) {
          return;
        }

        if (_installedFonts.ContainsKey(uid) && _installedFonts[uid] != scope) {
          _installedFonts[uid] = InstallationScope.All;
        } else {
          _installedFonts[uid] = scope;
        }
      });
    }

    public Task UnsintallFont(string uid, InstallationScope scope) {
      RegisterCall("UnsintallFont");
      return Task.Factory.StartNew(() => {
        if (_installedFonts.ContainsKey(uid)) {
          if (scope == InstallationScope.All || _installedFonts[uid] == scope) {
            _installedFonts.Remove(uid);
          } else if (scope == InstallationScope.User) {
            _installedFonts[uid] = InstallationScope.Process;
          } else {
            _installedFonts[uid] = InstallationScope.User;
          }
        }
      });
    }
    #endregion
  }
}