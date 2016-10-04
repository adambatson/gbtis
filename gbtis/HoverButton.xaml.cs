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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace gbtis {
    /// <summary>
    /// Interaction logic for HoverButton.xaml
    /// </summary>
    public partial class HoverButton : UserControl {
        public const double BOUNCE_RATE = 3.5;
        private bool over;
        private bool done;

        public event EventHandler Clicked;

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(String), typeof(HoverButton), new PropertyMetadata(""));
        public String Text {
            get { return (String)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public HoverButton() {
            InitializeComponent();
            over = false;

            Timer t = new Timer(50);
            t.Elapsed += T_Elapsed;
            t.Start();
        }

        public bool CursorOver(Point cursor) {
            Point self = this.PointFromScreen(cursor);
            over =
                self.X >= 0 && self.X < ActualWidth &&
                self.Y >= 0 && self.Y < ActualHeight;
            return over;
        }

        private void clicked() {
            if (done) return;

            Clicked?.Invoke(this, new EventArgs());
            borderBox.BorderBrush = new SolidColorBrush(Colors.Black);
            textBox.Foreground = new SolidColorBrush(Colors.Black);
            done = true;
        }

        private void T_Elapsed(object sender, ElapsedEventArgs e) {
            try {
                this.Dispatcher.Invoke(() => {
                    if (done && !over) {
                        done = false;
                        borderBox.BorderBrush = new SolidColorBrush(Colors.Teal);
                        textBox.Foreground = new SolidColorBrush(Colors.Teal);
                    }

                    double radius = borderBox.CornerRadius.TopLeft + ((over) ? BOUNCE_RATE : -BOUNCE_RATE);
                    if (radius < 0) radius = 0;
                    if (radius > ActualHeight / 2) {
                        radius = ActualHeight / 2;
                        clicked();
                    }
                    borderBox.CornerRadius = new CornerRadius(radius);
                });
            } catch (OperationCanceledException) { }
        }
    }
}
