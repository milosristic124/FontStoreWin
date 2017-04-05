using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing.Text;
using System.IO;
using System.Linq;

namespace FontInstaller.Impl.Test {
  [TestClass]
  public class FontInstallerImplTests {
    #region data
    private static readonly string FontUID = "test_uid";
    private static readonly string FamilyName = "Eurosoft";
    private MemoryStream FontData {
      get {
        return new MemoryStream(Properties.Resources.eurosoft_regular);
      }
    }
    #endregion

    [TestInitialize]
    public void Initialize() {
      FontInstaller installer = new FontInstaller();
      FontAPIResult res;
      res = installer.UninstallFont(FontUID, InstallationScope.Process).Result;
      Console.WriteLine("[TestInitialize] Process uninstall: {0}", res);
      res = installer.UninstallFont(FontUID, InstallationScope.User).Result;
      Console.WriteLine("[TestInitialize] User uninstall: {0}", res);
    }

    [TestCleanup]
    public void TearDown() {
      FontInstaller installer = new FontInstaller();
      FontAPIResult res;
      res = installer.UninstallFont(FontUID, InstallationScope.Process).Result;
      Console.WriteLine("[TestCleanup] Process uninstall: {0}", res);
      res = installer.UninstallFont(FontUID, InstallationScope.User).Result;
      Console.WriteLine("[TestCleanup] User uninstall: {0}", res);
    }

    [TestMethod]
    [TestCategory("FontInstaller.Behavior")]
    public void InstallFont_shouldSucceed_withProcessScope_whenFontIsNotInstalled() {
      FontInstaller installer = new FontInstaller();

      FontAPIResult result = installer.InstallFont(FontUID, InstallationScope.Process, FontData).Result;

      InstalledFontCollection collection = new InstalledFontCollection();
      bool familyExists = collection.Families.Any(family => family.Name == FamilyName);

      Assert.AreEqual(FontAPIResult.Success, result, "Installing a font should succeed");
      Assert.IsFalse(familyExists, "Fonts installed in the Process scope should not be enumerable");
    }

    [TestMethod]
    [TestCategory("FontInstaller.Behavior")]
    public void InstallFont_shouldSucceed_withUserScope_whenFontIsNotInstalled() {
      FontInstaller installer = new FontInstaller();

      FontAPIResult result = installer.InstallFont(FontUID, InstallationScope.User, FontData).Result;

      InstalledFontCollection collection = new InstalledFontCollection();
      bool familyExists = collection.Families.Any(family => family.Name == FamilyName);

      Assert.AreEqual(FontAPIResult.Success, result, "Installing a font should succeed");
      Assert.IsTrue(familyExists, "Fonts installed in the User scope should be enumerable");
    }

    [TestMethod]
    [TestCategory("FontInstaller.Behavior")]
    public void InstallFont_shouldDoNothing_withProcessScope_whenFontIsInstalled() {
      FontInstaller installer = new FontInstaller();
      installer.InstallFont(FontUID, InstallationScope.Process, FontData).Wait();

      FontAPIResult result = installer.InstallFont(FontUID, InstallationScope.Process, FontData).Result;
      Assert.AreEqual(FontAPIResult.Noop, result, "Installing an installed font should do nothing");
    }

    [TestMethod]
    [TestCategory("FontInstaller.Behavior")]
    public void InstallFont_shouldDoNothing_withUserScope_whenFontIsInstalled() {
      FontInstaller installer = new FontInstaller();
      installer.InstallFont(FontUID, InstallationScope.User, FontData).Wait();

      FontAPIResult result = installer.InstallFont(FontUID, InstallationScope.User, FontData).Result;
      Assert.AreEqual(FontAPIResult.Noop, result, "Installing an installed font should do nothing");
    }

    [TestMethod]
    [TestCategory("FontInstaller.Behavior")]
    public void UninstallFont_shouldSucceed_withProcessScope_whenFontIsInstalled() {
      FontInstaller installer = new FontInstaller();
      installer.InstallFont(FontUID, InstallationScope.Process, FontData).Wait();

      FontAPIResult result = installer.UninstallFont(FontUID, InstallationScope.Process).Result;

      InstalledFontCollection collection = new InstalledFontCollection();
      bool familyExists = collection.Families.Any(family => family.Name == FamilyName);

      Assert.AreEqual(FontAPIResult.Success, result, "Uninstalling a font should succeed");
      Assert.IsFalse(familyExists, "Uninstalled fonts should not be enumerable");
    }

    [TestMethod]
    [TestCategory("FontInstaller.Behavior")]
    public void UninstallFont_shouldSucceed_withUserScope_whenFontIsInstalled() {
      FontInstaller installer = new FontInstaller();
      installer.InstallFont(FontUID, InstallationScope.User, FontData).Wait();

      FontAPIResult result = installer.UninstallFont(FontUID, InstallationScope.User).Result;

      InstalledFontCollection collection = new InstalledFontCollection();
      bool familyExists = collection.Families.Any(family => family.Name == FamilyName);

      Assert.AreEqual(FontAPIResult.Success, result, "Uninstalling a font should succeed");
      Assert.IsFalse(familyExists, "Uninstalled fonts should not be enumerable");
    }

    [TestMethod]
    [TestCategory("FontInstaller.Behavior")]
    public void UninstallFont_shouldDoNothing_withProcessScope_whenFontIsNotInstalled() {
      FontInstaller installer = new FontInstaller();

      FontAPIResult result = installer.UninstallFont(FontUID, InstallationScope.Process).Result;
      Assert.AreEqual(FontAPIResult.Noop, result, "Uninstalling an uninstalled font should do nothing");
    }

    [TestMethod]
    [TestCategory("FontInstaller.Behavior")]
    public void UninstallFont_shouldDoNothing_withUserScope_whenFontIsNotInstalled() {
      FontInstaller installer = new FontInstaller();

      FontAPIResult result = installer.UninstallFont(FontUID, InstallationScope.User).Result;
      Assert.AreEqual(FontAPIResult.Noop, result, "Uninstalling an uninstalled font should do nothing");
    }
  }
}
