using Microsoft.Kinect;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace gbtis.Controls {
    /// <summary>
    /// Interaction logic for Help.xaml
    /// </summary>
    public partial class Help : UserControl {
        public const double INVISIBLE = 0;
        public const double PARTIALLY_VISIBLE = 0.7;
        public const double VISIBLE = 1;
        public const int SHOW_FOR = 10000;
        public static readonly PointF HAND_OFFSET = new PointF{ X=0.1f, Y=0.1f };

        private LineTracking tracking;

        public Help() {
            InitializeComponent();

            Loaded += (s, e) => {
                tracking = LineTracking.Both;
                cursorHand_LayoutUpdated(null, null);

                tracking = LineTracking.End;
                StartStoryboard(waveHand, "Wave");
                StartStoryboard(cursorHand, "Frame1");
            };
        }

        /// <summary>
        /// Display the overlay
        /// </summary>
        public void Show() {
            Timer t = new Timer(SHOW_FOR);
            t.Elapsed += (s, e) => Dispatcher.Invoke(new Action(() => {
                StartStoryboard(this, "FadeOut");
            }));

            StartStoryboard(this, "FadeIn");
            t.Start();
        }

        /// <summary>
        /// Allows the line to track the hand
        /// </summary>
        /// <param name="sender">The hand</param>
        /// <param name="e">Event args</param>
        private void cursorHand_LayoutUpdated(object sender, EventArgs e) {
            if (tracking == LineTracking.None) return;

            // Get position of the hand
            var handPoint = cursorHand.TransformToAncestor(this)
                .Transform(new Point(0, 0));
            
            if (tracking == LineTracking.Start || tracking == LineTracking.Both) {
                drawnLine.X2 = handPoint.X + cursorHand.ActualWidth / 2;
                drawnLine.Y2 = handPoint.Y + cursorHand.ActualWidth * HAND_OFFSET.X;
            }

            if (tracking == LineTracking.End || tracking == LineTracking.Both) {
                drawnLine.X1 = handPoint.X + cursorHand.ActualWidth / 2;
                drawnLine.Y1 = handPoint.Y + cursorHand.ActualWidth * HAND_OFFSET.Y;
            }
        }

        /// <summary>
        /// Drawing right
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Frame1_Completed(object sender, EventArgs e) {
            cursorHand.Source = OpenResource("Closed.png");
            tracking = LineTracking.None;
            cursorHand.Opacity = PARTIALLY_VISIBLE;
            drawnLine.Opacity = INVISIBLE;

            StartStoryboard(cursorHand, "Frame2");
        }

        /// <summary>
        /// First half of idle motion left
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Frame2_Completed(object sender, EventArgs e) {
            StartStoryboard(cursorHand, "Frame3");
        }

        /// <summary>
        /// Left idle motion finishes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Frame3_Completed(object sender, EventArgs e) {
            cursorHand.Source = OpenResource("Open.png");
            tracking = LineTracking.Start;
            cursorHand.Opacity = VISIBLE;
            drawnLine.Opacity =  VISIBLE;

            StartStoryboard(cursorHand, "Frame4");
        }

        /// <summary>
        /// Erase right ends
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Frame4_Completed(object sender, EventArgs e) {
            cursorHand.Source = OpenResource("Closed.png");
            tracking = LineTracking.None;
            cursorHand.Opacity = PARTIALLY_VISIBLE;
            drawnLine.Opacity = INVISIBLE;

            StartStoryboard(cursorHand, "Frame5");
        }

        /// <summary>
        /// First half of return left
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Frame5_Completed(object sender, EventArgs e) {
            StartStoryboard(cursorHand, "Frame6");
        }

        /// <summary>
        /// Return left complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Frame6_Completed(object sender, EventArgs e) {
            cursorHand.Source = OpenResource("Lasso.png");
            tracking = LineTracking.End;
            cursorHand.Opacity = VISIBLE;
            drawnLine.Opacity = VISIBLE;

            StartStoryboard(cursorHand, "Frame1");
        }

        private BitmapImage OpenResource(string path) {
            return new BitmapImage(new Uri(string.Format("/Resources/{0}", path), UriKind.Relative));
        }
 
        private void StartStoryboard(FrameworkElement source, String key) {
            ((Storyboard)source.FindResource(key)).Begin();
        }

        private enum LineTracking {
            None, Start, End, Both
        }
    }
}