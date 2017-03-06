using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using TestUtilities;
using TestUtilities.Protocol;

namespace Protocol.Impl.Tests {
  [TestClass()]
  public class ConnectionTests {
    [TestMethod()]
    public void Connect_shouldTriggerOnEstablished_whenTheConnectionIsEstablished() {
      MockedTransport transport = new MockedTransport();
      Connection connection = new Connection(transport);

      bool authenticationMessageSent = false;
      transport.OnMessageSent += (MockedBroadcastResponse resp, string evt, dynamic payload) => {
        if (evt == "validation.authenticate") {
          authenticationMessageSent = true;
          resp.SimulateReply("ok", TestData.UserData);
        }
      };

      AutoResetEvent connEstablishedEvent = new AutoResetEvent(false);
      connection.OnEstablished += delegate {
        connEstablishedEvent.Set();
      };

      connection.Connect("email", "password");

      Task.Factory.StartNew(() => {
        Thread.Sleep(50); // wait 50ms
        transport.SimulateConnection(); // simulate async socket connect
      });

      int timeout = 5000;
      bool timedout = connEstablishedEvent.WaitOne(timeout);

      transport.Verify("Connect", 1);
      Assert.IsTrue(authenticationMessageSent, "Connecting should send an authentication message to the server");
      Assert.IsTrue(timedout, "Connecting should trigger an OnEstablished event in less than {0}ms", timeout);
    }
  }
}