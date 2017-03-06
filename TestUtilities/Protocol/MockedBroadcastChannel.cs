using Protocol.Transport;

namespace TestUtilities.Protocol {
  public class MockedBroadcastChannel : IBroadcastChannel {
    #region properties
    public bool IsJoined { get; private set; }
    public string Topic { get; private set; }
    #endregion

    #region ctor
    public MockedBroadcastChannel(string topic) {
      Topic = topic;
    }
    #endregion

    #region test methods
    public delegate void ChannelSentHandler(MockedBroadcastChannel chan, MockedBroadcastResponse resp, string evt, dynamic payload);
    public event ChannelSentHandler OnMessageSent;
    #endregion

    #region methods
    public IBroadcastResponse Join() {
      IsJoined = true;
      return new MockedBroadcastResponse("ok");
    }

    public IBroadcastResponse Leave() {
      IsJoined = false;
      return new MockedBroadcastResponse("ok");
    }

    public IBroadcastResponse Send(string @event, dynamic payload) {
      MockedBroadcastResponse resp = new MockedBroadcastResponse();
      OnMessageSent?.Invoke(this, resp, @event, payload);
      return resp;
    }
    #endregion
  }
}
