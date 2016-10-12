using Microsoft.Kinect;
using System;
using System.Collections.Generic;
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

namespace gbtis {
    public partial class CanvasCursor : UserControl {
        public event EventHandler Moved;
        public event EventHandler Draw;
        public event EventHandler Erase;
        public event EventHandler Idle;

        private Kinect kinect;

        // Size of the cursor brush
        public int Size { get; set; }
        
        // The type of cursor
        private CursorType type;
        public CursorType Type {
            get { return type; }
            set {
                type = value;
                brushProperties(value);
            }
        }

        // Position of the cursor's center
        private Point position;
        public Point Position {
            get { return position; }
            set {
                position = value;

                // The actual position is the center
                Margin = new Thickness(
                    value.X - Width / 2,
                    value.Y - Height / 2,
                    0, 0
                );
            }
        }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public CanvasCursor() {
            InitializeComponent();
            Type = CursorType.Idle;
        }

        public void setKinect(Kinect kinect) {
            this.kinect = kinect;
            if (true || kinect.isAvailable()) {
                kinect.FingerPositionChanged += (p) => {
                    this.Dispatcher.Invoke(new Action(() => {
                        Position = p;
                        if (Type == CursorType.Draw)
                            cursorDraw();
                        else if (Type == CursorType.Erase)
                            cursorErase();
                        else if (Type == CursorType.Idle)
                            cursorIdle();
                    }));

                    Moved?.Invoke(this, new EventArgs());

                    kinect.ModeStart += (m) => {
                        if (m == CursorType.Draw)
                            cursorDraw();
                        else if (m == CursorType.Erase)
                            cursorErase();
                        else if (m == CursorType.Idle)
                            cursorIdle();
                    };

                    kinect.ModeEnd += (m) => cursorIdle();
                };
            } else {
                this.Dispatcher.Invoke(new Action(() => {
                    Window parentWindow = Window.GetWindow(this);
                    parentWindow.MouseMove += (s, e) => {
                        Position = RelativePosition(parentWindow);

                        if (Mouse.LeftButton == MouseButtonState.Pressed)
                            cursorDraw();
                        else if (Mouse.RightButton == MouseButtonState.Pressed)
                            cursorErase();

                        Moved?.Invoke(this, new EventArgs());
                    };
                }));

                MouseLeftButtonDown += (s, e) => cursorDraw();
                MouseRightButtonDown += (s, e) => cursorErase();

                MouseLeftButtonUp += (s, e) => cursorIdle();
                MouseRightButtonUp += (s, e) => cursorIdle();
            }
        }

        /// <summary>
        /// Get the current cursor position relative to another element
        /// </summary>
        /// <param name="relativeTo">Relative element</param>
        /// <returns>Coordinates of the cursor</returns>
        public Point RelativePosition(FrameworkElement relativeTo) {
            if (kinect.isAvailable()) { // 1920 x 1080
                return new Point(
                    relativeTo.ActualWidth * Position.X / 1920,
                    relativeTo.ActualHeight * Position.Y / 1080
                );
            }

            return Mouse.GetPosition(relativeTo);
        }

        /// <summary>
        /// Draw at cursor
        /// </summary>
        private void cursorDraw() {
            Type = CanvasCursor.CursorType.Draw;
            Draw?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Erase at cursor
        /// </summary>
        private void cursorErase() {
            Type = CanvasCursor.CursorType.Erase;
            Erase?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Return to idle
        /// </summary>
        private void cursorIdle() {
            Type = CanvasCursor.CursorType.Idle;
            Idle?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Set a brush's properties to match a type
        /// </summary>
        /// <param name="type">The type of cursor</param>
        private void brushProperties(CursorType type) {
            content.CornerRadius = new CornerRadius(type.round ? type.size / 2 : 0);
            content.Background = new SolidColorBrush(type.background);
            content.BorderBrush = new SolidColorBrush(type.border);

            //Resize
            Width = Height = type.size;
            Position = Position;
            Size = type.size;
        }

        /// <summary>
        /// Represents one of the cursor's modes
        /// </summary>
        public class CursorType {
            public Color background { get; set; }
            public Color border { get; set; }
            public bool round { get; set; }
            public int size { get; set; }

            /// <summary>
            /// Full constructor
            /// </summary>
            /// <param name="background">Backround color for the brush</param>
            /// <param name="border">Border color for the brush</param>
            /// <param name="size">Diameter of the brush</param>
            /// <param name="round">If true, round the brush</param>
            public CursorType(Color background, Color border, int size, bool round) {
                this.background = background;
                this.border = border;
                this.size = size;
                this.round = round;
            }

            /// <summary>
            /// Static constructor
            /// </summary>
            /// <param name="background">Backround color for the brush</param>
            /// <param name="border">Border color for the brush</param>
            /// <param name="size">Diameter of the brush</param>
            /// <param name="round">If true, round the brush</param>
            /// <returns></returns>
            public static CursorType Create(Color background, Color border, int size, bool round) {
                return new CursorType(background, border, size, round);
            }

            public static readonly CursorType Missing = Create(Colors.White, Colors.White, 0, false);
            public static readonly CursorType Idle = Create(Colors.Gray, Colors.DarkSlateGray, 15, true);
            public static readonly CursorType Draw = Create(Colors.Black, Colors.White, 10, true);
            public static readonly CursorType Erase = Create(Colors.White, Colors.Black, 100, false);
        }
    }
}
