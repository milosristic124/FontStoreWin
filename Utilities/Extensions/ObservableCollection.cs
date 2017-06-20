using System;
using System.Collections.ObjectModel;

namespace Utilities.Extensions {
  public static class ObservableCollectionExtension {
    public static void SortAdd<T>(this ObservableCollection<T> self,  T element) where T: class, IComparable<T> {
      for (int index = 0; index < self.Count; index++) {
        if (self[index].CompareTo(element) > 0) {
          self.Insert(index, element);
          return;
        }
      }
    }
  }
}
