using Protocol;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Storage {
  public class MemFontCatalog : IFontCatalog {
    private IConnection connection;
    private DateTime? lastCatalogUpdate;
    private DateTime? lastFontsUpdate;
    private CancellationTokenSource cancellationSource;

    public IList<Family> Families { get; private set; }

    public MemFontCatalog(IConnection serverConnection) {
      this.Families = new List<Family>();
      this.connection = serverConnection;
      this.cancellationSource = new CancellationTokenSource();

      this.lastCatalogUpdate = null;
      this.lastFontsUpdate = null;
    }

    ~MemFontCatalog() {
      this.cancellationSource.Cancel();
    }

    public async Task update() {
      // pop a thread for to update the catalog
      Task updateTask = Task.Run(new Action(() => {
        // register for font description/deletion and catalog update finish

        // start the catalog update (wait for result to ensure successfull delivery)
      }), this.cancellationSource.Token);


      await updateTask;
    }
  }
}
