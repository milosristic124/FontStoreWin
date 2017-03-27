using Microsoft.VisualStudio.TestTools.UnitTesting;
using Storage.Data;
using TestUtilities;

namespace Storage.Impl.Tests {
  [TestClass]
  public class FamilyCollectionTests {
    [TestMethod]
    public void FamilyCollectionClear_shouldClearAllFamilies() {
      FamilyCollection collection = NewCollection();

      int familyCount = collection.Families.Count;
      collection.Clear();
      Assert.AreNotEqual(familyCount, collection.Families.Count, "FamilyCollection.Clear should clear the family collection");
    }

    [TestMethod]
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
    public void FamilyCollectionFindFont_shouldReturnSearchedFont() {
      FamilyCollection collection = NewCollection();
      Font font = collection.Families[0].Fonts[0];

      Assert.AreSame(font, collection.FindFont(font.UID), "FamilyCollection.FindFont should return searched font");
    }

    [TestMethod]
    public void FamilyCollectionFindFont_shouldReturnNull_whenFontDoesNotExist() {
      FamilyCollection collection = new FamilyCollection();

      Assert.IsNull(collection.FindFont(TestData.Font1_Description.UID), "FamilyCollection.FindFont should return null when the font is not found");
    }

    [TestMethod]
    public void FamilyCollectionAddFont_shouldCreateNewFamily() {
      FamilyCollection collection = new FamilyCollection();
      Font font = new Font(TestData.Font1_Description);

      collection.AddFont(font);
      Assert.AreEqual(1, collection.Families.Count, "FamilyCollection.AddFont should create new family");
    }

    [TestMethod]
    public void FamilyCollectionAddFont_shouldNotCreateNewFamily_whenFamilyAlreadyExists() {
      FamilyCollection collection = new FamilyCollection();
      Font font1 = new Font(TestData.Font1_Description);
      Font font3 = new Font(TestData.Font3_Description);

      collection.AddFont(font1).AddFont(font3);
      Assert.AreEqual(1, collection.Families.Count, "FamilyCollection.AddFont should not duplicate a family");
    }

    [TestMethod]
    public void FamilyCollectionAddFont_shouldTriggerFamilyAddedEvent_whenFamilyIsCreated() {
      FamilyCollection collection = new FamilyCollection();
      Font font = new Font(TestData.Font1_Description);

      bool eventTriggered = false;
      collection.OnFamilyAdded += delegate {
        eventTriggered = true;
      };

      collection.AddFont(font);
      Assert.IsTrue(eventTriggered, "FamilyCollection.AddFont should trigger family added event when a family is created");
    }

    [TestMethod]
    public void FamilyCollectionRemoveFont_shouldRemoveFamily_whenFamilyIsEmpty() {
      FamilyCollection collection = new FamilyCollection();
      Font font = new Font(TestData.Font1_Description);
      collection.AddFont(font);

      collection.RemoveFont(font.UID);
      Assert.AreEqual(0, collection.Families.Count, "FamilyCollection.RemoveFont should remove empty family");
    }

    [TestMethod]
    public void FamilyCollectionRemoveFont_shouldNotRemoveFamily_whenFamilyIsNotEmpty() {
      FamilyCollection collection = new FamilyCollection();
      Font font = new Font(TestData.Font1_Description);
      Font font3 = new Font(TestData.Font3_Description);
      collection.AddFont(font).AddFont(font3);

      collection.RemoveFont(font.UID);
      Assert.AreEqual(1, collection.Families.Count, "FamilyCollection.RemoveFont should not remove non empty family");
    }

    [TestMethod]
    public void FamilyCollectionRemoveFont_shouldTriggerFamilyRemovedEvent_whenFamilyIsRemoved() {
      FamilyCollection collection = new FamilyCollection();
      Font font = new Font(TestData.Font1_Description);
      collection.AddFont(font);

      bool eventTriggered = false;
      collection.OnFamilyRemoved += delegate {
        eventTriggered = true;
      };

      collection.RemoveFont(font.UID);
      Assert.IsTrue(eventTriggered, "FamilyCollection.RemoveFont should trigger family removed event when a family is removed");
    }

    [TestMethod]
    public void FamilyCollection_shouldTriggerFontActivatedEvent_whenFontActivationStatusChange() {
      FamilyCollection collection = new FamilyCollection();
      Font font = new Font(TestData.Font1_Description);
      collection.AddFont(font);

      bool eventTriggered = false;
      collection.OnFontActivationChanged += delegate {
        eventTriggered = true;
      };

      font.Activated = true;

      Assert.IsTrue(eventTriggered, "FamilyCollection should trigger font activation event when a font activation status change");
    }

    #region setup methods
    private FamilyCollection NewCollection() {
      FamilyCollection collection = new FamilyCollection();

      Font font1 = new Font(TestData.Font1_Description);
      Font font2 = new Font(TestData.Font2_Description);

      collection.AddFont(font1).AddFont(font2);

      return collection;
    }
    #endregion
  }
}
