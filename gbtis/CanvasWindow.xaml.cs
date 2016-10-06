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
        public event EventHandler Cancel;
        public event EventHandler Continue;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CanvasWindow() {
            InitializeComponent();
            Mouse.OverrideCursor = Cursors.None;
            drawCursor.Type = CanvasCursor.CursorType.Idle;

            // Set up cursor update events
            drawCursor.Moved += DrawCursor_Moved;
            drawCursor.Draw += (s, e) => drawCanvas.Mark(
                drawCursor.RelativePosition(drawCanvas), drawCursor.Size, drawCursor.Type.round);
            drawCursor.Erase += (s, e) => drawCanvas.Erase(
                drawCursor.RelativePosition(drawCanvas), drawCursor.Size, drawCursor.Type.round);
            drawCursor.Idle += (s, e) => drawCanvas.ClearPrevious();

            // Button events
            clearButton.Clicked += (s, e) => drawCanvas.ClearCanvas();
            cancelButton.Clicked += (s, e) => Cancel?.Invoke(this, new EventArgs());
            continueButton.Clicked += (s, e) => Continue?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Get the canvas' current image
        /// </summary>
        /// <returns>The canvas ImageSource</returns>
        public ImageSource Image() {
            return drawCanvas.Image();
        }

        /// <summary>
        /// Get the raw pixel data from the canvas
        /// </summary>
        /// <returns>RGBA pixel data</returns>
        public byte[] Pixels() {
            return drawCanvas.Pixels();
        }

        /// <summary>
        /// Cursor moved. Update position
        /// </summary>
        /// <param name="sender">Source of the event</param>
        /// <param name="e">Event parameters</param>
        private void DrawCursor_Moved(object sender, EventArgs e) {
            this.Dispatcher.Invoke(() => {
                // Update cursor position
                Point p = drawCursor.RelativePosition(drawCanvas);
                drawCursor.Position = p;

                // Test canvas borders
                if (!isInRange(p.X, 0, drawCanvas.ActualWidth - 1) || !isInRange(p.Y, 0, drawCanvas.ActualHeight - 1)) {
                    drawCursor.Type = CanvasCursor.CursorType.Missing;
                } else if (drawCursor.Type == CanvasCursor.CursorType.Missing) {
                    drawCursor.Type = CanvasCursor.CursorType.Idle;
                }

                // Test button
                clearButton.CursorOver(Mouse.GetPosition(this));
                cancelButton.CursorOver(Mouse.GetPosition(this));
                continueButton.CursorOver(Mouse.GetPosition(this));
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
        /// Test a value for a range
        /// </summary>
        /// <param name="n">The value</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>True if in range</returns>
        private bool isInRange(double n, double min, double max) {
            return (n >= min) && (n <= max);
        }

        /// <summary>
        /// Get a cursor position on the canvas
        /// </summary>
        /// <returns>The cursor position</returns>
        private Point CursorPosition() {
            Point p = Mouse.GetPosition(drawCanvas);
            return p;
        }
    }
}
