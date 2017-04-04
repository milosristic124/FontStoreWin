using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities.Threading {
  public class ProcessingAgent {
    #region private data
    private ConcurrentQueue<Action> _commandQueue;
    private bool _running;
    private CancellationTokenSource _cancelSource;
    private bool _processingStarted;
    private int _processedCommands;

    private Func<int> _getConcurrentFactor;
    private int _concurrentFactor {
      get {
        return _getConcurrentFactor();
      }
    }
    #endregion

    #region properties
    public int CommandCount {
      get {
        return _commandQueue.Count;
      }
    }

    public bool Running {
      get {
        return _running;
      }
      protected set {
        if (_running != value) {
          _running = value;

          if (_running) {
            OnProcessingStarted?.Invoke();
          }
          else {
            OnProcessingFinished?.Invoke(_processedCommands);
          }
        }
      }
    }
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
    public ProcessingAgent(int concurrentFactor, CancellationTokenSource cancelSource) {
      if (concurrentFactor < 1) throw new Exception($"Processing agent concurrent factor can not be less than 1 (value = {concurrentFactor})");

      _getConcurrentFactor = () => concurrentFactor;

      _running = false;
      _commandQueue = new ConcurrentQueue<Action>();
      _processingStarted = false;

      if (cancelSource != null) {
        _cancelSource = cancelSource;
      } else {
        _cancelSource = new CancellationTokenSource();
      }
    }

    public ProcessingAgent(Func<int> getConcurrentFactor, CancellationTokenSource cancelSource) {
      _getConcurrentFactor = getConcurrentFactor;
      _running = false;
      _commandQueue = new ConcurrentQueue<Action>();
      _processingStarted = false;

      if (cancelSource != null) {
        _cancelSource = cancelSource;
      }
      else {
        _cancelSource = new CancellationTokenSource();
      }
    }
    #endregion

    #region methods
    public void Start() {
      _processingStarted = true;
      _processedCommands = 0;
      StartProcessing();
    }

    public void Stop() {
      _processingStarted = false;
    }

    public void Enqueue(Action command) {
      _commandQueue.Enqueue(command);
      StartProcessing();
    }
    #endregion

    #region private methods

    private async void StartProcessing() {
      if (_processingStarted && !Running) {
        await ProcessCommands();
      }
    }


    private async Task ProcessCommands() {
      await Task.Run(delegate {
        while (_processingStarted && !_commandQueue.IsEmpty) {
          Running = true;

          List<Task> batch = new List<Task>();
          for (int it = 0; it < _concurrentFactor && !_commandQueue.IsEmpty; it++) {
            Action action;
            if (_commandQueue.TryDequeue(out action)) {
              batch.Add(Task.Run(action));
            }
          }
          Task.WaitAll(batch.ToArray());
          _processedCommands += batch.Count;
        }
        Running = false;
      });
    }
    #endregion
  }
}
