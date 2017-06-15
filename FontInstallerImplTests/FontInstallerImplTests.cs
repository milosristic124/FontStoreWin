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

    //[TestMethod]
    //[TestCategory("FontInstaller.Behavior")]
    //public void InstallFont_shouldSucceed_whenFontIsNotInstalled() {
    //  FontInstaller installer = new FontInstaller();

    //  FontAPIResult result = installer.InstallFont(FontUID, FontData).Result;

    //  InstalledFontCollection collection = new InstalledFontCollection();
    //  bool familyExists = collection.Families.Any(family => family.Name == FamilyName);

    //  installer.UninstallAllFonts().Wait();
    //  Assert.AreEqual(FontAPIResult.Success, result, "Installing a font should succeed");
    //  Assert.IsTrue(familyExists, "Fonts installed in the User scope should be enumerable");

    //}

    //[TestMethod]
    //[TestCategory("FontInstaller.Behavior")]
    //public void InstallFont_shouldDoNothing_whenFontIsInstalled() {
    //  FontInstaller installer = new FontInstaller();
    //  installer.InstallFont(FontUID, FontData).Wait();

    //  FontAPIResult result = installer.InstallFont(FontUID, FontData).Result;

    //  installer.UninstallAllFonts().Wait();
    //  Assert.AreEqual(FontAPIResult.Noop, result, "Installing an installed font should do nothing");
    //}

    //[TestMethod]
    //[TestCategory("FontInstaller.Behavior")]
    //public void UninstallFont_shouldSucceed_whenFontIsInstalled() {
    //  FontInstaller installer = new FontInstaller();
    //  installer.InstallFont(FontUID, FontData).Wait();

    //  FontAPIResult result = FontAPIResult.Failure;
    //  result = installer.UninstallFont(FontUID).Result;

    //  InstalledFontCollection collection = new InstalledFontCollection();
    //  bool familyExists = collection.Families.Any(family => family.Name == FamilyName);


    //  installer.UninstallAllFonts().Wait();
    //  Assert.AreEqual(FontAPIResult.Success, result, "Uninstalling a font should succeed");
    //  Assert.IsFalse(familyExists, "Uninstalled fonts should not be enumerable");
    //}

    //[TestMethod]
    //[TestCategory("FontInstaller.Behavior")]
    //public void UninstallFont_shouldDoNothing_whenFontIsNotInstalled() {
    //  FontInstaller installer = new FontInstaller();

    //  FontAPIResult result = installer.UninstallFont(FontUID).Result;

    //  installer.UninstallAllFonts().Wait();
    //  Assert.AreEqual(FontAPIResult.Noop, result, "Uninstalling an uninstalled font should do nothing");
    //}
  }
}
