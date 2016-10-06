using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        Timer t;
        private float completion;
        private bool over;
        private bool done;
        private bool reset;

        public event EventHandler Clicked;

        /// <summary>
        /// Register the Text property of the hoverbutton control
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(String), typeof(HoverButton), new PropertyMetadata(""));
        public String Text {
            get { return (String)this.GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Register the Color property of the hoverbutton control
        /// </summary>
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(HoverButton), new PropertyMetadata(Colors.Black));
        public Color Color {
            get { return (Color)this.GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public HoverButton() {
            InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;
            over = done = reset = false;
            completion = 0;
            
            t = new Timer(50);
            t.Elapsed += T_Elapsed;
            t.Start();
        }

        /// <summary>
        /// Test the cursor's position against the button
        /// </summary>
        /// <param name="cursor">Coordinates of the cursor</param>
        /// <returns></returns>
        public bool CursorOver(Point cursor) {
            Window parentWindow = Window.GetWindow(this);
            Point self = parentWindow.TransformToDescendant(this).Transform(cursor);
            over =
                self.X >= 0 && self.X <= ActualWidth &&
                self.Y >= 0 && self.Y <= ActualHeight;
            return over;
        }

        /// <summary>
        /// Activates the Clicked event and changes the look of the control
        /// </summary>
        private void clicked() {
            if (done) return;
            done = true;

            Clicked?.Invoke(this, new EventArgs());

            try {
                this.Dispatcher.Invoke(() => {
                    textBox.Foreground = new SolidColorBrush(Colors.Black);
                    reset = true;
                });
            } catch (OperationCanceledException) { }
        }

        /// <summary>
        /// Timed event to check the button's progress
        /// </summary>
        /// <param name="sender">Source of the event</param>
        /// <param name="e">Event args</param>
        private void T_Elapsed(object sender, ElapsedEventArgs e) {
            if (!over) {
                // User is not on the button. Reverse progress
                completion -= completionRate() * UNDO_MULTIPLIER;
                if (completion < 0) completion = 0;

                // No longer over the button, and finished the animation. Reset.
                if (done) {
                    done = false;
                    completion = 0;
                }

                if (reset) {
                    try {
                        this.Dispatcher.Invoke(() => {
                            textBox.Foreground = new SolidColorBrush(Colors.White);
                        });
                    } catch (OperationCanceledException) { }
                }
            }

            // Over the button. Progress towards the event
            if (over && !done) {
                completion += completionRate();
                if (completion >= 1) {
                    completion = 1;
                    clicked();
                }
            }

            // Update Button look to reflect progress
            try {
                this.Dispatcher.Invoke(() => {
                    progressBlack.Offset = progressColor.Offset = 1 - completion;
                });
            } catch (OperationCanceledException) { }
        }

        /// <summary>
        /// Turn the completion time from seconds to a delta per tick
        /// </summary>
        /// <returns>Completion delta</returns>
        private float completionRate() {
            return 1f / COMPLETION_TIME * 0.05f;
        }
    }
}
