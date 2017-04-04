using Microsoft.VisualStudio.TestTools.UnitTesting;
using Storage;
using System;
using System.Net;
using System.Threading;
using TestUtilities;
using TestUtilities.Protocol;

namespace Protocol.Impl.Tests {
  public class TestConnection : Connection {
    #region properties
    public MockedTransport MockedTransport {
      get {
        return Transport as MockedTransport;
      }
    }

    public MockedHttpTransport MockedHttpTransport {
      get {
        return HttpTransport as MockedHttpTransport;
      }
    }

    public static readonly TimeSpan DefaultTestTimeout = TimeSpan.FromMilliseconds(1000);
    #endregion

    #region ctor
    public TestConnection(MockedTransport transport,
                          MockedHttpTransport http,
                          IFontStorage storage,
                          int authInterval = 1000,
                          int connInterval = 1000) : base(transport, http, storage) {
      AuthenticationRetryInterval = TimeSpan.FromMilliseconds(authInterval);
      ConnectionRetryInterval = TimeSpan.FromMilliseconds(connInterval);
    }
    #endregion

    #region test
    public string UserTopicEvent(string evt = null) {
      if (evt == null)
        return $"users:{UserData.UID}";
      else
        return $"users:{UserData.UID}.{evt}";
    }

    public void Connected(Action test, TimeSpan? timeout = null) {
      TimeSpan _timeout = timeout ?? DefaultTestTimeout;

      MockedHttpTransport.HttpRequestSentHandler httpRequestCallback = (MockedHttpRequest request, string body) => {
        if (request.Endpoint.Contains("session")) {
          return request.CreateResponse(HttpStatusCode.OK, TestData.Serialize(TestData.UserData));
        }
        return null;
      };
      MockedTransport.ConnectionAttemptHandler connectionAttemptCallback = () => {
        return true;
      };

      MockedHttpTransport.OnHttpRequestSent += httpRequestCallback;
      MockedTransport.OnConnectionAttempt += connectionAttemptCallback;

      AutoResetEvent testDoneEvent = new AutoResetEvent(false);
      Exception error = null;
      OnEstablished += delegate {
        try {
          test?.Invoke();
        }
        catch (Exception e) {
          error = e;
        }
        testDoneEvent.Set();
      };

      Connect("test_email", "test_password");

      if (!testDoneEvent.WaitOne(_timeout)) {
        Assert.Fail("Test should execute in less than {0}ms", timeout);
      }

      if (error != null) {
        throw new Exception("Test failed", error);
      }
    }

    public void Updated(Action test, TimeSpan? timeout = null) {
      TimeSpan _timeout = timeout ?? DefaultTestTimeout;

      Connected(delegate {
        AutoResetEvent catalogUpdateRequest = new AutoResetEvent(false);
        AutoResetEvent fontUpdateRequest = new AutoResetEvent(false);
        AutoResetEvent updateFinished = new AutoResetEvent(false);

        MockedTransport.OnMessageSent += (MockedBroadcastResponse resp, string evt, dynamic payload) => {
          if (evt == "catalog.update:request") {
            catalogUpdateRequest.Set();
          }
          else if (evt == UserTopicEvent("update:request")) {
            fontUpdateRequest.Set();
          }
        };

        MockedHttpTransport.OnHttpRequestSent += (MockedHttpRequest request, string body) => {
          if (request.Endpoint.Contains("downloads")) {
            return request.CreateResponse(HttpStatusCode.OK, "BODYBODYBODY");
          }
          return null;
        };

        OnCatalogUpdateFinished += delegate {
          updateFinished.Set();
        };

        UpdateCatalog();

        // wait for the catalog update request
        if (!catalogUpdateRequest.WaitOne(_timeout)) {
          Assert.Fail("Catalog update request should be send in less than {0}ms", timeout);
        }
        MockedTransport.SimulateMessage("catalog", "font:description", TestData.Font1_Description);
        MockedTransport.SimulateMessage("catalog", "update:complete");
        // wait for the fonts update request
        if (!fontUpdateRequest.WaitOne(_timeout)) {
          Assert.Fail("Font update request should be send in less than {0}ms", timeout);
        }
        MockedTransport.SimulateMessage(UserTopicEvent(), "update:complete");

        if (!updateFinished.WaitOne(_timeout)) {
          Assert.Fail("Update should finish in less than {0}ms", timeout);
        }

        try {
          test?.Invoke();
        }
        catch (Exception e) {
          throw e;
        }

      }, _timeout);
    }
    #endregion
  }
}
