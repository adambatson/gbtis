using gbtis.Windows;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace gbtis.Controls {
    /// <summary>
    /// Interaction logic for Cursor.xaml
    /// </summary>
    public partial class Cursor : UserControl {
        public const int ERASER_SIZE = 100;
        public const int DEFAULT_SIZE = 10;
        
        // Movement event
        public delegate void PositionEventHandler(Point p);
        public event PositionEventHandler Moved;

        // Mode events
        public delegate void ModeEventHandler(CursorModes m);
        public event ModeEventHandler ModeStart;
        public event ModeEventHandler ModeEnd;

        /// <summary>
        /// Cursor bounds
        /// </summary>
        private int _size;
        public int Size { get { return _size; } }

        // Parent bounds
        private Size offset;
        private Window parentWindow;
        private Size parentBounds;

        /// <summary>
        /// Cursor position
        /// </summary>
        private Point _position;
        public Point Position {
            get { return new Point(
                (_position.X + ActualWidth / 2) - offset.Width, 
                (_position.Y + ActualHeight / 2) - offset.Height); }
            set {
                value = new Point(
                    value.X - ActualWidth / 2, 
                    value.Y - ActualHeight / 2);

                try {
                    Margin = new Thickness(value.X - offset.Width, value.Y - offset.Height, 0, 0);
                } catch (Exception) { return; }
                _position = value;
                Moved?.Invoke(Position);
            }
        }

        /// <summary>
        /// Current cursor display mode
        /// </summary>
        private CursorModes _mode;
        public CursorModes Mode {
            get { return _mode; }
            set { _mode=value; setMode(value); }
        }

        public void SetBounds(Size bounds, Size offset) {
            parentBounds = bounds;
            this.offset = offset;
        }

        public Cursor() {
            InitializeComponent();
            toggleIdle(true);
            Kinect kinect = Kinect.getInstance();
            parentBounds = new Size(0, 0);

            Loaded += (source, args) => {
                parentWindow = Window.GetWindow(this);

                // Kinect controls
                kinect.FingerPositionChanged += (p) => {
                    Position = kinect.ColorToInterface(p, parentBounds);
                    Mode = kinect.CursorMode;
                };
                kinect.ModeStart += (m) => {
                    Position = kinect.ColorToInterface(kinect.FingerPosition, parentBounds);
                    Mode = m;

                    ModeStart?.Invoke(m);
                };
                kinect.ModeEnd += (m) => {
                    Position = kinect.ColorToInterface(kinect.FingerPosition, parentBounds);
                    Mode = m;

                    ModeEnd?.Invoke(m);
                };
                
                // Mouse controls
                parentWindow.MouseMove += (s, e) => {
                    Position = Mouse.GetPosition(parentWindow);
                    Mode = mouseMode();

                    Moved?.Invoke(Position);
                };
                parentWindow.PreviewMouseDown += (s, e) => {
                    Position = Mouse.GetPosition(parentWindow);
                    Mode = mouseMode();

                    ModeStart?.Invoke(Mode);
                };
                parentWindow.PreviewMouseUp += (s, e) => {
                    Position = Mouse.GetPosition(parentWindow);
                    Mode = mouseMode();

                    ModeEnd?.Invoke(Mode);
                };
            };
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
        /// Change the cursor display mode
        /// </summary>
        /// <param name="mode">Cursor mode to use</param>
        private void setMode(CursorModes mode) {
            switch (mode) {
                case CursorModes.Idle:
                    toggleIdle(true);
                    toggleErase(false);
                    break;
                case CursorModes.Draw:
                    toggleErase(false);
                    toggleIdle(false);
                    break;
                case CursorModes.Erase:
                    toggleErase(true);
                    toggleIdle(false);
                    break;
            }
        }

        /// <summary>
        /// Show or hide eraser cursor
        /// </summary>
        /// <param name="on">True for show, false for hide</param>
        private void toggleErase(bool on) {
            if (on) _size = ERASER_SIZE;
            erase.Background = on ? new SolidColorBrush(Colors.White) : null;
            erase.BorderBrush = on ? new SolidColorBrush(Colors.Black) : null;
        }

        /// <summary>
        /// Show or hide idle cursor
        /// </summary>
        /// <param name="on">True for show, false for hide</param>
        public void toggleIdle(bool on) {
            if (on) _size = DEFAULT_SIZE;
            idle.Opacity = on ? 1 : 0;
        }
    }
}
