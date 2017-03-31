using Storage.Data;

namespace Protocol.Impl.States {
  class Running : ConnectionState {
    #region ctor
    public Running(Connection connection): this("Running", connection) {
    }

    private Running(string name, Connection connection) : base(name, connection) {
    }
    #endregion

    #region methods
    public override void Abort() {
      _context.Storage.AbortSynchronization();
      UnregisterEvents();
    }

    public override void Stop() {
      _context.Storage.EndSynchronization();
      UnregisterEvents();
    }

    protected override void Start() {
      RegisterEvents();
      WillTransition = true; // This state is supposed to change at any moment
      _context.Storage.BeginSynchronization();
      _context.TriggerUpdateFinished();
    }
    #endregion

    #region private methods
    private void RegisterEvents() {
      _context.CatalogChannel.OnFontDescription += CatalogChannel_OnFontDescription;
      _context.CatalogChannel.OnFontDeletion += CatalogChannel_OnFontDeletion;
      _context.UserChannel.OnFontActivation += UserChannel_OnFontActivation;
      _context.UserChannel.OnFontDeactivation += UserChannel_OnFontDeactivation;
    }

    private void UnregisterEvents() {
      _context.CatalogChannel.OnFontDescription -= CatalogChannel_OnFontDescription;
      _context.CatalogChannel.OnFontDeletion -= CatalogChannel_OnFontDeletion;
      _context.UserChannel.OnFontActivation -= UserChannel_OnFontActivation;
      _context.UserChannel.OnFontDeactivation -= UserChannel_OnFontDeactivation;
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
      _context.Storage.AddFont(desc);
    }
    #endregion
  }
}
