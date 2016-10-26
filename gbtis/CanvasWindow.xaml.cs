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
        public const int FR_CA = 0x0c0c;
        public const int ERASER_SIZE = 100;

        private string _text;
        public string Text { get { return _text; } }

        public event EventHandler Cancel;
        public event EventHandler Continue;
        
        private Recognizer recognizer;
        private StylusPointCollection stylusPoints;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CanvasWindow(Kinect kinect) {
            InitializeComponent();

            // Recognizers

            // Get language. French first, or default
            Recognizers systemRecognizers = new Recognizers();
            try {
                recognizer = systemRecognizers.GetDefaultRecognizer(FR_CA);
            } catch {
                recognizer = systemRecognizers.GetDefaultRecognizer();
            }

            // Overlay
            kinect.BitMapReady += BitMapArrived;

            // Init canvas
            canvas.EraserShape = new RectangleStylusShape(ERASER_SIZE, ERASER_SIZE);
            this.Dispatcher.Invoke(new Action(() => recognize())); 

            // Kinect controls
            kinect.FingerPositionChanged += (p) => {
                cursorMove(p, kinect.CursorMode);
            };
            kinect.ModeEnd += (m) => {
                cursorUp(kinect.FingerPosition, m);
            };
            kinect.ModeStart += (m) => {
                cursorDown(kinect.FingerPosition, m);
            };

            // Mouse controls
            canvas.MouseMove += (s, e) => {
                cursorMove(Mouse.GetPosition(canvas), mouseMode());
                e.Handled = true;
            };
            canvas.PreviewMouseDown += (s, e) => {
                cursorDown(Mouse.GetPosition(canvas), mouseMode());
                e.Handled = true;
            };
            canvas.PreviewMouseUp += (s, e) => {
                cursorUp(Mouse.GetPosition(canvas), mouseMode());
                e.Handled = true;
            };

            // Button events
            clearButton.Clicked += (s, e) => {
                this.Dispatcher.Invoke(new Action(() => recognize()));
                this.Dispatcher.Invoke(new Action(() => canvas.Strokes.Clear()));
            };
            cancelButton.Clicked += (s, e) => Cancel?.Invoke(this, new EventArgs());
            continueButton.Clicked += (s, e) => Continue?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Mouse based cursor mode
        /// </summary>
        /// <returns></returns>
        private CursorModes mouseMode() {
            bool leftDown = Mouse.LeftButton == MouseButtonState.Pressed;
            bool rightDown = Mouse.RightButton == MouseButtonState.Pressed;

            if (leftDown) return CursorModes.Draw;
            if (rightDown) return CursorModes.Erase;
            return CursorModes.Idle;
        }

        /// <summary>
        /// Cursor has been moved
        /// </summary>
        /// <param name="cursor">Position</param>
        /// <param name="mode">State</param>
        private void cursorMove(Point cursor, CursorModes mode) {
            // Stop if input capture isn't ready
            if (stylusPoints == null) return;

            // Add current point
            stylusPoints.Add(new StylusPoint(cursor.X, cursor.Y));

            //Erase if need be
            if (mode == CursorModes.Erase) erase(cursor);
        }

        /// <summary>
        /// Cursor pressed
        /// </summary>
        /// <param name="cursor">Position</param>
        /// <param name="mode">State</param>
        private void cursorDown(Point cursor, CursorModes mode) {
            // Start input capture
            if (stylusPoints == null)
                stylusPoints = new StylusPointCollection();

            // Add current point
            stylusPoints.Add(new StylusPoint(cursor.X, cursor.Y));

            // Draw points if need be
            if (mode == CursorModes.Draw) draw();
            if (mode == CursorModes.Erase) erase(cursor);
        }

        /// <summary>
        /// Cursor released
        /// </summary>
        /// <param name="cursor">Position</param>
        /// <param name="mode">State</param>
        private void cursorUp(Point cursor, CursorModes mode) {
            // Revert to standby cursor
            if (mode == CursorModes.Idle)
                canvas.EditingMode = InkCanvasEditingMode.Select;

            // Recognize input and clear set of points
            //this.Dispatcher.Invoke(new Action(() => recognize()));
            stylusPoints = null;
        }

        /// <summary>
        /// Draw the strokes saved up
        /// </summary>
        private void draw() {
            canvas.EditingMode = InkCanvasEditingMode.Ink;

            System.Windows.Ink.Stroke stroke = new System.Windows.Ink.Stroke(stylusPoints, canvas.DefaultDrawingAttributes);
            canvas.Strokes.Add(stroke);
        }
        
        /// <summary>
        /// Erase at a point
        /// </summary>
        /// <param name="cursor">Cursor location</param>
        private void erase(Point cursor) {
            canvas.EditingMode = InkCanvasEditingMode.EraseByPoint;

            Rect eraser = new Rect(
                new Point(cursor.X - ERASER_SIZE / 2, cursor.Y - ERASER_SIZE / 2),
                new Size(ERASER_SIZE, ERASER_SIZE));
            canvas.Strokes.Erase(eraser);
        }

        /// <summary>
        /// OCR magic
        /// </summary>
        private void recognize() {
            using (MemoryStream ms = new MemoryStream()) {
                // Build an inkCollector containing the strokes
                canvas.Strokes.Save(ms);
                var myInkCollector = new InkCollector();
                var ink = new Ink();
                ink.Load(ms.ToArray());
                
                using (RecognizerContext context = recognizer.CreateRecognizerContext()) {
                    RecognitionStatus status;
                    context.Strokes = ink.Strokes;

                    // Cannot do if there are no strokes
                    if (ink.Strokes.Count > 0) {
                        var results = context.Recognize(out status);
                        if (status == RecognitionStatus.NoError) {
                            if (results.TopString.Length > 0) {
                                // Set the text
                                _text = results.TopString;
                                previewText.Text = _text;

                                // Styling changes
                                previewText.Foreground = new SolidColorBrush(Colors.Black);
                                continueButton.Enable();

                                return;
                            }
                        }
                    }

                    // Default options
                    previewText.Text = gbtis.Properties.Resources.noString;
                    previewText.Foreground = new SolidColorBrush(Colors.Gray);
                    continueButton.Disable();
                }
            }
        }

        /// <summary>
        /// Update the camera feed from the sensor
        /// </summary>
        /// <param name="img">The latest ImageSource</param>
        private void BitMapArrived(ImageSource img) {
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

    public enum CursorModes {
        Idle, Draw, Erase
    }
}
