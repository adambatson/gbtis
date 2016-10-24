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
using Microsoft.Ink;
using System.Windows.Ink;
using System.IO;

namespace gbtis {
    /// <summary>
    /// Interaction logic for CanvasWindow.xaml
    /// </summary>
    public partial class CanvasWindow : Window {
        public const int ERASER_SIZE = 50;
        public event EventHandler Cancel;
        public event EventHandler Continue;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CanvasWindow(Kinect kinect) {
            InitializeComponent();
            canvas.EraserShape = new RectangleStylusShape(ERASER_SIZE, ERASER_SIZE);

            canvas.PreviewMouseDown += (s, e) => {
                canvas.EditingMode = (Mouse.RightButton == MouseButtonState.Pressed) ?
                InkCanvasEditingMode.EraseByPoint : InkCanvasEditingMode.Ink;
            };

            canvas.MouseUp += (s, e) => {
                if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released)
                    canvas.EditingMode = InkCanvasEditingMode.Select;
            };

            recognize();
            MouseLeftButtonUp += (s, e) => recognize();
            MouseRightButtonUp += (s, e) => recognize();

            // Button events
            clearButton.Clicked += (s, e) => this.Dispatcher.Invoke(new Action(() => canvas.Strokes.Clear()));
            cancelButton.Clicked += (s, e) => Cancel?.Invoke(this, new EventArgs());
            continueButton.Clicked += (s, e) => Continue?.Invoke(this, new EventArgs());
        }

        public void recognize() {
            using (MemoryStream ms = new MemoryStream()) {
                canvas.Strokes.Save(ms);
                var myInkCollector = new InkCollector();
                var ink = new Ink();
                ink.Load(ms.ToArray());

                using (RecognizerContext recognizer = new RecognizerContext()) {
                    RecognitionStatus status;
                    recognizer.Strokes = ink.Strokes;

                    if (ink.Strokes.Count > 0) {
                        var results = recognizer.Recognize(out status);
                        if (status == RecognitionStatus.NoError) {
                            if (results.TopString.Length > 0) {
                                previewText.Text = results.TopString;
                                previewText.Foreground = new SolidColorBrush(Colors.Black);
                                return;
                            }
                        }
                    }

                    previewText.Text = gbtis.Properties.Resources.noString;
                    previewText.Foreground = new SolidColorBrush(Colors.Gray);
                }
            }
        }

        /// <summary>
        /// Update the camera feed from the sensor
        /// </summary>
        /// <param name="img">The latest ImageSource</param>
        void BitMapArrived(ImageSource img) {
            this.Dispatcher.Invoke(new Action(() =>
                sensorOverlay.Source = img));
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
    }
}
