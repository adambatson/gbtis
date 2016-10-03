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
        public CanvasWindow() {
            InitializeComponent();
            Mouse.OverrideCursor = Cursors.None;

            MouseMove += CanvasWindow_MouseMove;
            MouseLeftButtonDown += CanvasWindow_MouseLeftButtonDown;
            MouseRightButtonUp += CanvasWindow_MouseRightButtonUp;
            MouseRightButtonDown += CanvasWindow_MouseRightButtonDown;
        }

        private void CanvasWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Point p = Mouse.GetPosition(this);
            drawCanvas.mark(p, drawCursor.Size);
        }

        private void CanvasWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            drawCursor.Type = gbtis.Cursor.CursorType.Erase;
        }

        private void CanvasWindow_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            drawCursor.Type = gbtis.Cursor.CursorType.Draw;
        }

        private void CanvasWindow_MouseMove(object sender, MouseEventArgs e) {
            this.Dispatcher.Invoke(() => {
                Point p = Mouse.GetPosition(this);
                drawCursor.Position = new Point(
                    p.X,
                    p.Y
                );

                if (Mouse.LeftButton == MouseButtonState.Pressed) {
                    drawCanvas.mark(p, drawCursor.Size);
                } else if (Mouse.RightButton == MouseButtonState.Pressed) {
                    drawCanvas.erase(p, drawCursor.Size);
                }
            });
        }

        public void CursorType(Cursor.CursorType type) {
            drawCursor.Type = type;
        }
    }
}
