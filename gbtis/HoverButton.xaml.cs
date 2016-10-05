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
        public const float COMPLETION_TIME = 0.75f;
        public const float UNDO_MULTIPLIER = 2.0f;

        private float completion;
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
            over = done = false;
            completion = 0;

            Timer t = new Timer(50);
            t.Elapsed += T_Elapsed;
            t.Start();
        }

        public bool CursorOver(Point cursor) {
            Window parentWindow = Window.GetWindow(this);
            Point self = parentWindow.TransformToDescendant(this).Transform(cursor);
            over =
                self.X >= 0 && self.X <= ActualWidth &&
                self.Y >= 0 && self.Y <= ActualHeight;
            return over;
        }

        private void clicked() {
            if (done) return;
            done = true;

            Clicked?.Invoke(this, new EventArgs());

            try {
                this.Dispatcher.Invoke(() => {
                    borderBox.BorderBrush = new SolidColorBrush(Colors.Black);
                    textBox.Foreground = new SolidColorBrush(Colors.Black);
                });
            } catch (OperationCanceledException) { }
        }

        private void T_Elapsed(object sender, ElapsedEventArgs e) {
            if (!over) {
                completion -= completionRate() * UNDO_MULTIPLIER;
                if (completion < 0) completion = 0;

                if (done) {
                    done = false;

                    try {
                        this.Dispatcher.Invoke(() => {
                            borderBox.BorderBrush = new SolidColorBrush(Colors.Teal);
                            textBox.Foreground = new SolidColorBrush(Colors.Teal);
                        });
                    } catch (OperationCanceledException) { }
                }
            }

            if (over && !done) {
                completion += completionRate();
                if (completion > 1) {
                    completion = 1;
                    clicked();
                }
            }

            try {
                this.Dispatcher.Invoke(() => {
                    double radius = (ActualHeight/2) * completion;
                    borderBox.CornerRadius = new CornerRadius(radius);
                });
            } catch (OperationCanceledException) { }
        }

        private float completionRate() {
            return 1f / COMPLETION_TIME * 0.05f;
        }
    }
}
