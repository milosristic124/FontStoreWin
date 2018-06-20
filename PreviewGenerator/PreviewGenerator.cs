using FontInstaller;
using Logging;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Threading.Tasks;

namespace PreviewGenerator {
  public class PreviewGenerator: IDisposable
  {
    #region data
    private IFontInstaller _installer;
    private PrivateFontCollection _fontCollection;

    private string _previewsDir;
    #endregion

    #region constants
    public int MAX_PREVIEW_HEIGHT = 40;
    public int MAX_PREVIEW_WIDTH = 140;
    public int FONT_SIZE = 12;
    #endregion

    #region ctor
    public PreviewGenerator(IFontInstaller installer) {
      _installer = installer;
      _fontCollection = new PrivateFontCollection();

      _previewsDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Fontstore/previews";
      Directory.CreateDirectory(_previewsDir);

      _installer.OnPrivateFontInstalled += _installer_OnPrivateFontInstalled;
    }
    #endregion

    #region methods
    public void List() {
      Logger.Log("\nPrivate font collection:");
      foreach (FontFamily family in _fontCollection.Families) {
        Logger.Log("\t{0}", family.Name);
      }
    }

    public async Task<string> GetPreviewPath(Storage.Data.Font model) {
      if (PreviewExists(model.UID)) {
        return FontPreviewPath(model.UID);
      }

      string fullName = model.FamilyName + " " + model.Style;
      if (model.Style.ToLower() == "regular") {
        fullName = model.FamilyName;
      }

      string previewPath = FontPreviewPath(model.UID);

      Image image = await GenerateImage(fullName);
      await SaveImage(previewPath, image);

      return previewPath;
    }
    #endregion

    #region private methods
    private bool PreviewExists(string uid) {
      return File.Exists(FontPreviewPath(uid));
    }

    private string FontPreviewPath(string uid) {
      return _previewsDir + $"/{uid}.png";
    }

    private int BoundedWidth(SizeF imageSize) {
      return Math.Min((int)Math.Ceiling(imageSize.Width), MAX_PREVIEW_WIDTH);
    }

    private int BoundedHeight(SizeF imageSize) {
      return Math.Min((int)Math.Ceiling(imageSize.Height), MAX_PREVIEW_HEIGHT);
    }

    private async Task<Image> GenerateImage(string fullName) {
      return await Task.Factory.StartNew(delegate {
        FontFamily family = new FontFamily(fullName, _fontCollection);
        Font font = new Font(family, FONT_SIZE, FontStyle.Regular, GraphicsUnit.Pixel);

        SizeF imageSize = SizeF.Empty;
        using (Image dummyImage = new Bitmap(1, 1)) {
          using (Graphics dummyContext = Graphics.FromImage(dummyImage)) {
            imageSize = dummyContext.MeasureString("", font);
          }
        }

        Image image = new Bitmap(BoundedWidth(imageSize), BoundedHeight(imageSize));
        using (Graphics drawingContext = Graphics.FromImage(image)) {
          drawingContext.Clear(Color.White);
          drawingContext.SmoothingMode = SmoothingMode.AntiAlias;
          drawingContext.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

          Brush textBrush = null;
          drawingContext.DrawString("text", font, textBrush, 0, 0);

          drawingContext.Flush();
        }

        return image;
      });
    }

    private async Task SaveImage(string path, Image image) {
      await Task.Factory.StartNew(delegate {
        image.Save(path, ImageFormat.Png);
      });
    }
    #endregion

    #region IDisposable
    public void Dispose() {
      _installer.OnPrivateFontInstalled -= _installer_OnPrivateFontInstalled;
      _fontCollection.Dispose();
    }
    #endregion

    #region event handling
    private void _installer_OnPrivateFontInstalled(string uid, string path) {
      _fontCollection.AddFontFile(path);
    }
    #endregion
  }
}
