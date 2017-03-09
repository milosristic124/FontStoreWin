using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TestUtilities;
using TestUtilities.Protocol;

namespace Storage.Impl.Tests {
  [TestClass()]
  public class FSFontStorageTests {
    [TestMethod()]
    public void FontStorage_shouldBeEmpty_whenCreated() {
      MockedConnection connection = new MockedConnection(true);
      FSFontStorage storage = new FSFontStorage(connection, TestPath);

      Assert.IsTrue(storage.Families.Count == 0, "FontStorage has no data when created");
    }

    [TestMethod()]
    public void FindFont_shouldReturnNull_whenTheFontDoesNotExist() {
      MockedConnection connection = new MockedConnection(true);
      FSFontStorage storage = new FSFontStorage(connection, TestPath);

      Assert.IsNull(storage.FindFont(TestData.FontDescription1.UID), "FindFont return null for unexisting fonts");
    }

    [TestMethod()]
    public void ReceivingFontDescription_shouldUpdateFontStorage() {
      MockedConnection connection = new MockedConnection(true);
      FSFontStorage storage = new FSFontStorage(connection, TestPath);
      storage.Load().Wait();
      storage.StartUpdate();
      connection.SimulateEvent(MockedConnection.ConnectionEvents.FontDescriptionReceived, TestData.FontDescription1);

      Assert.IsTrue(storage.Families.Count == 1, "Triggering a FontDescriptionReceived event updates the FontStorage");
    }

    [TestMethod()]
    public void ReceivingFontDescription_shouldReplaceObsoleteData() {
      MockedConnection connection = new MockedConnection(true);
      FSFontStorage storage = new FSFontStorage(connection, TestPath);
      storage.Load().Wait();
      storage.StartUpdate();
      connection.SimulateEvent(MockedConnection.ConnectionEvents.FontDescriptionReceived, TestData.FontDescription1);
      connection.SimulateEvent(MockedConnection.ConnectionEvents.FontDescriptionReceived, TestData.FontDescription1_2);

      Assert.AreSame(TestData.FontDescription1_2, storage.Families[0].Fonts[0].Description,
        "Triggering a FontDescriptionReceived event updates obsolete data of the FontStorage");
    }

    [TestMethod()]
    public void ReceivingFontDeletion_shouldUpdateFontStorage() {
      MockedConnection connection = new MockedConnection(true);
      FSFontStorage storage = new FSFontStorage(connection, TestPath);
      storage.Load().Wait();
      storage.StartUpdate();

      connection.SimulateEvent(MockedConnection.ConnectionEvents.FontDescriptionReceived, TestData.FontDescription1);
      connection.SimulateEvent(MockedConnection.ConnectionEvents.FontDeleted, TestData.FontDescription1.UID);

      Assert.IsTrue(storage.Families.Count == 0, "Triggering a FontDeleted event updates the FontStorage");
    }

    [TestMethod()]
    public void FindFont_shouldReturnTheSearchedFont() {
      MockedConnection connection = new MockedConnection(true);
      FSFontStorage storage = new FSFontStorage(connection, TestPath);
      storage.Load().Wait();
      storage.StartUpdate();
      connection.SimulateEvent(MockedConnection.ConnectionEvents.FontDescriptionReceived, TestData.FontDescription1);

      Assert.IsNotNull(storage.FindFont(TestData.FontDescription1.UID), "FindFont find an existing font");
    }

    [TestMethod()]
    public void ReceivingFontActivation_shouldUpdateFontStorate() {
      MockedConnection connection = new MockedConnection(true);
      FSFontStorage storage = new FSFontStorage(connection, TestPath);
      storage.Load().Wait();
      storage.StartUpdate();

      connection.SimulateEvent(MockedConnection.ConnectionEvents.FontDescriptionReceived, TestData.FontDescription1);
      connection.SimulateEvent(MockedConnection.ConnectionEvents.FontActivated, TestData.FontDescription1.UID);

      Assert.IsTrue(storage.FindFont(TestData.FontDescription1.UID).Activated, "Trigger a FontActivation event updates the FontStorage");
    }

    [TestMethod()]
    public void ReceivingFontDeactivation_shouldUpdateFontStorate() {
      MockedConnection connection = new MockedConnection(true);
      FSFontStorage storage = new FSFontStorage(connection, TestPath);
      storage.Load().Wait();
      storage.StartUpdate();

      connection.SimulateEvent(MockedConnection.ConnectionEvents.FontDescriptionReceived, TestData.FontDescription1);
      connection.SimulateEvent(MockedConnection.ConnectionEvents.FontActivated, TestData.FontDescription1.UID);
      connection.SimulateEvent(MockedConnection.ConnectionEvents.FontDeactivated, TestData.FontDescription1.UID);

      Assert.IsFalse(storage.FindFont(TestData.FontDescription1.UID).Activated, "Trigger a FontDeactivation event updates the FontStorage");
    }

    [TestMethod()]
    public void Load_shouldLoadSavedData() {
      MockedConnection connection = new MockedConnection(true);
      string storagePath = TestPath;
      FSFontStorage storage = new FSFontStorage(connection, storagePath);
      AutoResetEvent evt = new AutoResetEvent(false);

      storage.OnUpdateFinished += () => {
        evt.Set();
      };

      storage.Load().Wait();
      storage.StartUpdate();
      connection.SimulateEvent(MockedConnection.ConnectionEvents.FontDescriptionReceived, TestData.FontDescription1);
      connection.SimulateEvent(MockedConnection.ConnectionEvents.UpdateFinished);
      connection.SimulateEvent(MockedConnection.ConnectionEvents.FontActivated, TestData.FontDescription1.UID);
      connection.SimulateEvent(MockedConnection.ConnectionEvents.UpdateFinished);

      evt.WaitOne();

      storage = new FSFontStorage(connection, storagePath);

      int timeout = 5000;
      bool signaled = storage.Load().Wait(timeout);

      Assert.IsTrue(signaled, "Load should not timeout during tests...");
      Assert.IsTrue(storage.Families.Count == 1, "Load should load saved catalog data from file system");
      Assert.IsTrue(storage.FindFont(TestData.FontDescription1.UID).Activated, "Load should load saved fonts data from file system");
    }

    [TestMethod()]
    public void StartUpdate_shouldLoadDataFromServer() {
      MockedConnection connection = new MockedConnection(true);
      FSFontStorage storage = new FSFontStorage(connection, TestPath);
      storage.Load().Wait();

      AutoResetEvent updateDone = new AutoResetEvent(false);

      storage.OnUpdateFinished += () => {
        updateDone.Set();
      };

      storage.StartUpdate();

      Task.Run(() => {
        connection.SimulateEvent(MockedConnection.ConnectionEvents.FontDescriptionReceived, TestData.FontDescription1_2);
        connection.SimulateEvent(MockedConnection.ConnectionEvents.FontDescriptionReceived, TestData.FontDescription2);
        connection.SimulateEvent(MockedConnection.ConnectionEvents.UpdateFinished);
        connection.SimulateEvent(MockedConnection.ConnectionEvents.FontActivated, TestData.FontDescription1.UID);
        connection.SimulateEvent(MockedConnection.ConnectionEvents.UpdateFinished);
      }).Wait();

      int timeout = 500;
      bool  finishedInTime = updateDone.WaitOne(timeout);
      if (finishedInTime) {
        connection.Verify("UpdateCatalog", 1);
        connection.Verify("UpdateFontsStatus", 1);

        Assert.AreSame(TestData.FontDescription1_2, storage.Families[0].Fonts[0].Description,
          "StartUpdate update catalog data from server");
        Assert.IsTrue(storage.Families.Count == 2, "StartUpdate load new catalog data from server");
        Assert.IsTrue(storage.FindFont(TestData.FontDescription1.UID).Activated, "StartUpdate update font data from server");
      } else {
        Assert.Inconclusive("StartUpdate timed-out after {0}ms.", timeout);
      }
    }

    private string TestPath {
      get {
        string tempDir = Path.GetTempPath();
        return string.Format("{0}{1}\\", tempDir, Guid.NewGuid().ToString());
      }
    }
  }
}