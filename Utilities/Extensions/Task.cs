using System;
using System.Threading.Tasks;

namespace Utilities.Extensions {
  public static class TaskExtensions {
    #region Then extension
    public static async Task Then(this Task antecedent, Action continuation) {
      await antecedent;
      continuation();
    }

    public static async Task Then<TResult>(this Task<TResult> antecedent, Action<TResult> continuation) {
      continuation(await antecedent);
    }

    public static async Task<TNewResult> Then<TNewResult>(this Task antecedent, Func<TNewResult> continuation) {
      await antecedent;
      return continuation();
    }

    public static async Task<TNewResult> Then<TResult, TNewResult>(this Task<TResult> antecedent, Func<TResult, TNewResult> continuation) {
      return continuation(await antecedent);
    }

    public static async Task Then(this Task antecedent, Func<Task> continuation) {
      await antecedent;
      await continuation();
    }

    public static async Task<TNewResult> Then<TNewResult>(this Task antecedent, Func<Task<TNewResult>> continuation) {
      await antecedent;
      return await continuation();
    }

    public static async Task Then<TResult>(this Task<TResult> antecedent, Func<TResult, Task> continuation) {
      await continuation(await antecedent);
    }

    public static async Task<TNewResult> Then<TResult, TNewResult>(this Task<TResult> antecedent, Func<TResult, Task<TNewResult>> continuation) {
      return await continuation(await antecedent);
    }
    #endregion

    #region Recover extension
    public static async Task Recover(this Task antecedent, Action<Exception> recover) {
      await antecedent.ContinueWith(t => {
        if (t.IsFaulted) {
          recover(t.Exception);
        }
      });
    }

    public static async Task Recover<TResult>(this Task<TResult> antecedent, Action<Exception> recover) {
      await antecedent.ContinueWith(t => {
        if (t.IsFaulted) {
          recover(t.Exception);
        }
      });
    }

    public static async Task<TResult> Recover<TResult>(this Task<TResult> antecedent, Func<Exception, TResult> recover) {
      return await antecedent.ContinueWith(t => {
        if (t.IsFaulted) {
          return recover(t.Exception);
        }
        return t.Result;
      });
    }
    #endregion
  }
}
