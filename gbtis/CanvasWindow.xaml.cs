using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Ink;
using System.IO;

namespace gbtis {
    /// <summary>
    /// Interaction logic for CanvasWindow.xaml
    /// </summary>
    public partial class CanvasWindow : Window {
        public const int FR_CA = 0x0c0c;

        // OCR property
        private string _text;
        public string Text { get { return _text; } }

        // Button events
        public event ButtonEventHandler Help;
        public event ButtonEventHandler Cancel;
        public event TextEventHandler Continue;

        // Event handler types
        public delegate void TextEventHandler(string text);
        public delegate void ButtonEventHandler();

        private Recognizer recognizer;
        private StylusPointCollection stylusPoints;
        private int c;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CanvasWindow(Kinect kinect) {
            InitializeComponent();
            cursor.Position = Mouse.GetPosition(canvas);

            // Get language. French first, or default
            Recognizers systemRecognizers = new Recognizers();
            recognizer = systemRecognizers.GetDefaultRecognizer();

            // Overlay
            kinect.BitMapReady += BitMapArrived;

            // Init canvas
            Dispatcher.Invoke(new Action(() => recognize()));

            // Kinect controls
            kinect.FingerPositionChanged += (p) => {
                cursor.Position = Kinect.ColorToInterface(p, new Size(ActualWidth, ActualHeight));
                cursor.Mode = kinect.CursorMode;
                c++;
                cursorMove();
            };
            kinect.ModeEnd += (m) => {
                cursor.Position = Kinect.ColorToInterface(kinect.FingerPosition, new Size(ActualWidth, ActualHeight));
                cursor.Mode = m;
                cursorUp();
            };
            kinect.ModeStart += (m) => {
                cursor.Position = Kinect.ColorToInterface(kinect.FingerPosition, new Size(ActualWidth, ActualHeight));
                cursor.Mode = m;
                cursorDown();
            };

            // Mouse controls
            MouseMove += (s, e) => {
                cursor.Position = Mouse.GetPosition(canvas);
                cursor.Mode = mouseMode();

                cursorMove();
                e.Handled = true;
            };
            PreviewMouseDown += (s, e) => {
                cursor.Position = Mouse.GetPosition(canvas);
                cursor.Mode = mouseMode();

                cursorDown();
            };
            PreviewMouseUp += (s, e) => {
                cursor.Position = Mouse.GetPosition(canvas);
                cursor.Mode = mouseMode();

                cursorUp();
            };

            // Disable default controls
            canvas.PreviewMouseDown += (s, e) => e.Handled = true;
            canvas.PreviewMouseUp += (s, e) => e.Handled = true;
            canvas.PreviewStylusDown += (s, e) => e.Handled = true;
            canvas.PreviewStylusUp += (s, e) => e.Handled = true;
            canvas.PreviewTouchDown += (s, e) => e.Handled = true;
            canvas.PreviewTouchUp += (s, e) => e.Handled = true;

            // Button events
            cursor.Moved += (p) => {
                helpButton.Over((helpButton.Intersects(this, p)));
                clearButton.Over((clearButton.Intersects(this, p)));
                cancelButton.Over((cancelButton.Intersects(this, p)));
                continueButton.Over((continueButton.Intersects(this, p)));
            };
            clearButton.Clicked += (s, e) => {
                this.Dispatcher.Invoke(new Action(() => {
                    canvas.Strokes.Clear();
                    recognize();
                }));
            };
            cancelButton.Clicked += (s, e) => Cancel?.Invoke();
            continueButton.Clicked += (s, e) => Continue?.Invoke(Text);
            helpButton.Clicked += (s, e) => Help?.Invoke();
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

        private StylusPoint RelativeTransformStylus(Point p) {
            p = this.TransformToDescendant(canvas).Transform(p);
            return new StylusPoint(p.X, p.Y);
        }

        private Point RelativeTransform(Point p) {
            p = this.TransformToDescendant(canvas).Transform(p);
            return new Point(p.X, p.Y);
        }

        /// <summary>
        /// Cursor has been moved
        /// </summary>
        /// <param name="cursor">Position</param>
        /// <param name="mode">State</param>
        private void cursorMove() {
            // Stop if input capture isn't ready
            if (stylusPoints == null) return;

            // Add current point
            stylusPoints.Add(RelativeTransformStylus(cursor.Position));

            //Erase if need be
            if (cursor.Mode == CursorModes.Erase) erase();
        }

        /// <summary>
        /// Cursor pressed
        /// </summary>
        /// <param name="cursor">Position</param>
        /// <param name="mode">State</param>
        private void cursorDown() {
            // Start input capture
            if (stylusPoints == null)
                stylusPoints = new StylusPointCollection();

            // Add current point
            stylusPoints.Add(RelativeTransformStylus(cursor.Position));

            // Draw points if need be
            if (cursor.Mode == CursorModes.Draw) draw();
            if (cursor.Mode == CursorModes.Erase) erase();
        }

        /// <summary>
        /// Cursor released
        /// </summary>
        /// <param name="cursor">Position</param>
        /// <param name="mode">State</param>
        private void cursorUp() {
            // Revert to standby cursor
            if (cursor.Mode == CursorModes.Idle)
                canvas.EditingMode = InkCanvasEditingMode.Select;

            // Recognize input and clear set of points
            Dispatcher.Invoke(new Action(() => recognize()));
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
        private void erase() {
            canvas.EditingMode = InkCanvasEditingMode.EraseByPoint;

            Point p = RelativeTransform(new Point(
                cursor.Position.X - cursor.Size / 2, cursor.Position.Y - cursor.Size / 2));
            canvas.Strokes.Erase(new Rect(p, new Size(cursor.Size, cursor.Size)));
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
                    context.Factoid = Factoid.WordList; // Magic smoke for name recognition
                    context.Strokes = ink.Strokes;

                    // Cannot do if there are no strokes
                    if (ink.Strokes.Count > 0) {
                        var results = context.Recognize(out status);
                        if (status == RecognitionStatus.NoError) {
                            if (results.TopString.Length > 0) {
                                // Set the text
                                _text = results.ToString();
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
    }

    /// <summary>
    /// Cursor modes for the canvas' cursor
    /// </summary>
    public enum CursorModes {
        Idle, Draw, Erase
    }
}
