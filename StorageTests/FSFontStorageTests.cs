using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using TestUtilities;

namespace Storage.Impl.Tests {
  [TestClass]
  public class FSFontStorageTests {
    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void FontStorage_shouldBeEmpty_whenCreated() {
      FSFontStorage storage = new FSFontStorage(TestPath);
      Assert.IsTrue(storage.FamilyCollection.Families.Count == 0, "FontStorage has no data when created");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void FindFont_shouldReturnNull_whenTheFontDoesNotExist() {
      FSFontStorage storage = new FSFontStorage(TestPath);
      Assert.IsNull(storage.FindFont(TestData.Font1_Description.UID), "FindFont return null for unexisting fonts");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void FindFont_shouldReturnTheSearchedFont() {
      FSFontStorage storage = new FSFontStorage(TestPath);
      storage.Load().Wait();

      storage.AddFont(TestData.Font1_Description);

      Assert.IsNotNull(storage.FindFont(TestData.Font1_Description.UID), "FindFont should find an existing font");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void AddFont_shouldUpdateFontStorage() {
      FSFontStorage storage = new FSFontStorage(TestPath);
      storage.Load().Wait();

      storage.AddFont(TestData.Font1_Description);

      Assert.IsTrue(storage.FamilyCollection.Families.Count == 1, "AddFont should add a font to the FontStorage");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void AddFont_shouldReplaceObsoleteData() {
      FSFontStorage storage = new FSFontStorage(TestPath);
      storage.Load().Wait();

      storage.AddFont(TestData.Font1_Description);
      storage.AddFont(TestData.Font1_Description2);

      Assert.AreSame(TestData.Font1_Description2, storage.FamilyCollection.Families[0].Fonts[0].Description,
        "AddFont should replace obsolete font in the FontStorage");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void RemoveFont_shouldUpdateFontStorage() {
      FSFontStorage storage = new FSFontStorage(TestPath);
      storage.Load().Wait();

      storage.AddFont(TestData.Font1_Description);
      storage.RemoveFont(TestData.Font1_Description.UID);

      Assert.IsTrue(storage.FamilyCollection.Families.Count == 0, "RemoveFont should remove font from the FontStorage");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void ActivateFont_shouldActivateFont() {
      FSFontStorage storage = new FSFontStorage(TestPath);
      storage.Load().Wait();

      storage.AddFont(TestData.Font1_Description);
      storage.ActivateFont(TestData.Font1_Description.UID);

      Assert.IsTrue(storage.FindFont(TestData.Font1_Description.UID).Activated, "ActivateFont shoudl activate the corresponding font in the FontStorage");
    }

    [TestMethod]
    [TestCategory("Storage.Behavior")]
    public void DeactivateFont_shouldDeactivateFont() {
      FSFontStorage storage = new FSFontStorage(TestPath);
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
      FSFontStorage storage = new FSFontStorage(storagePath);

      storage.Load().Wait();
      storage.AddFont(TestData.Font1_Description);
      storage.ActivateFont(TestData.Font1_Description.UID);
      storage.Save().Wait();

      storage = new FSFontStorage(storagePath);

      int timeout = 5000;
      bool signaled = storage.Load().Wait(timeout);

      Assert.IsTrue(signaled, "Load should not timeout during tests...");
      Assert.IsTrue(storage.FamilyCollection.Families.Count == 1, "Load should load saved catalog data from file system");
      Assert.IsTrue(storage.FindFont(TestData.Font1_Description.UID).Activated, "Load should load saved fonts data from file system");
    }

    private string TestPath {
      get {
        string tempDir = Path.GetTempPath();
        return string.Format("{0}{1}\\", tempDir, Guid.NewGuid().ToString());
      }
    }
  }
}