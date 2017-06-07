using Logging;
using Newtonsoft.Json;
using Protocol.Transport.Http;
using System;
using System.IO;
using System.Net;
using Utilities;

namespace Protocol.Impl.States {
  class Authenticating : ConnectionState {
    #region private data
    private Payloads.Authentication _authPayload;
    private IHttpRequest _authRequest;

    private static readonly int AuthenticationTimeout = 60000;
    #endregion

    #region ctor
    public Authenticating(Connection connection, string email, string password) : this(connection) {
      _authPayload = new Payloads.Connect {
        Login = email,
        Password = password,
        ProtocolVersion = Constants.App.ProtocolVersion,
        ApplicationVersion = Constants.App.ApplicationVersion,
        Os = Constants.App.OSType,
        OsVersion = Environment.OSVersion.VersionString
      };
    }

    public Authenticating(Connection connection, string authToken) : this(connection) {
      _authPayload = new Payloads.AutoConnect {
        AuthToken = authToken,
        ProtocolVersion = Constants.App.ProtocolVersion,
        ApplicationVersion = Constants.App.ApplicationVersion,
        Os = Constants.App.OSType,
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
      _authRequest = _context.HttpTransport.CreateHttpRequest(Constants.Urls.Authentication);

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
        _context.TriggerValidationFailure("An internet connection is required. Please check your connection and try again.");
        return;
      }
      catch (Exception) {
        WillTransition = true;
        FSM.State = new Idle(_context);
        _context.TriggerValidationFailure("Unknown internal error.");
        return;
      }

      try {
        IHttpResponse response = await _authRequest.Response;
        string data = ReadResponseBody(response.ResponseStream);

        if (response.StatusCode != HttpStatusCode.OK) {
          Payloads.AuthenticationFailure failure = JsonConvert.DeserializeObject<Payloads.AuthenticationFailure>(data);
          WillTransition = true;
          FSM.State = new Idle(_context);
          _context.TriggerValidationFailure(failure.Message);
        }
        else {
          Payloads.UserData userData = JsonConvert.DeserializeObject<Payloads.UserData>(data);
          WillTransition = true;
          FSM.State = new Connecting(_context, userData);
        }
      } catch (WebException e) {
        if (e.Response != null) {
          string data = ReadResponseBody(e.Response.GetResponseStream());
          Payloads.AuthenticationFailure failure = JsonConvert.DeserializeObject<Payloads.AuthenticationFailure>(data);
          WillTransition = true;
          FSM.State = new Idle(_context);
          _context.TriggerValidationFailure(failure.Message);
        } else {
#if DEBUG
          Logger.Log("Authentication failed: {0}", e);
#endif
          WillTransition = true;
          FSM.State = new RetryAuthenticating(_context, _authPayload);
        }
#if DEBUG
      } catch (Exception e) {
        Logger.Log("Authentication failed: {0}", e);
#else
      } catch (Exception ) {
#endif
        WillTransition = true;
        FSM.State = new RetryAuthenticating(_context, _authPayload);
      }
    }
    #endregion

    #region private methods
    private string ReadResponseBody(Stream body) {
      using (StreamReader bodyReader = new StreamReader(body)) {
        return bodyReader.ReadToEnd();
      }
    }
    #endregion
  }
}
