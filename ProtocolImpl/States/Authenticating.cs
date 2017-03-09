using Newtonsoft.Json;
using Protocol.Payloads;
using Protocol.Transport;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Protocol.Impl.States {
  class Authenticating : ConnectionState {
    #region private data
    private Authentication _authPayload;
    private IHttpRequest _authRequest;

    private static readonly string AuthenticationEndpoint = "https://app.fontstore.com/authenticate";
    private static readonly int AuthenticationTimeout = 60000;
    #endregion

    #region ctor
    public Authenticating(Connection connection, IConnectionTransport transport, string email, string password): this(connection, transport) {
      _authPayload = new Authentication {
        Login = email,
        Password = password,
        ProtocolVersion = "0.0.3",
        ApplicationVersion = "0.2",
        Os = "Win",
        OsVersion = Environment.OSVersion.VersionString
      };
    }

    private Authenticating(Connection connection, IConnectionTransport transport) : base(connection, transport) {
      _authPayload = null;
      _authRequest = null;
    }
    #endregion

    #region methods
    public override void Stop() {
      _authRequest = null;
    }

    public override void Abort() {
      _authRequest?.Abort();
      _authRequest = null;
    }

    protected override void Start() {
      _authRequest = _context.CreateHttpRequest(AuthenticationEndpoint);

      _authRequest.Method = WebRequestMethods.Http.Post;
      _authRequest.ContentType = "application/json";
      _authRequest.Timeout = AuthenticationTimeout;

      using (StreamWriter body = new StreamWriter(_authRequest.RequestStream)) {
        string json = JsonConvert.SerializeObject(_authPayload);
        body.Write(json);
      }

      _authRequest.Response.ContinueWith(requestTask => {
        if (requestTask.Status == TaskStatus.RanToCompletion) {
          IHttpResponse response = requestTask.Result;

          using (StreamReader body = new StreamReader(response.ResponseStream)) {
            string data = body.ReadToEnd();

            if (response.StatusCode != HttpStatusCode.OK) {
              WillTransition = true;
              FSM.State = new Idle(_connection, _context);
              _connection.TriggerValidationFailure(data);
            } else {
              UserData userData = JsonConvert.DeserializeObject<UserData>(data);
              WillTransition = true;
              FSM.State = new Connecting(_connection, _context, userData);
            }
          }
        } else {
          WillTransition = true;
          FSM.State = new RetryAuthenticating(_connection, _context, _authPayload.Login, _authPayload.Password);
        }
      });
    }
    #endregion
  }
}
