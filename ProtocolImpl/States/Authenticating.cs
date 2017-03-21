using Newtonsoft.Json;
using Protocol.Transport;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Utilities;
using Utilities.Extensions;

namespace Protocol.Impl.States {
  class Authenticating : ConnectionState {
    #region private data
    private Payloads.Authentication _authPayload;
    private IHttpRequest _authRequest;

    private static readonly int AuthenticationTimeout = 60000;
    #endregion

    #region ctor
    public Authenticating(Connection connection, string email, string password) : this(connection) {
      _authPayload = new Payloads.Authentication {
        Login = email,
        Password = password,
        ProtocolVersion = "0.2.8",
        ApplicationVersion = "0.2",
        Os = "Win",
        OsVersion = Environment.OSVersion.VersionString
      };
    }

    private Authenticating(Connection connection) : base("Authenticating", connection) {
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

    protected override async void Start() {
      _authRequest = _context.Transport.CreateHttpRequest(Urls.Authentication);

      _authRequest.Method = WebRequestMethods.Http.Post;
      _authRequest.ContentType = "application/json";
      _authRequest.Timeout = AuthenticationTimeout;

      try {
        using (StreamWriter body = new StreamWriter(_authRequest.RequestStream)) {
          string json = JsonConvert.SerializeObject(_authPayload);
          body.Write(json);
        }
      }
      catch (WebException) {
        WillTransition = true;
        FSM.State = new Idle(_context);
        _context.TriggerValidationFailure("The application failed to connect to the Fontstore servers.\nPlease check your Internet access is working properly.");
        return;
      }
      catch (Exception) {
        WillTransition = true;
        FSM.State = new Idle(_context);
        _context.TriggerValidationFailure("Unknown internal error");
        return;
      }

      await _authRequest.Response
        .Then(response => {
          using (StreamReader body = new StreamReader(response.ResponseStream)) {
            string data = body.ReadToEnd();

            if (response.StatusCode != HttpStatusCode.OK) {
              WillTransition = true;
              FSM.State = new Idle(_context);
              _context.TriggerValidationFailure(data);
            }
            else {
              Payloads.UserData userData = JsonConvert.DeserializeObject<Payloads.UserData>(data);
              WillTransition = true;
              FSM.State = new Connecting(_context, userData);
            }
          }
        })
        .Recover(e => {
          WillTransition = true;
          FSM.State = new RetryAuthenticating(_context, _authPayload.Login, _authPayload.Password);
        });
    }
    #endregion
  }
}
