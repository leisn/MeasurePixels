using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

using Microsoft.Graphics.Canvas;

using Windows.Graphics;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.Display;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml.Controls;

namespace MeasurePixels.Pages
{
    public partial class EditorPage
    {

        bool IsCaptureSupported = true;
        bool IsCaptureNotSupported => !IsCaptureSupported;

        // Capture API objects.
        SizeInt32 _lastSize;
        GraphicsCaptureItem _item;
        Direct3D11CaptureFramePool _framePool;
        GraphicsCaptureSession _session;

        void SetupCapture()
        {
            IsCaptureSupported = GraphicsCaptureSession.IsSupported();
            if (IsCaptureNotSupported)
                return;
        }

        public void StopCapture()
        {
            _session?.Dispose();
            _framePool?.Dispose();
            _item = null;
            _session = null;
            _framePool = null;
        }

        public async Task StartCaptureAsync()
        {
            var picker = new GraphicsCapturePicker();
            var item = await picker.PickSingleItemAsync();
            // The item may be null if the user dismissed the
            // control without making a selection or hit Cancel.
            if (item != null)
            {
                StartCaptureInternal(item);
            }
        }


        private void StartCaptureInternal(GraphicsCaptureItem item)
        {
            // Stop the previous capture if we had one.
            StopCapture();

            _item = item;
            _lastSize = _item.Size;

            _framePool = Direct3D11CaptureFramePool.Create(
               this.measureCanvas.Device, // D3D device
               DirectXPixelFormat.B8G8R8A8UIntNormalized, // Pixel format
               2, // Number of frames
               _item.Size); // Size of the buffers

            _framePool.FrameArrived += (s, a) =>
            {
                // The FrameArrived event is raised for every frame on the thread
                // that created the Direct3D11CaptureFramePool. This means we
                // don't have to do a null-check here, as we know we're the only
                // one dequeueing frames in our application.  

                // NOTE: Disposing the frame retires it and returns  
                // the buffer to the pool.

                using (var frame = _framePool.TryGetNextFrame())
                {
                    if (frame == null)
                        return;
                    ProcessFrame(frame);
                }

                StopCapture();// NOTE: just need one frame
            };

            _item.Closed += (s, a) =>
            {
                StopCapture();
            };

            _session = _framePool.CreateCaptureSession(_item);
            _session.IsCursorCaptureEnabled = false;
            _session.StartCapture();
        }

        private void ProcessFrame(Direct3D11CaptureFrame frame)
        {
            // Resize and device-lost leverage the same function on the
            // Direct3D11CaptureFramePool. Refactoring it this way avoids
            // throwing in the catch block below (device creation could always
            // fail) along with ensuring that resize completes successfully and
            // isn’t vulnerable to device-lost.
            bool needsReset = false;

            if ((frame.ContentSize.Width != _lastSize.Width) ||
                (frame.ContentSize.Height != _lastSize.Height))
            {
                needsReset = true;
                _lastSize = frame.ContentSize;
            }

            try
            {
                // Convert our D3D11 surface into a Win2D object.
                var bitmap = CanvasBitmap.CreateFromDirect3D11Surface(
                     this.measureCanvas.Device,
                     frame.Surface);
                measureCanvas.Update(bitmap);
            }
            catch
            {
                needsReset = true;
            }

            if (needsReset)
            {
                _framePool.Recreate(
                        this.measureCanvas.Device,
                        DirectXPixelFormat.B8G8R8A8UIntNormalized,
                        2,
                        frame.ContentSize);
            }
        }
    }
}
