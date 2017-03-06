﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace TestUtilities {
  public interface ICallTracer {
    void Verify(string methodName, int times);
    void RegisterCall(string name);
  }

  public class CallTracer: ICallTracer {
    private Dictionary<string, int> _calls;

    public CallTracer() {
      _calls = new Dictionary<string, int>();
    }

    public void Verify(string methodName, int times) {
      int callCount = 0;
      if (this._calls.ContainsKey(methodName))
        callCount = this._calls[methodName];

      if (callCount != times)
        Assert.Fail("Expected {0} calls of method {1} but method was called {2} times", times, methodName, callCount);
    }

    public  void RegisterCall(string name) {
      if (!this._calls.ContainsKey(name))
        this._calls[name] = 1;
      else {
        this._calls[name] += 1;
      }
    }
  }
}
