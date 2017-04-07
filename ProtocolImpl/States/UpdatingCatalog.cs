using Protocol.Transport;
using Storage;

namespace Protocol.Impl.States {
  class UpdatingCatalog : ConnectionState {
    #region ctor
    public UpdatingCatalog(Connection connection) : base("UpdatingCatalog", connection) {
    }
    #endregion

    #region methods
    public override void Abort() {
      Stop();
      _context.CatalogChannel.Leave();
    }

    public override void Stop() {
      _context.CatalogChannel.OnFontDescription -= _chan_OnFontDescription;
      _context.CatalogChannel.OnFontDeletion -= _chan_OnFontDeletion;
      _context.CatalogChannel.OnUpdateComplete -= _chan_OnUpdateComplete;
    }

    protected override void Start() {
      _context.CatalogChannel.OnFontDescription += _chan_OnFontDescription;
      _context.CatalogChannel.OnFontDeletion += _chan_OnFontDeletion;
      _context.CatalogChannel.OnUpdateComplete += _chan_OnUpdateComplete;

      _context.CatalogChannel.Join().Then(() => {
        _context.CatalogChannel.SendUpdateRequest(_context.Storage.LastCatalogUpdate);
      });
    }
    #endregion

    #region event handling
    private void _chan_OnUpdateComplete() {
      WillTransition = true;
      FSM.State = new UpdatingFonts(_context);
    }

    private void _chan_OnFontDeletion(Payloads.TimestampedFontId fid) {
      _context.Storage.RemoveFont(fid);
    }

    private void _chan_OnFontDescription(Payloads.FontDescription desc) {
      _context.Storage.AddFont(desc);
    }
    #endregion
  }
}
