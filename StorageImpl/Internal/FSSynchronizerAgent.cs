using Protocol.Transport;
using Storage.Data;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Utilities.Extensions;
using Utilities.Threading;

namespace Storage.Impl.Internal {
  internal class FSSynchronizationAgent {
    #region private data
    private IHttpTransport _transport;
    private FSStorage _storage;

    private ProcessingAgent _agent;
    private CancellationTokenSource _cancelSource;
    #endregion

    #region properties
    public int CommandCount {
      get {
        return _agent.CommandCount;
      }
    }
    #endregion

    #region delegates
    public delegate void ProcessingStartedHandler();
    public delegate void ProcessingFinishedHandler();
    #endregion

    #region events
    public event ProcessingStartedHandler OnDownloadsStarted;
    public event ProcessingFinishedHandler OnDownloadsFinished;
    #endregion

    #region ctor
    public FSSynchronizationAgent(IHttpTransport transport, FSStorage storage) {
      _transport = transport;
      _storage = storage;
      _cancelSource = new CancellationTokenSource();
      _agent = new ProcessingAgent(transport.DownloadParallelism, _cancelSource);

      _agent.OnProcessingFinished += _agent_OnProcessingFinished;
      _agent.OnProcessingStarted += _agent_OnProcessingStarted;
    }
    #endregion

    #region methods
    public void BeginDownloads() {
      _agent.Start();
    }

    public void PauseDownloads() {
      _agent.Stop();
    }

    public void AbortDownloads() {
      _cancelSource.Cancel();
    }

    public void QueueDownload(Font font, Action then = null) {
      _agent.Enqueue(delegate {
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
      IHttpRequest request = _transport.CreateHttpRequest(font.DownloadUrl.AbsoluteUri);
      request.Method = WebRequestMethods.Http.Get;

      IHttpResponse response = await request.Response;
      using (response.ResponseStream) {
        await _storage.SaveFontFile(font.UID, response.ResponseStream);
      }
    }
    #endregion

    #region event handling
    private void _agent_OnProcessingStarted() {
      OnDownloadsStarted?.Invoke();
    }

    private void _agent_OnProcessingFinished() {
      OnDownloadsFinished?.Invoke();
    }
    #endregion
  }
}
