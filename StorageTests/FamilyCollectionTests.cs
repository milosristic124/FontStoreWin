using Microsoft.VisualStudio.TestTools.UnitTesting;
using Storage.Data;
using Storage.Impl.Tests.Utilities;
using TestUtilities;

namespace Storage.Impl.Tests {
  [TestClass]
  public class FamilyCollectionTests {
    [TestMethod]
    [TestCategory("FamilyCollection.Behavior")]
    public void FamilyCollectionClear_shouldClearAllFamilies() {
      FamilyCollection collection = NewCollection();

      int familyCount = collection.Families.Count;
      collection.Clear();
      Assert.AreNotEqual(familyCount, collection.Families.Count, "FamilyCollection.Clear should clear the family collection");
    }

    [TestMethod]
    [TestCategory("FamilyCollection.Events")]
    public void FamilyCollectionClear_shouldTriggerClearEvent() {
      FamilyCollection collection = NewCollection();

      bool eventTriggered = false;
      collection.OnCollectionCleared += delegate {
        eventTriggered = true;
      };

      collection.Clear();
      Assert.IsTrue(eventTriggered, "FamilyCollection.Clear should trigger collection cleared event");
    }

    [TestMethod]
    [TestCategory("FamilyCollection.Behavior")]
    public void FamilyCollectionFindFont_shouldReturnSearchedFont() {
      FamilyCollection collection = NewCollection();
      Font font = collection.Families[0].Fonts[0];

      Assert.AreSame(font, collection.FindFont(font.UID), "FamilyCollection.FindFont should return searched font");
    }

    [TestMethod]
    [TestCategory("FamilyCollection.Behavior")]
    public void FamilyCollectionFindFont_shouldReturnNull_whenFontDoesNotExist() {
      FamilyCollection collection = new FamilyCollection();

      Assert.IsNull(collection.FindFont(TestData.Font1_Description.UID), "FamilyCollection.FindFont should return null when the font is not found");
    }

    [TestMethod]
    [TestCategory("FamilyCollection.Behavior")]
    public void FamilyCollectionAddFont_shouldCreateNewFamily() {
      FamilyCollection collection = new FamilyCollection();
      Font font = Factory.CreateFont(TestData.Font1_Description);

      collection.AddFont(font);
      Assert.AreEqual(1, collection.Families.Count, "FamilyCollection.AddFont should create new family");
    }

    [TestMethod]
    [TestCategory("FamilyCollection.Behavior")]
    public void FamilyCollectionAddFont_shouldNotCreateNewFamily_whenFamilyAlreadyExists() {
      FamilyCollection collection = new FamilyCollection();
      Font font1 = Factory.CreateFont(TestData.Font1_Description);
      Font font3 = Factory.CreateFont(TestData.Font3_Description);

      collection.AddFont(font1).AddFont(font3);
      Assert.AreEqual(1, collection.Families.Count, "FamilyCollection.AddFont should not duplicate a family");
    }

    [TestMethod]
    [TestCategory("FamilyCollection.Events")]
    public void FamilyCollectionAddFont_shouldTriggerFamilyAddedEvent_whenFamilyIsCreated() {
      FamilyCollection collection = new FamilyCollection();
      Font font = Factory.CreateFont(TestData.Font1_Description);

      bool eventTriggered = false;
      collection.OnFamilyAdded += delegate {
        eventTriggered = true;
      };

      collection.AddFont(font);
      Assert.IsTrue(eventTriggered, "FamilyCollection.AddFont should trigger family added event when a family is created");
    }

    [TestMethod]
    [TestCategory("FamilyCollection.Behavior")]
    public void FamilyCollectionRemoveFont_shouldRemoveFamily_whenFamilyIsEmpty() {
      FamilyCollection collection = new FamilyCollection();
      Font font = Factory.CreateFont(TestData.Font1_Description);
      collection.AddFont(font);

      collection.RemoveFont(font.UID);
      Assert.AreEqual(0, collection.Families.Count, "FamilyCollection.RemoveFont should remove empty family");
    }

    [TestMethod]
    [TestCategory("FamilyCollection.Behavior")]
    public void FamilyCollectionRemoveFont_shouldNotRemoveFamily_whenFamilyIsNotEmpty() {
      FamilyCollection collection = new FamilyCollection();
      Font font = Factory.CreateFont(TestData.Font1_Description);
      Font font3 = Factory.CreateFont(TestData.Font3_Description);
      collection.AddFont(font).AddFont(font3);

      collection.RemoveFont(font.UID);
      Assert.AreEqual(1, collection.Families.Count, "FamilyCollection.RemoveFont should not remove non empty family");
    }

    [TestMethod]
    [TestCategory("FamilyCollection.Events")]
    public void FamilyCollectionRemoveFont_shouldTriggerFamilyRemovedEvent_whenFamilyIsRemoved() {
      FamilyCollection collection = new FamilyCollection();
      Font font = Factory.CreateFont(TestData.Font1_Description);
      collection.AddFont(font);

      bool eventTriggered = false;
      collection.OnFamilyRemoved += delegate {
        eventTriggered = true;
      };

      collection.RemoveFont(font.UID);
      Assert.IsTrue(eventTriggered, "FamilyCollection.RemoveFont should trigger family removed event when a family is removed");
    }

    [TestMethod]
    [TestCategory("FamilyCollection.Events")]
    public void FamilyCollection_shouldTriggerFontActivatedEvent_whenFontActivationStatusChange() {
      FamilyCollection collection = new FamilyCollection();
      Font font = Factory.CreateFont(TestData.Font1_Description);
      collection.AddFont(font);

      bool eventTriggered = false;
      collection.OnActivationChanged += delegate {
        eventTriggered = true;
      };

      font.Activated = true;

      Assert.IsTrue(eventTriggered, "FamilyCollection should trigger font activation event when a font activation status change");
    }

    [TestMethod]
    [TestCategory("FamilyCollection.Events")]
    public void FamilyCollection_shouldTriggerFontInstallationEvent_whenFontInstallationStatusChange() {
      FamilyCollection collection = new FamilyCollection();
      Font font = Factory.CreateFont(TestData.Font1_Description);
      collection.AddFont(font);

      bool eventTriggered = false;
      collection.OnInstallationChanged += delegate {
        eventTriggered = true;
      };

      font.IsInstalled = true;

      Assert.IsTrue(eventTriggered, "FamilyCollection should trigger font installation event when a font installation status change");
    }

    [TestMethod]
    [TestCategory("FamilyCollection.Events")]
    public void FamilyCollection_shouldTriggerAddFontEvent_whenFontIsAddedToFamily() {
      FamilyCollection collection = new FamilyCollection();
      Font font = Factory.CreateFont(TestData.Font1_Description);

      bool eventTriggered = false;
      collection.OnFontAdded += delegate {
        eventTriggered = true;
      };
      collection.AddFont(font);

      Assert.IsTrue(eventTriggered, "FamilyCollection should trigger font added event when fonts are added to the collection");
    }

    [TestMethod]
    [TestCategory("FamilyCollection.Events")]
    public void FamilyCollection_shouldTriggerRemovedFontEvent_whenFontIsRemovedFromFamily() {
      FamilyCollection collection = new FamilyCollection();
      Font font = Factory.CreateFont(TestData.Font1_Description);
      collection.AddFont(font);

      bool eventTriggered = false;
      collection.OnFontRemoved += delegate {
        eventTriggered = true;
      };
      collection.RemoveFont(font.UID);

      Assert.IsTrue(eventTriggered, "FamilyCollection should trigger font removed event when fonts are removed from the collection");
    }

    [TestMethod]
    [TestCategory("FamilyCollection.Events")]
    public void FamilyCollection_shouldTriggerUpdatedFontEvent_whenFontIsUpdated() {
      FamilyCollection collection = new FamilyCollection();
      Font font = Factory.CreateFont(TestData.Font1_Description);
      Font updatedFont = Factory.CreateFont(TestData.Font1_Description2);
      collection.AddFont(font);

      bool eventTriggered = false;
      collection.OnFontUpdated += delegate {
        eventTriggered = true;
      };
      collection.AddFont(updatedFont);

      Assert.IsTrue(eventTriggered, "FamilyCollection should trigger font removed event when fonts are removed from the collection");
    }

    #region setup methods
    private FamilyCollection NewCollection() {
      FamilyCollection collection = new FamilyCollection();

      Font font1 = Factory.CreateFont(TestData.Font1_Description);
      Font font2 = Factory.CreateFont(TestData.Font2_Description);

      collection.AddFont(font1).AddFont(font2);

      return collection;
    }
    #endregion
  }
}
