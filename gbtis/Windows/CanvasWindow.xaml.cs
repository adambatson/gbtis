﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Ink;
using System.IO;
using gbtis.Helpers;
using System.Timers;

namespace gbtis.Windows {
    /// <summary>
    /// Interaction logic for CanvasWindow.xaml
    /// </summary>
    public partial class CanvasWindow : Window {
        public const int HELP_DURATION = 10000;
        public const int THANKS_DURATION = 3000;
        public const int FR_CA = 0x0c0c;

        // OCR property
        private string _text;
        public string Text { get { return _text; } }

        // Button events
        public event ButtonEventHandler Cancel;
        public event TextEventHandler Continue;

        // Event handler types
        public delegate void TextEventHandler(string text);
        public delegate void ButtonEventHandler();

        //Text and canvas data
        private CharacterRecognizer ocr;
        private StylusPointCollection stylusPoints;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CanvasWindow() {
            InitializeComponent();
            Kinect kinect = Kinect.getInstance();

            // Overlay
            kinect.BitMapReady += BitMapArrived;

            // Init canvas
            ocr = new CharacterRecognizer(canvas);
            Dispatcher.Invoke(new Action(() => recognize()));
        }

        /// <summary>
        /// Handling it
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e">It</param>
        public void Handle(Object s, RoutedEventArgs e) {
            e.Handled = true;
        }

        /// <summary>
        /// Handle a resize of the overlay
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        public void HandleResize(Object s, EventArgs e) {
            Size bounds = (sensorOverlay.ActualWidth > 0) ?
                new Size(sensorOverlay.ActualWidth, sensorOverlay.ActualHeight) :
                new Size(ActualWidth, ActualHeight);

            cursor.SetBounds(bounds, sensorOverlay.Margin.Left);
        }

        /// <summary>
        /// Show the help overlay
        /// </summary>
        public void HelpClicked(Object source, EventArgs e) {
            helpOverlay.AnimateOpacity(1);
            Timer t = new Timer(HELP_DURATION);
            t.Elapsed += (s, ee) => Dispatcher.Invoke(new Action(() => {
                helpOverlay.AnimateOpacity(0);
            }));
            t.Start();
        }

        /// <summary>
        /// Cancel the operation
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void CancelClicked(Object source, EventArgs e) {
            Cancel?.Invoke();
        }

        /// <summary>
        /// Clear the canvas
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void ClearClicked(Object source, EventArgs e) {
            this.Dispatcher.Invoke(new Action(() => {
                canvas.Strokes.Clear();
                recognize();
            }));
        }

        /// <summary>
        /// Move to the next step
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void ContinueClicked(Object source, EventArgs e) {
            Dispatcher.Invoke(new Action(() => {
                sensorOverlay.Opacity = 1;
                thanksMsg.Opacity = 1;
                cursor.Opacity = 0;
                canvasBorder.Opacity = 0;
                buttonBar.Opacity = 0;
            }));

            Timer t = new Timer(THANKS_DURATION);
            t.Elapsed += (ss, ee) => Continue?.Invoke(Text);
            t.Start();
        }

        //im so sorry
        public void clearScreen() {
            canvas.Strokes.Clear();
            recognize();
        }

        /// <summary>
        ///  Convert a Point into a StylusPoint
        /// </summary>
        /// <param name="p">A Point</param>
        /// <returns>StylusPoint</returns>
        private StylusPoint toStylusPoint(Point p) {
            return new StylusPoint(p.X, p.Y);
        }

        /// <summary>
        /// Transform a point into a point relative to the canvas
        /// </summary>
        /// <param name="p">The point</param>
        /// <returns>A Point</returns>
        private Point relativeTransform(Point p) {
            p = sensorOverlay.TransformToAncestor(this).Transform(p);
            p = this.TransformToDescendant(canvas).Transform(p);

            Size bounds = new Size(canvas.ActualWidth, canvas.ActualHeight);
            if (p.X > canvas.ActualWidth) p.X = canvas.ActualWidth;
            if (p.X < 0) p.X = 0;
            if (p.Y > canvas.ActualHeight) p.Y = canvas.ActualHeight;
            if (p.Y < 0) p.Y = 0;

            return new Point(p.X, p.Y);
        }

        /// <summary>
        /// Cursor has been moved
        /// </summary>
        /// <param name="cursor">Position</param>
        /// <param name="mode">State</param>
        private void cursorMove(Point p) {
            p = relativeTransform(p);

            // Check buttons
            helpButton.Over((helpButton.Intersects(this, p)));
            clearButton.Over((clearButton.Intersects(this, p)));
            cancelButton.Over((cancelButton.Intersects(this, p)));
            continueButton.Over((continueButton.Intersects(this, p)));

            // Stop if input capture isn't ready
            if (stylusPoints == null) return;

            // Add current point
            stylusPoints.Add(toStylusPoint(relativeTransform(cursor.Position)));

            //Erase if need be
            if (cursor.Mode == CursorModes.Erase) erase();
        }

        /// <summary>
        /// Cursor pressed
        /// </summary>
        /// <param name="cursor">Position</param>
        /// <param name="mode">State</param>
        private void cursorDown(CursorModes m) {
            // Start input capture
            if (stylusPoints == null)
                stylusPoints = new StylusPointCollection();

            // Add current point
            stylusPoints.Add(toStylusPoint(relativeTransform(cursor.Position)));

            // Draw points if need be
            if (cursor.Mode == CursorModes.Draw) draw();
            if (cursor.Mode == CursorModes.Erase) erase();
        }

        /// <summary>
        /// Cursor released
        /// </summary>
        /// <param name="cursor">Position</param>
        /// <param name="mode">State</param>
        private void cursorUp(CursorModes m) {
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

            Point p = relativeTransform(new Point(
                cursor.Position.X - cursor.Size / 2, cursor.Position.Y - cursor.Size / 2));
            canvas.Strokes.Erase(new Rect(p, new Size(cursor.Size, cursor.Size)));
        }

        /// <summary>
        /// OCR magic
        /// </summary>
        private void recognize() {
            string results = ocr.Recognize();
            if (results.Length > 0) {
                // Set the text
                _text = results.ToString();
                previewText.Text = _text;

                // Styling changes
                previewText.Foreground = new SolidColorBrush(Colors.Black);
                continueButton.Enable();
            } else {
                // Default options
                previewText.Text = gbtis.Properties.Resources.noString;
                previewText.Foreground = new SolidColorBrush(Colors.Gray);
                continueButton.Disable();
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
