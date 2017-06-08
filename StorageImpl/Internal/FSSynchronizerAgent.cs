using Logging;
using Protocol.Transport.Http;
using Storage.Data;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Utilities.Threading;

namespace Storage.Impl.Internal {
  internal class FSSynchronizationAgent {
    #region private data
    private IHttpTransport _transport;
    private FSStorage _storage;

    private ProcessingAgent _agent;
    private CancellationTokenSource _cancelSource;

    private Dictionary<string, IHttpRequest> _dlRequests;
    #endregion

    #region properties
    public int CommandCount {
      get {
        return _agent.CommandCount;
      }
    }
    public int DownloadCount { get; private set; }
    #endregion

    #region delegates
    public delegate void ProcessingStartedHandler();
    public delegate void ProcessingFinishedHandler(int processedCommands);
    #endregion

    #region events
    public event ProcessingStartedHandler OnProcessingStarted;
    public event ProcessingFinishedHandler OnProcessingFinished;
    #endregion

    #region ctor
    public FSSynchronizationAgent(IHttpTransport transport, FSStorage storage) {
      _transport = transport;
      _storage = storage;
      _dlRequests = new Dictionary<string, IHttpRequest>();
      _cancelSource = new CancellationTokenSource();
      _agent = new ProcessingAgent(() => transport.DownloadParallelism, _cancelSource);

      _agent.OnProcessingFinished += _agent_OnProcessingFinished;
      _agent.OnProcessingStarted += _agent_OnProcessingStarted;
    }

    internal FSSynchronizationAgent(FSSynchronizationAgent other): this(other._transport, other._storage) {
    }
    #endregion

    #region methods
    public void StartProcessing() {
      _agent.Start();
    }

    public void PauseProcessing() {
      _agent.Stop();

      foreach (IHttpRequest request in _dlRequests.Values) {
        request?.Abort();
      }
      _dlRequests.Clear();
    }

    public void AbortProcessing() {
      _agent.Stop();
      _cancelSource.Cancel();

      lock (_dlRequests) {
        foreach (IHttpRequest request in _dlRequests.Values) {
          request?.Abort();
        }
        _dlRequests.Clear();
      }
    }

    public void QueueDownload(Font font, Action then = null) {
      _agent.Enqueue(delegate {
        Logger.Log("Download started: {0}", font.UID);
        DownloadFont(font).Wait();
        then?.Invoke();
      });
    }

    public void QueueDeletion(Font font, Action then = null) {
      _agent.Enqueue(delegate {
        _storage.RemoveFontFile(font.UID).Wait();
        then?.Invoke();
      });
    }
    #endregion

    #region private methods
    private async Task DownloadFont(Font font) {
      if (!_storage.FontFileExists(font.UID)) {
        try {
          IHttpRequest request = _transport.CreateHttpRequest(font.DownloadUrl.AbsoluteUri);
          request.Method = WebRequestMethods.Http.Get;

          lock (_dlRequests) {
            _dlRequests.Add(font.UID, request);
          }
          IHttpResponse response = await request.Response;
          using (response.ResponseStream) {
            await _storage.SaveFontFile(font.UID, response.ResponseStream, _cancelSource.Token);
          }
          lock (_dlRequests) {
            _dlRequests.Remove(font.UID);
          }
          DownloadCount += 1;
        }
        catch (Exception e) {
          Logger.Log("Downloading font {0} failed: {1}", font.UID, e);
        }
      }
      else {
        Logger.Log("Download already done: {0}", font.UID);
      }
    }
    #endregion

    #region event handling
    private void _agent_OnProcessingStarted() {
      OnProcessingStarted?.Invoke();
    }

    private void _agent_OnProcessingFinished(int processedCommands) {
      OnProcessingFinished?.Invoke(processedCommands);
    }
    #endregion
  }
}
