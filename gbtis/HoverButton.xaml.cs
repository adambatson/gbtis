using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace gbtis {
    /// <summary>
    /// Interaction logic for HoverButton.xaml
    /// </summary>
    public partial class HoverButton : UserControl {
        public const float COMPLETION_TIME = 0.75f;
        public const float UNDO_MULTIPLIER = 2.0f;
        public const float DISABLED_OPACITY = 0.3f;

        Timer t;
        private bool disabled;
        private float completion;
        private bool over;
        private bool done;
        private bool reset;

        public event EventHandler Clicked;

        /// <summary>
        /// Register the Text property of the hoverbutton control
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(HoverButton), new PropertyMetadata(""));
        public string Text {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Register the Color property of the hoverbutton control
        /// </summary>
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(HoverButton), new PropertyMetadata(Colors.Gray));
        public Color Color {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public HoverButton() {
            InitializeComponent();
            this.DataContext = this;

            over = done = reset = disabled = false;
            completion = 0;
            
            t = new Timer(50);
            t.Elapsed += T_Elapsed;
            t.Start();
        }
        
        /// <summary>
        /// Hover over the button
        /// </summary>
        /// <param name="over">True to activate</param>
        public void Over(bool over) {
            this.over = !disabled && over;
        }

        public bool Intersects(Visual parent, Point p) {
            Rect bounds = TransformToVisual(parent).TransformBounds(new Rect(RenderSize));
            return bounds.Contains(p);
        }

        /// <summary>
        /// Disable the button
        /// </summary>
        public void Disable() {
            disabled = true;
            borderBox.Opacity = DISABLED_OPACITY;
        }

        /// <summary>
        /// Enable the button
        /// </summary>
        public void Enable() {
            disabled = false;
            borderBox.Opacity = 1;
        }

        /// <summary>
        /// Timed event to check the button's progress
        /// </summary>
        /// <param name="sender">Source of the event</param>
        /// <param name="e">Event args</param>
        private void T_Elapsed(object sender, ElapsedEventArgs e) {
            completion += (over) ? completionRate() : -completionRate() * UNDO_MULTIPLIER;
            if (completion < 0) completion = 0;
            if (completion > 1) completion = 1;

            // Click the button
            if (over && !done && completion >= 1) {
                done = true;
                Clicked?.Invoke(this, new EventArgs());

                try {
                    this.Dispatcher.Invoke(() => {
                        textBox.Foreground = new SolidColorBrush(Colors.Black);
                        reset = true;
                    });
                } catch (OperationCanceledException) { }
            }

            // All done, reset everything
            if (!over && reset) {
                done = reset = false;
                completion = 0;

                try {
                    this.Dispatcher.Invoke(() => {
                        textBox.Foreground = new SolidColorBrush(Colors.White);
                        reset = true;
                    });
                } catch (OperationCanceledException) { }
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
