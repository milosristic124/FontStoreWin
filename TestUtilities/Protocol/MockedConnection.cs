using Protocol;
using Protocol.Payloads;
using Protocol.Transport;
using System;
using System.Threading;

namespace TestUtilities.Protocol {
  public class MockedConnection : CallTracer, IConnection {
    #region private data
    private bool _connected;
    #endregion

    #region properties
    public UserData UserData { get; set; }
    public IConnectionTransport Transport { get; }

    public TimeSpan AuthenticationRetryInterval { get; private set; }
    public TimeSpan ConnectionRetryInterval { get; private set; }
    #endregion

    #region ctor
    public MockedConnection(bool connected = false) {
      _connected = connected;
      Transport = null;
      AuthenticationRetryInterval = Timeout.InfiniteTimeSpan;
      ConnectionRetryInterval = Timeout.InfiniteTimeSpan;
    }
    #endregion

    #region methods
    public void Connect(string email, string password) {
      RegisterCall("Connect");
      _connected = true;
      OnEstablished?.Invoke(TestData.UserData);
    }

    public void Disconnect() {
      RegisterCall("Disconnect");
      _connected = false;
    }
    #endregion

    #region test methods
    public void SimulateEvent(ConnectionEvents eventType, dynamic data = null) {
      switch(eventType) {
        case ConnectionEvents.Established:
          UserData = data as UserData;
          OnEstablished?.Invoke(UserData);
          break;

        case ConnectionEvents.ValidationFailure:
          OnValidationFailure?.Invoke(data);
          break;

        case ConnectionEvents.FontDescriptionReceived:
          OnFontDesctiptionReceived?.Invoke(data);
          break;

        case ConnectionEvents.FontDeleted:
          OnFontDeleted?.Invoke(data);
          break;

        case ConnectionEvents.FontActivated:
          OnFontActivated?.Invoke(data);
          break;

        case ConnectionEvents.FontDeactivated:
          OnFontDeactivated?.Invoke(data);
          break;

        case ConnectionEvents.UpdateFinished:
          OnUpdateFinished?.Invoke();
          break;
      }
    }

    public enum ConnectionEvents {
      Established,
      ValidationFailure,
      FontDescriptionReceived,
      FontDeleted,
      FontActivated,
      FontDeactivated,
      UpdateFinished
    }
    #endregion

    #region events
    public event ConnectionEstablishedHandler OnEstablished;
    public event ConnectionValidationFailedHandler OnValidationFailure;
    public event FontDeletedHandler OnFontDeleted;
    public event FontDescriptionHandler OnFontDesctiptionReceived;
    public event FontActivationHandler OnFontActivated;
    public event FontDeactivationHandler OnFontDeactivated;
    public event UpdateFinishedHandler OnUpdateFinished;
    #endregion
  }
}
