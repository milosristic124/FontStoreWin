using Microsoft.VisualStudio.TestTools.UnitTesting;
using Storage.Data;
using Storage.Impl.Tests.Utilities;
using TestUtilities;

namespace Storage.Impl.Tests {
  [TestClass]
  public class FontTests {
    [TestMethod]
    [TestCategory("Font.Events")]
    public void FontActivation_shouldTriggerActivationEvent() {
      Font font = Factory.CreateFont(TestData.Font1_Description);

      bool eventTriggered = false;
      font.OnActivationChanged += delegate {
        eventTriggered = true;
      };

      font.Activated = !font.Activated;

      Assert.IsTrue(eventTriggered, "Font.Activated status change should trigger a font activation event");
    }

    [TestMethod]
    [TestCategory("Font.Events")]
    public void FontInstallation_shouldTriggerInstallationEvent() {
      Font font = Factory.CreateFont(TestData.Font1_Description);

      bool eventTriggered = false;
      font.OnInstallationChanged += delegate {
        eventTriggered = true;
      };

      font.IsInstalled = true;

      Assert.IsTrue(eventTriggered, "Font.IsInstalled status change should trigger a font installation event");
    }
  }
}
