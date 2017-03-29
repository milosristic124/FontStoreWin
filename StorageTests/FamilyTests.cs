using Microsoft.VisualStudio.TestTools.UnitTesting;
using Storage.Data;
using TestUtilities;

namespace Storage.Impl.Tests {
  [TestClass]
  public class FamilyTests {
    [TestMethod]
    [TestCategory("Family.Behavior")]
    public void FamilyAddFont_shouldAddFontToTheFamily() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font = new Font(TestData.Font1_Description);

      family.Add(font);
      Assert.AreEqual(1, family.Fonts.Count, "Family.Add should add the font to the family");
    }

    [TestMethod]
    [TestCategory("Family.Behavior")]
    public void FamilyAddFont_shouldUpdateFontInTheFamily() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font = new Font(TestData.Font1_Description);
      Font updatedFont = new Font(TestData.Font1_Description2);

      family.Add(font);
      family.Add(updatedFont);

      Assert.AreEqual(1, family.Fonts.Count, "Family.Add should not duplicate fonts with the same UID");
      Assert.AreEqual(TestData.Font1_Description2.Name, family.Fonts[0].Name, "Family.Add should update font");
    }

    [TestMethod]
    [TestCategory("Family.Events")]
    public void FamilyAddFont_shouldTriggerAddFontEvent() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font = new Font(TestData.Font1_Description);

      bool eventTriggered = false;
      family.OnFontAdded += delegate {
        eventTriggered = true;
      };

      family.Add(font);
      Assert.IsTrue(eventTriggered, "Family.Add should trigger font added event");
    }

    [TestMethod]
    [TestCategory("Family.Events")]
    public void FamilyAddFont_shouldTriggerRemoveAndAddFontEvents_whenUpdatingFont() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font = new Font(TestData.Font1_Description);
      Font updatedFont = new Font(TestData.Font1_Description2);

      family.Add(font);

      bool addTriggered = false;
      family.OnFontAdded += delegate {
        addTriggered = true;
      };
      bool removeTriggered = false;
      family.OnFontRemoved += delegate {
        removeTriggered = true;
      };

      family.Add(updatedFont);

      Assert.IsTrue(removeTriggered, "Family.Add should trigger font removed event when replacing existing font");
      Assert.IsTrue(addTriggered, "Family.Add should trigger font added event when replacing existing font");
    }

    [TestMethod]
    [TestCategory("Family.Behavior")]
    public void FamilyRemoveFont_shouldRemoveFontFromTheFamily() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font = new Font(TestData.Font1_Description);
      family.Add(font);

      family.Remove(font.UID);
      Assert.AreEqual(0, family.Fonts.Count, "Family.Remove should remove font from family");
    }

    [TestMethod]
    [TestCategory("Family.Events")]
    public void FamilyRemoveFont_shouldTriggerRemovedFontEvent() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font = new Font(TestData.Font1_Description);
      family.Add(font);

      bool eventTriggered = false;
      family.OnFontRemoved += delegate {
        eventTriggered = true;
      };

      family.Remove(font.UID);
      Assert.IsTrue(eventTriggered, "Family.Remove should trigger font removed event");
    }

    [TestMethod]
    [TestCategory("Family.Behavior")]
    public void FamilyRemoveFont_shouldDoNothing_whenFontDoesNotExist() {
      Family family = new Family(TestData.Font1_Description.FamilyName);

      bool eventTriggered = false;
      family.OnFontRemoved += delegate {
        eventTriggered = true;
      };

      family.Remove(TestData.Font1_Description.UID);
      Assert.IsFalse(eventTriggered, "Family.Remove should not trigger font removed event when no font were removed");
    }

    [TestMethod]
    [TestCategory("Family.Behavior")]
    public void FamilyFindFont_shouldReturnSearchedFont() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font = new Font(TestData.Font1_Description);
      family.Add(font);

      Assert.AreSame(font, family.FindFont(font.UID), "Family.FindFont should return the searched font");
    }

    [TestMethod]
    [TestCategory("Family.Behavior")]
    public void FamilyFindFont_shouldReturnNull_whenFontDoesNotExist() {
      Family family = new Family(TestData.Font1_Description.FamilyName);

      Assert.IsNull(family.FindFont(TestData.Font1_Description.UID), "Family.FindFont should return null when font is not found");
    }

    [TestMethod]
    [TestCategory("Family.Behavior")]
    public void FamilyFullyActivated_shouldBeFalse_whenAtLeastOneFontIsNotActivated() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font1 = new Font(TestData.Font1_Description);
      Font font2 = new Font(TestData.Font2_Description);
      family.Add(font1);
      family.Add(font2);

      font1.Activated = true;
      font2.Activated = false;

      Assert.IsFalse(family.FullyActivated, "Family.FullyActivated should be false when a font is not activated");
    }

    [TestMethod]
    [TestCategory("Family.Behavior")]
    public void FamilyFullyActivated_shouldBeTrue_whenAllFontsAreActivated() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font1 = new Font(TestData.Font1_Description);
      Font font2 = new Font(TestData.Font2_Description);
      family.Add(font1);
      family.Add(font2);

      font1.Activated = true;
      font2.Activated = true;

      Assert.IsTrue(family.FullyActivated, "Family.FullyActivated should be true when all fonts are activated");
    }

    [TestMethod]
    [TestCategory("Family.Behavior")]
    public void FamilyFullyActivatedSet_shouldSetAllFontsActivatedState() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font1 = new Font(TestData.Font1_Description);
      Font font2 = new Font(TestData.Font2_Description);
      family.Add(font1);
      family.Add(font2);

      family.FullyActivated = true;
      Assert.IsTrue(font1.Activated && font2.Activated, "Family.FullyActivated set to True should activate all fonts");

      family.FullyActivated = false;
      Assert.IsTrue(!font1.Activated && !font2.Activated, "Family.FullyActivated set to False should deactivate all fonts");
    }

    [TestMethod]
    [TestCategory("Family.Events")]
    public void Family_shouldTriggerFullyActivatedEvent_whenAllFontsBecomeActivated() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font1 = new Font(TestData.Font1_Description);
      Font font2 = new Font(TestData.Font2_Description);
      family.Add(font1);
      family.Add(font2);

      bool eventTriggered = false;
      family.OnFullyActivatedChanged += delegate {
        eventTriggered = true;
      };

      family.FullyActivated = true;

      Assert.IsTrue(eventTriggered, "Family.FullyActivated should trigger a fully activated change event when set to True");
    }

    [TestMethod]
    [TestCategory("Family.Events")]
    public void Family_shouldTriggerFullyActivatedEvent_whenFullyActivatedStatusIsLost() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font1 = new Font(TestData.Font1_Description);
      Font font2 = new Font(TestData.Font2_Description);
      family.Add(font1);
      family.Add(font2);

      font1.Activated = true;
      font2.Activated = true;

      bool eventTriggered = false;
      family.OnFullyActivatedChanged += delegate {
        eventTriggered = true;
      };

      font2.Activated = false;
      Assert.IsTrue(eventTriggered, "Family.FullyActivated status change should trigger a fully activated change event");
    }

    [TestMethod]
    [TestCategory("Family.Events")]
    public void Family_shouldTriggerFontActivatedEvent_whenChildFontActivationStatusChange() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font = new Font(TestData.Font1_Description);
      family.Add(font);

      bool eventTriggered = false;
      family.OnActivationChanged += delegate {
        eventTriggered = true;
      };

      font.Activated = true;
      Assert.IsTrue(eventTriggered, "Family should trigger font activation events when font activation status change");
    }
  }
}
