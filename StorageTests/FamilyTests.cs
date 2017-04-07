using Microsoft.VisualStudio.TestTools.UnitTesting;
using Storage.Data;
using Storage.Impl.Tests.Utilities;
using TestUtilities;

namespace Storage.Impl.Tests {
  [TestClass]
  public class FamilyTests {
    [TestMethod]
    [TestCategory("Family.Behavior")]
    public void FamilyAddFont_shouldAddFontToTheFamily() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font = Factory.CreateFont(TestData.Font1_Description);

      family.Add(font);
      Assert.AreEqual(1, family.Fonts.Count, "Family.Add should add the font to the family");
    }

    [TestMethod]
    [TestCategory("Family.Behavior")]
    public void FamilyAddFont_shouldUpdateFontInTheFamily() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font = Factory.CreateFont(TestData.Font1_Description);
      Font updatedFont = Factory.CreateFont(TestData.Font1_Description2);

      family.Add(font);
      family.Add(updatedFont);

      Assert.AreEqual(1, family.Fonts.Count, "Family.Add should not duplicate fonts with the same UID");
      Assert.AreEqual(TestData.Font1_Description2.Name, family.Fonts[0].Name, "Family.Add should update font");
    }

    [TestMethod]
    [TestCategory("Family.Behavior")]
    public void FamilyAddFont_shouldTranferActivatedState_whenUpdatingFonts() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font = Factory.CreateFont(TestData.Font1_Description);
      Font updatedFont = Factory.CreateFont(TestData.Font1_Description2);

      font.Activated = true;

      family.Add(font);
      family.Add(updatedFont);

      Assert.IsTrue((family.FindFont(TestData.Font1_Description.UID)?.Activated).Value,
        "AddFont should transfer activated state to updated font when updating a font");
    }

    [TestMethod]
    [TestCategory("Family.Events")]
    public void FamilyAddFont_shouldTriggerAddFontEvent() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font = Factory.CreateFont(TestData.Font1_Description);

      bool eventTriggered = false;
      family.OnFontAdded += delegate {
        eventTriggered = true;
      };

      family.Add(font);
      Assert.IsTrue(eventTriggered, "Family.Add should trigger font added event");
    }

    [TestMethod]
    [TestCategory("Family.Events")]
    public void FamilyAddFont_shouldTriggerUpdateFontEvents_whenUpdatingFont() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font = Factory.CreateFont(TestData.Font1_Description);
      Font updatedFont = Factory.CreateFont(TestData.Font1_Description2);

      family.Add(font);

      bool addTriggered = false;
      family.OnFontAdded += delegate {
        addTriggered = true;
      };
      bool removeTriggered = false;
      family.OnFontRemoved += delegate {
        removeTriggered = true;
      };
      bool updateTriggered = false;
      family.OnFontUpdated += delegate {
        updateTriggered = true;
      };

      family.Add(updatedFont);

      Assert.IsFalse(removeTriggered, "Family.Add should not trigger font removed event when replacing existing font");
      Assert.IsFalse(addTriggered, "Family.Add should not trigger font added event when replacing existing font");
      Assert.IsTrue(updateTriggered, "Family.Add should trigger font updated event when replacing existing font");
    }

    [TestMethod]
    [TestCategory("Family.Behavior")]
    public void FamilyRemoveFont_shouldRemoveFontFromTheFamily() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font = Factory.CreateFont(TestData.Font1_Description);
      family.Add(font);

      family.Remove(font.UID);
      Assert.AreEqual(0, family.Fonts.Count, "Family.Remove should remove font from family");
    }

    [TestMethod]
    [TestCategory("Family.Events")]
    public void FamilyRemoveFont_shouldTriggerRemovedFontEvent() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font = Factory.CreateFont(TestData.Font1_Description);
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
      Font font = Factory.CreateFont(TestData.Font1_Description);
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
      Font font1 = Factory.CreateFont(TestData.Font1_Description);
      Font font2 = Factory.CreateFont(TestData.Font2_Description);
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
      Font font1 = Factory.CreateFont(TestData.Font1_Description);
      Font font2 = Factory.CreateFont(TestData.Font2_Description);
      family.Add(font1);
      family.Add(font2);

      font1.Activated = true;
      font2.Activated = true;

      Assert.IsTrue(family.FullyActivated, "Family.FullyActivated should be true when all fonts are activated");
    }

    [TestMethod]
    [TestCategory("Family.Behavior")]
    public void FamilyFullyActivatedSet_shouldTriggerRequestActivationForAllChildFonts() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font1 = Factory.CreateFont(TestData.Font1_Description);
      Font font2 = Factory.CreateFont(TestData.Font2_Description);
      family.Add(font1);
      family.Add(font2);

      int activationRequests = 0;
      font1.OnActivationRequest += delegate {
        activationRequests += 1;
      };
      font2.OnActivationRequest += delegate {
        activationRequests += 1;
      };
      int deactivationRequests = 0;
      font1.OnDeactivationRequest += delegate {
        deactivationRequests += 1;
      };
      font2.OnDeactivationRequest += delegate {
        deactivationRequests += 1;
      };

      family.FullyActivated = true;
      Assert.AreEqual(family.Fonts.Count, activationRequests, "Family.FullyActivated set should request fonts activation");

      family.FullyActivated = false;
      Assert.AreEqual(family.Fonts.Count, deactivationRequests, "Family.FullyActivated unset should request fonts deactivation");
    }

    [TestMethod]
    [TestCategory("Family.Events")]
    public void Family_shouldTriggerFullyActivatedEvent_whenFullyActivatedStatusIsLost() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font1 = Factory.CreateFont(TestData.Font1_Description);
      Font font2 = Factory.CreateFont(TestData.Font2_Description);
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
      Font font = Factory.CreateFont(TestData.Font1_Description);
      family.Add(font);

      bool eventTriggered = false;
      family.OnActivationChanged += delegate {
        eventTriggered = true;
      };

      font.Activated = true;
      Assert.IsTrue(eventTriggered, "Family should trigger font activation events when font activation status change");
    }

    [TestMethod]
    [TestCategory("Family.Events")]
    public void Family_shouldtriggerFontInstallationEvent_whenChildFontInstallationStatusChange() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font = Factory.CreateFont(TestData.Font1_Description);
      family.Add(font);

      bool eventTriggered = false;
      family.OnInstallationChanged += delegate {
        eventTriggered = true;
      };

      font.IsInstalled = true;
      Assert.IsTrue(eventTriggered, "Family should trigger font installation events when font installation status change");
    }

    [TestMethod]
    [TestCategory("Family.Events")]
    public void Family_shouldTriggerFontActivationRequest_whenChildFontTriggerActivationRequest() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font = Factory.CreateFont(TestData.Font1_Description);
      family.Add(font);

      bool eventTriggered = false;
      family.OnActivationRequest += delegate {
        eventTriggered = true;
      };

      font.RequestActivation();
      Assert.IsTrue(eventTriggered, "Family should trigger font activation request events when font request activation event is triggered");
    }

    [TestMethod]
    [TestCategory("Family.Events")]
    public void Family_shouldTriggerFontDeactivationRequest_whenChildFontTriggerDeactivationRequest() {
      Family family = new Family(TestData.Font1_Description.FamilyName);
      Font font = Factory.CreateFont(TestData.Font1_Description);
      family.Add(font);

      bool eventTriggered = false;
      family.OnDeactivationRequest += delegate {
        eventTriggered = true;
      };

      font.RequestDeactivation();
      Assert.IsTrue(eventTriggered, "Family should trigger font deactivation request events when font request deactivation event is triggered");
    }
  }
}
