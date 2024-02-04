using System;
using System.Diagnostics;
using System.IO;
using System.Resources;
using System.Threading.Tasks;

using MeasurePixels.Helpers;
using MeasurePixels.Pages;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;

using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Graphics.DirectX;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace MeasurePixels.Measure
{
    public sealed partial class MeasureCanvas : UserControl
    {
        public MeasureCanvas()
        {
            this.InitializeComponent();
            this.InitCanvasClearColor();
            settings.ThemeChanged += Settings_ThemeChanged;
        }

        private void InitCanvasClearColor()
        {
            Color color = ColorExts.FromHex("#C7C8C9");
            if (settings.Theme == ElementTheme.Dark
                || (settings.Theme == ElementTheme.Default && Application.Current.RequestedTheme == ApplicationTheme.Dark))
                color = ColorExts.FromHex("#333");
            canvas.ClearColor = color;
        }

        public void Unload()
        {
            Debug.WriteLine("Unload Mesure canvas");
            for (int i = measureObjects.Count - 1; i >= 0; i--)
                measureObjects[i].Dispose();
            measureObjects.Clear();
            temp?.Dispose();
            temp = null;
            measureBitmap?.Dispose();
            measureBitmap = null;
            magnifier?.Dispose();
            magnifier = null;
            this.canvas.RemoveFromVisualTree();
            this.canvas = null;
        }

        private void canvas_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
            => args.TrackAsyncAction(Paste(sender).AsAsyncAction());

        public async Task<bool> CheckClipboard()
        {
            try
            {
                var dataView = Clipboard.GetContent();
                var re = dataView.Contains(StandardDataFormats.Bitmap);
                if (!re)
                {
                    if (dataView.Contains(StandardDataFormats.StorageItems))
                    {
                        var items = await dataView.GetStorageItemsAsync();
                        if (items.Count > 0)
                        {
                            var (IsImage, Stream) = await TryParseImage(items[0]);
                            re = IsImage;
                            if (!re) Stream?.Dispose();
                        }
                    }
                }
                return re;
            }
            catch (UnauthorizedAccessException)
            {
                //when app is deactivated cannot access clipboard 
                //always a UnauthorizedAccessException
                //Whatever return true then we will check agin when acitivated
                Debug.WriteLine(nameof(CheckClipboard) + " UnauthorizedAccessException for now.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(nameof(CheckClipboard) + ": " + ex);

#if DEBUG
                ContentDialog dialog = new ContentDialog
                {
                    PrimaryButtonText = "OK",
                    Title = "Clipboard Exception",
                    Content = ex.ToString()
                };
                await dialog.ShowAsync();
#endif
            }
            return false;
        }

        public async Task Paste(CanvasControl control = null)
        {
            DataPackageView dataView = null;
            try
            {
                dataView = Clipboard.GetContent();
            }
            catch (UnauthorizedAccessException ue)
            {
                Debug.WriteLine(ue);//ignore
            }
            if (dataView != null)
                await Paste(dataView, control);
        }

        private void Grid_DragOver(object sender, Windows.UI.Xaml.DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy | DataPackageOperation.Link;
            e.DragUIOverride.IsCaptionVisible = false;
            e.DragUIOverride.IsGlyphVisible = false;
        }

        private async void Grid_Drop(object sender, Windows.UI.Xaml.DragEventArgs e)
            => await Paste(e.DataView);

        public async Task Paste(DataPackageView dataView, CanvasControl control = null)
        {
            IRandomAccessStreamWithContentType stream = null;
            try
            {
                if (dataView.Contains(StandardDataFormats.Bitmap))
                {
                    var data = await dataView.GetBitmapAsync();
                    stream = await data.OpenReadAsync();

                }
                else if (dataView.Contains(StandardDataFormats.StorageItems))
                {
                    var items = await dataView.GetStorageItemsAsync();
                    if (items.Count > 0)
                    {
                        var (IsImage, Stream) = await TryParseImage(items[0]);
                        if (IsImage) stream = Stream;
                    }
                }

                if (stream != null)
                {
                    var bitmap = await CanvasBitmap.LoadAsync(control ?? this.canvas, stream);
                    this.Update(bitmap);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.GetType() + ":\n" + e);//ignore
                await Toast.ShowAsync(
                    ResourceLoader.GetForCurrentView().GetString("OpenImageFailed")
                    + $"\n{e.Message}");
            }
            finally
            {
                stream?.Dispose();
            }
        }


        private async Task<(bool IsImage, IRandomAccessStreamWithContentType Stream)>
            TryParseImage(IStorageItem item)
        {
            var file = item as StorageFile;
            if (file == null || !file.IsAvailable)
                return (false, null);
            try
            {
                var stream = await file.OpenReadAsync();
                var bm = new BitmapImage();
                bm.SetSource(stream);
                return (bm.PixelHeight > 0 && bm.PixelWidth > 0, stream);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return (false, null);
        }

        public async Task SaveAsync(StorageFile file, bool measuresOnly)
        {
            if (file == null || measureBitmap == null) return;

            Debug.WriteLine(file.ContentType + "   " + file.FileType);
            var fileType = file.ContentType?.Trim().ToLower();
            CanvasBitmapFileFormat fileFormat = CanvasBitmapFileFormat.Png;
            switch (fileType)
            {
                case "image/jpeg":
                    fileFormat = CanvasBitmapFileFormat.Jpeg;
                    break;
                case "image/bmp":
                    fileFormat = CanvasBitmapFileFormat.Bmp;
                    break;
                case "image/gif":
                    fileFormat = CanvasBitmapFileFormat.Gif;
                    break;
                case "image/tiff":
                    fileFormat = CanvasBitmapFileFormat.Tiff;
                    break;
                case "image/png":
                default:
                    break;
            }

            var device = CanvasDevice.GetSharedDevice();
            var size = measureBitmap.Size;
            var target = new CanvasRenderTarget(device, (float)size.Width, (float)size.Height,
                 measureBitmap.Dpi, DirectXPixelFormat.B8G8R8A8UIntNormalized,
                 CanvasAlphaMode.Premultiplied);
            using (var g = target.CreateDrawingSession())
            {
                g.Clear(fileFormat == CanvasBitmapFileFormat.Png ? Colors.Transparent : Colors.White);
                if (!measuresOnly)
                {
                    g.DrawImage(measureBitmap);
                    g.Antialiasing = CanvasAntialiasing.Antialiased;
                    g.TextAntialiasing = CanvasTextAntialiasing.Auto;
                }
                foreach (var item in measureObjects)
                    item.Draw(g, false);
            }
            var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            await target.SaveAsync(stream, fileFormat);
            stream.Dispose();
            target.Dispose();
        }

        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                ViewMode = PickerViewMode.Thumbnail,
            };
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".jpe");
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".tif");
            picker.FileTypeFilter.Add(".tiff");

            var file = await picker.PickSingleFileAsync();
            if (file == null) return;

            var (isImage, stream) = await TryParseImage(file);
            if (isImage && stream != null)
            {
                var bitmap = await CanvasBitmap.LoadAsync(this.canvas, stream);
                this.Update(bitmap);
            }
            else
            {
                await Toast.ShowAsync(ResourceLoader.GetForCurrentView().GetString("OpenImageFailed"));
            }
        }
    }
}
