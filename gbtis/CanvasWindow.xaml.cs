using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace gbtis {
    /// <summary>
    /// Interaction logic for CanvasWindow.xaml
    /// </summary>
    public partial class CanvasWindow : Window {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CanvasWindow() {
            InitializeComponent();
            Mouse.OverrideCursor = Cursors.None;
            drawCursor.Type = CanvasCursor.CursorType.Idle;

            // Set up cursor update events
            MouseMove += CanvasWindow_MouseMove;
            MouseLeftButtonDown += (s, e) => Draw();
            MouseLeftButtonUp += (s, e) => Idle();
            MouseRightButtonDown += (s, e) => Erase();
            MouseRightButtonUp += (s, e) => Idle();
        }

        /// <summary>
        /// Draw at cursor
        /// </summary>
        private void Draw() {
            Point p = CursorPosition();
            drawCursor.Type = CanvasCursor.CursorType.Draw;
            drawCanvas.mark(p, drawCursor.Size);
        }

        /// <summary>
        /// Erase at cursor
        /// </summary>
        private void Erase() {
            Point p = CursorPosition();
            drawCursor.Type = CanvasCursor.CursorType.Erase;
            drawCanvas.erase(p, drawCursor.Size);
        }

        /// <summary>
        /// Return to idle
        /// </summary>
        private void Idle() {
            drawCursor.Type = CanvasCursor.CursorType.Idle;
        }

        /// <summary>
        /// Cursor moved. Update position
        /// </summary>
        /// <param name="sender">Source of the event</param>
        /// <param name="e">Event parameters</param>
        private void CanvasWindow_MouseMove(object sender, MouseEventArgs e) {
            this.Dispatcher.Invoke(() => {
                // Update cursor position
                Point p = CursorPosition();
                drawCursor.Position = p;
                
                // Perform on the spot
                if (Mouse.LeftButton == MouseButtonState.Pressed) {
                    Draw();
                } else if (Mouse.RightButton == MouseButtonState.Pressed) {
                    Erase();
                }
            });
        }

        /// <summary>
        /// Update the camera feed from the sensor
        /// </summary>
        /// <param name="sender">Object sending the event</param>
        /// <param name="e">Arguments for the event object</param>
        private void FrameReader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e) {
            var reference = e.FrameReference.AcquireFrame();
            using (var frame = reference.ColorFrameReference.AcquireFrame()) {
                if (frame != null) {
                    this.Dispatcher.Invoke(new Action(() =>
                    sensorOverlay.Source = ToBitmap(frame)));
                }
            }
        }

        /// <summary>
        /// Convert a frame of kinect color video to bitmap for display
        /// Conversion code from http://pterneas.com/2014/02/20/kinect-for-windows-version-2-color-depth-and-infrared-streams/
        /// Under Apache License 2.0
        /// </summary>
        /// <param name="frame">The frame received from the kinect</param>
        /// <returns>A bitmap format source for a WPF image control</returns>
        private static ImageSource ToBitmap(ColorFrame frame) {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;

            byte[] pixels = new byte[width * height * ((PixelFormats.Bgr32.BitsPerPixel + 7) / 8)];

            if (frame.RawColorImageFormat == ColorImageFormat.Bgra) {
                frame.CopyRawFrameDataToArray(pixels);
            } else {
                frame.CopyConvertedFrameDataToArray(pixels, ColorImageFormat.Bgra);
            }

            int stride = width * PixelFormats.Bgr32.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
        }

        /// <summary>
        /// Move a value into a set range so that min < n < max
        /// </summary>
        /// <param name="n">The value</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>The new value</returns>
        private double moveIntoRange(double n, double min, double max) {
            if (n < min) n = min;
            if (n > max) n = max;
            return n;
        }

        /// <summary>
        /// Get a cursor position on the canvas
        /// </summary>
        /// <returns>The cursor position</returns>
        private Point CursorPosition() {
            Point p = Mouse.GetPosition(drawCanvas);
            p.X = moveIntoRange(p.X, 0, drawCanvas.ActualWidth - 1);
            p.Y = moveIntoRange(p.Y, 0, drawCanvas.ActualHeight - 1);

            return p;
        }
    }
}
