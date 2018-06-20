using System;
using System.Collections.ObjectModel;

namespace Utilities.Extensions {
  public static class ObservableCollectionExtension {
    public static void SortAdd<T>(this ObservableCollection<T> self,  T element) where T: class, IComparable<T> {
      if (self.Count <= 0) {
        // nothing to compare with, the collection is empty
        self.Add(element);
      } else if (self[self.Count - 1].CompareTo(element) <= 0) {
        // last element is before the element to insert, no need to check the entire collection, the new element is last
        self.Add(element);
      } else {
        // collection is not empty and new element is not supposed to be last, check the vald position within the collection
        for (int index = 0; index < self.Count; index++) {
          if (self[index].CompareTo(element) > 0) {
            self.Insert(index, element);
            return;
          }
        }
      }
    }
  }
}
