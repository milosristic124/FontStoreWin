using FontInstaller;
using Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Previews {
  public class Generator: IDisposable
  {
    #region data
    private IFontInstaller _installer;
    private Dictionary<string, FontCollection> _fontCollections;

    private string _previewsDir;
    private Brush _textBrush;
    #endregion

    #region constants
    private const int MAX_PREVIEW_HEIGHT = 40;
    private const int MAX_PREVIEW_WIDTH = 140;
    private const int FONT_SIZE = 13;
    private const string FONT_BRUSH_NAME = "FSBlackBrush";
    #endregion

    #region static properties
    public static Generator Instance { get; private set; }
    #endregion

    #region ctors
    private Generator() {
      throw new InvalidOperationException("PreviewGenerator can not be instanciated. Please call PreviewGenerator.Initialize instead.");
    }

    private Generator(IFontInstaller installer) {
      _installer = installer;
      _fontCollections = new Dictionary<string, FontCollection>();

      _previewsDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Fontstore/previews";
      Directory.CreateDirectory(_previewsDir);

      _textBrush = new SolidBrush(Color.FromArgb(27, 27, 27));

      _installer.OnPrivateFontInstalled += _installer_OnPrivateFontInstalled;
    }
    #endregion

    #region static methods
    public static Generator Initialize(IFontInstaller installer) {
      if (Instance == null) {
        Instance = new Generator(installer);
      }

      return Instance;
    }
    #endregion

    #region methods
    public string GetPreviewPath(Storage.Data.Font model, bool familyPreview = false) {
      return FontPreviewPath(model.UID, familyPreview);
    }

    public async Task GeneratePreview(Storage.Data.Font model, bool familyPreview = false) {
      if (!PreviewExists(model.UID, familyPreview)) {
        Logger.Log("Generating preview for font {0}", model.UID);

        string text = familyPreview ? model.FamilyName : model.Style;

        try {
          FontCollection collection = _fontCollections[model.UID];
          Image image = await GenerateImage(text, collection);
          await SaveImage(FontPreviewPath(model.UID, familyPreview), image);
        } catch (Exception e) {
          Logger.Log("Failed to generate preview for font {0}: {1}", model.UID, e.Message);
        }
      }
    }
    #endregion

    #region private methods
    private bool PreviewExists(string uid, bool familyPreview) {
      return File.Exists(FontPreviewPath(uid, familyPreview));
    }

    private string FontPreviewPath(string uid, bool familyPreview) {
      if (familyPreview) {
        return _previewsDir + $"/ff_{uid}.png";
      } else {
        return _previewsDir + $"/{uid}.png";
      }
    }

    private int BoundedWidth(SizeF imageSize) {
      return Math.Min((int)Math.Ceiling(imageSize.Width), MAX_PREVIEW_WIDTH);
    }

    private int BoundedHeight(SizeF imageSize) {
      return Math.Min((int)Math.Ceiling(imageSize.Height), MAX_PREVIEW_HEIGHT);
    }

    private async Task<Image> GenerateImage(string text, FontCollection collection) {
      return await Task.Factory.StartNew(delegate {
        Font font = new Font(collection.Families[0], FONT_SIZE, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel);

        SizeF imageSize = SizeF.Empty;
        using (Image dummyImage = new Bitmap(1, 1)) {
          using (Graphics dummyContext = Graphics.FromImage(dummyImage)) {
            imageSize = dummyContext.MeasureString(text, font);
          }
        }

        Image image = new Bitmap(BoundedWidth(imageSize), BoundedHeight(imageSize));
        using (Graphics drawingContext = Graphics.FromImage(image)) {
          drawingContext.Clear(Color.Transparent);
          drawingContext.CompositingQuality = CompositingQuality.HighQuality;
          drawingContext.InterpolationMode = InterpolationMode.HighQualityBicubic;
          drawingContext.SmoothingMode = SmoothingMode.HighQuality;
          drawingContext.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

          drawingContext.DrawString(text, font, _textBrush, 0, 0);

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
      foreach (FontCollection collection in _fontCollections.Values) {
        collection.Dispose();
      }
      _fontCollections.Clear();

      Instance = null;
    }
    #endregion

    #region event handling
    private void _installer_OnPrivateFontInstalled(string uid, string path) {
      PrivateFontCollection collection = new PrivateFontCollection();
      collection.AddFontFile(path);
      _fontCollections.Add(uid, collection);
    }
    #endregion
  }
}
