using Protocol;
using System;

namespace TestUtilities.Protocol {
  public class MockedConnection : CallTracer, IConnection {
    private bool _connected;

    public MockedConnection(bool connected = false) {
      _connected = connected;
    }

    public void Connect(string email, string password) {
      RegisterCall("Connect");
      _connected = true;
      OnEstablished?.Invoke(TestData.UserData);
    }

    public void Disconnect() {
      RegisterCall("Disconnect");
      _connected = false;
    }

    public void UpdateCatalog(DateTime? lastUpdate) {
      RegisterCall("UpdateCatalog");
    }

    public void UpdateFontsStatus(DateTime? lastUpdate) {
      RegisterCall("UpdateFontsStatus");
    }

    public void SimulateEvent(ConnectionEvents eventType, dynamic data = null) {
      switch(eventType) {
        case ConnectionEvents.Established:
          OnEstablished?.Invoke(data);
          break;

        case ConnectionEvents.ValidationFailure:
          OnValidationFailure?.Invoke();
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

    public event ConnectionEstablishedHandler OnEstablished;
    public event ConnectionValidationFailedHandler OnValidationFailure;
    public event FontDeletedHandler OnFontDeleted;
    public event FontDescriptionHandler OnFontDesctiptionReceived;
    public event FontActivationHandler OnFontActivated;
    public event FontDeactivationHandler OnFontDeactivated;
    public event UpdateFinishedHandler OnUpdateFinished;
  }
}
