using Protocol.Transport;
using Storage.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Utilities;
using Utilities.Extensions;

namespace Protocol.Impl.States {
  class Installing : ConnectionState {
    #region private data
    private CancellationTokenSource _cancelSource;
    #endregion

    #region ctor
    public Installing(Connection connection) : this("Installing", connection) {
    }

    private Installing(string name, Connection connection) : base(name, connection) {
      _cancelSource = new CancellationTokenSource();
    }
    #endregion

    #region methods
    public override void Abort() {
      Stop();
    }

    public override void Stop() {
      _cancelSource.Cancel();
    }

    protected override void Start() {
      // find all fonts not already downloaded
      IEnumerable<Font> fontsToDownload = _context.Storage.Families.SelectMany(family => {
        return family.Fonts.Where(font => {
          return !_context.Storage.IsFontDownloaded(font.UID);
        });
      });

      // group not downloaded fonts by parallelism packet
      IEnumerable<IEnumerable<Font>> downloadSteps = fontsToDownload
        .Select((el, i) => new { Index = i, Value = el })
        .GroupBy(el => el.Index / _context.DownloadParallelism)
        .Select(el => el.Select(v => v.Value));

      // chain all download steps after the seed
      Task seed = Task.Factory.StartNew(delegate { }, _cancelSource.Token, TaskCreationOptions.LongRunning);
      Task download = downloadSteps.Aggregate(seed, (previousStep, step) => {
        return previousStep.Then(async delegate {
          // we start this download step only when the previous step has successfully finished
          IEnumerable<Task> downloadTasks = step.Select(font => { // for each fonts in this step:
            if (_cancelSource.IsCancellationRequested) { // do NOT start downloading the file if cancellation is requested
              return new Task(delegate { });
            }

            return DownloadFont(font);
          });

          // synchronize download tasks so that this step finishes only when all downloads are finished and saved to storage
          await Task.WhenAll(downloadTasks);
        });
      }).Then(delegate { // when all download steps are done
        DownloadFinished();
      });
    }
    #endregion

    #region private methods
    private Task DownloadFont(Font font) {
      IHttpRequest request = _context.Transport.CreateHttpRequest(font.DownloadUrl.AbsoluteUri);
      request.Method = WebRequestMethods.Http.Get;

      return request.Response.Then(response => {
        using(response.ResponseStream) {
          if (!_cancelSource.IsCancellationRequested) {
            return _context.Storage.SaveFontFile(font.UID, response.ResponseStream);
          }
          else { // do NOT save the file if cancellation was requested
            return Task.Factory.StartNew(() => { });
          }
        }
      });
    }
    #endregion

    #region event handling
    private void DownloadFinished() {
      // this is executed in an async task so I don't really care for the double foreach

      // activate all fonts
      foreach(Family family in _context.Storage.ActivatedFamilies) {
        foreach(Font font in family.Fonts) {
          if (font.Activated) {
            // TODO: Activate font
            Console.WriteLine($"Activating font {font.Name}");
          }
        }
      }

      WillTransition = true;
      FSM.State = new Running(_context);
    }
    #endregion
  }
}
