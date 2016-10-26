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
    /// <summary>
    /// Interaction logic for Cursor.xaml
    /// </summary>
    public partial class Cursor : UserControl {
        public const int ERASER_SIZE = 100;
        public const int DEFAULT_SIZE = 10;

        public delegate void PositionEventHandler(Point p);
        public event PositionEventHandler Moved;

        /// <summary>
        /// Cursor bounds
        /// </summary>
        private int _size;
        private Point _position;
        public Point Position {
            get { return new Point(_position.X + ActualWidth / 2, _position.Y + ActualWidth / 2); }
            set {
                value = new Point(value.X - ActualWidth / 2, value.Y - ActualHeight / 2);
                _position = value;
                Margin = new Thickness(value.X, value.Y, 0, 0);
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

        /// <summary>
        /// Boundary of the cursor
        /// </summary>
        public Rect Rect {
            get {
                return new Rect(
                    new Point(Position.X - _size / 2, Position.Y - _size / 2), 
                    new Size(_size, _size));
            }
        }

        public Cursor() {
            InitializeComponent();
            toggleIdle(true);
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
