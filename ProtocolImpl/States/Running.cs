using Protocol.Transport;
using Storage.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Utilities.Extensions;

namespace Protocol.Impl.States {
  class Running : ConnectionState {
    #region private type
    private class ProcessingAgent {
      private Task _processingTask;
      private ConcurrentQueue<Action> _actionQueue;
      private int _concurrentFactor;
      private bool _running;
      private CancellationTokenSource _cancelSource;

      public ProcessingAgent(int concurrentFactor = 1, CancellationTokenSource cancelSource = null) {
        _concurrentFactor = concurrentFactor;
        _processingTask = null;
        _running = false;
        _actionQueue = new ConcurrentQueue<Action>();
        _cancelSource = cancelSource;
      }

      public void Enqueue(Action action) {
        _actionQueue.Enqueue(action);

        if (!_running) { // no running tasks
          _processingTask = ProcessQueue();
        }
      }

      // pop _concurrentFactor actions from the queue and start them asynchronously
      // the returned task finishes when all actions are finished
      private Task ProcessQueue() {
        // if there is nothing to do we just create a task to reset the running state.
        if (_actionQueue.IsEmpty) {
          return Task.Factory.StartNew(() => { _running = false; }, _cancelSource.Token);
        }
        _running = true;

        // pop up to _concurrentFactor actions to run in this batch
        List<Action> batch = new List<Action>();
        for (int it = 0; it < _concurrentFactor && !_actionQueue.IsEmpty; it++) {
          Action ac;
          if (_actionQueue.TryDequeue(out ac)) {
            batch.Add(ac);
          }
        }

        // start each action of the batch asynchronously and wait for all of them to finish.
        return Task.WhenAll(batch.Select(a => Task.Factory.StartNew(a, _cancelSource.Token))).ContinueWith(async delegate {
          // when the batch has finished, queue another one.
          // we will process every queued action recursively.
          // once the last action has been processed the reset task will set _running to false allowing
          // the next queued action to restart the processing queue.
          await ProcessQueue();
        }, TaskContinuationOptions.OnlyOnRanToCompletion);
      }
    }
    #endregion

    #region private data
    private CancellationTokenSource _cancelSource;
    private ProcessingAgent _downloadAgent;
    #endregion

    #region ctor
    public Running(Connection connection): this("Running", connection) {
      _cancelSource = new CancellationTokenSource();
      _downloadAgent = new ProcessingAgent(_context.DownloadParallelism, _cancelSource);
    }

    private Running(string name, Connection connection) : base(name, connection) {
    }
    #endregion

    #region methods
    public override void Abort() {
      Stop();
    }

    public override void Stop() {
      _cancelSource.Cancel();

      _context.CatalogChannel.OnFontDescription -= CatalogChannel_OnFontDescription;
      _context.CatalogChannel.OnFontDeletion -= CatalogChannel_OnFontDeletion;
      _context.UserChannel.OnFontActivation -= UserChannel_OnFontActivation;
      _context.UserChannel.OnFontDeactivation -= UserChannel_OnFontDeactivation;
    }

    protected override void Start() {
      _context.CatalogChannel.OnFontDescription += CatalogChannel_OnFontDescription;
      _context.CatalogChannel.OnFontDeletion += CatalogChannel_OnFontDeletion;
      _context.UserChannel.OnFontActivation += UserChannel_OnFontActivation;
      _context.UserChannel.OnFontDeactivation += UserChannel_OnFontDeactivation;

      WillTransition = true; // This state is supposed to change at any moment

      _context.TriggerUpdateFinished();
    }
    #endregion

    #region event handling
    private void UserChannel_OnFontDeactivation(string uid) {
      _context.Storage.DeactivateFont(uid);
    }

    private void UserChannel_OnFontActivation(string uid) {
      _context.Storage.ActivateFont(uid);
    }

    private void CatalogChannel_OnFontDeletion(string uid) {
      _context.Storage.RemoveFont(uid);
    }

    private void CatalogChannel_OnFontDescription(Payloads.FontDescription desc) {
      Font newFont = _context.Storage.AddFont(desc);
      if (!_context.Storage.IsFontDownloaded(newFont.UID)) {
        _downloadAgent.Enqueue(async delegate {
          await DownloadFont(newFont.UID, newFont.DownloadUrl);
        });
      }
    }
    #endregion

    #region private methods
    private async Task DownloadFont(string uid, Uri uri) {
      IHttpRequest request = _context.Transport.CreateHttpRequest(uri.AbsoluteUri);
      request.Method = WebRequestMethods.Http.Get;

      await request.Response.Then(async response => {
        using (response.ResponseStream) {
          if (!_cancelSource.IsCancellationRequested) {
            await _context.Storage.SaveFontFile(uid, response.ResponseStream);
          }
          else { // do NOT save the file if cancellation was requested
            await Task.Factory.StartNew(() => { });
          }
        }
      });
    }
    #endregion
  }
}
