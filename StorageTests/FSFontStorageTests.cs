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
      FSFontStorage storage = new FSFontStorage(TestPath);
      Assert.IsTrue(storage.Families.Count == 0, "FontStorage has no data when created");
    }

    [TestMethod()]
    public void FindFont_shouldReturnNull_whenTheFontDoesNotExist() {
      FSFontStorage storage = new FSFontStorage(TestPath);
      Assert.IsNull(storage.FindFont(TestData.FontDescription1.UID), "FindFont return null for unexisting fonts");
    }

    [TestMethod()]
    public void FindFont_shouldReturnTheSearchedFont() {
      FSFontStorage storage = new FSFontStorage(TestPath);
      storage.Load().Wait();

      storage.AddFont(TestData.FontDescription1);

      Assert.IsNotNull(storage.FindFont(TestData.FontDescription1.UID), "FindFont should find an existing font");
    }

    [TestMethod()]
    public void AddFont_shouldUpdateFontStorage() {
      FSFontStorage storage = new FSFontStorage(TestPath);
      storage.Load().Wait();

      storage.AddFont(TestData.FontDescription1);

      Assert.IsTrue(storage.Families.Count == 1, "AddFont should add a font to the FontStorage");
    }

    [TestMethod()]
    public void AddFont_shouldReplaceObsoleteData() {
      FSFontStorage storage = new FSFontStorage(TestPath);
      storage.Load().Wait();

      storage.AddFont(TestData.FontDescription1);
      storage.AddFont(TestData.FontDescription1_2);

      Assert.AreSame(TestData.FontDescription1_2, storage.Families[0].Fonts[0].Description,
        "AddFont should replace obsolete font in the FontStorage");
    }

    [TestMethod()]
    public void RemoveFont_shouldUpdateFontStorage() {
      FSFontStorage storage = new FSFontStorage(TestPath);
      storage.Load().Wait();

      storage.AddFont(TestData.FontDescription1);
      storage.RemoveFont(TestData.FontDescription1.UID);

      Assert.IsTrue(storage.Families.Count == 0, "RemoveFont should remove font from the FontStorage");
    }

    [TestMethod()]
    public void ActivateFont_shouldActivateFont() {
      FSFontStorage storage = new FSFontStorage(TestPath);
      storage.Load().Wait();

      storage.AddFont(TestData.FontDescription1);
      storage.ActivateFont(TestData.FontDescription1.UID);

      Assert.IsTrue(storage.FindFont(TestData.FontDescription1.UID).Activated, "ActivateFont shoudl activate the corresponding font in the FontStorage");
    }

    [TestMethod()]
    public void DeactivateFont_shouldDeactivateFont() {
      FSFontStorage storage = new FSFontStorage(TestPath);
      storage.Load().Wait();

      storage.AddFont(TestData.FontDescription1);
      storage.ActivateFont(TestData.FontDescription1.UID);
      storage.DeactivateFont(TestData.FontDescription1.UID);

      Assert.IsFalse(storage.FindFont(TestData.FontDescription1.UID).Activated, "DeactivateFont should deactivate the corresponding font in the FontStorage");
    }

    [TestMethod()]
    public void Load_shouldLoadSavedData() {
      string storagePath = TestPath;
      FSFontStorage storage = new FSFontStorage(storagePath);

      storage.Load().Wait();
      storage.AddFont(TestData.FontDescription1);
      storage.ActivateFont(TestData.FontDescription1.UID);
      storage.Save().Wait();

      storage = new FSFontStorage(storagePath);

      int timeout = 5000;
      bool signaled = storage.Load().Wait(timeout);

      Assert.IsTrue(signaled, "Load should not timeout during tests...");
      Assert.IsTrue(storage.Families.Count == 1, "Load should load saved catalog data from file system");
      Assert.IsTrue(storage.FindFont(TestData.FontDescription1.UID).Activated, "Load should load saved fonts data from file system");
    }

    private string TestPath {
      get {
        string tempDir = Path.GetTempPath();
        return string.Format("{0}{1}\\", tempDir, Guid.NewGuid().ToString());
      }
    }
  }
}