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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace gbtis.Controls {
    /// <summary>
    /// Interaction logic for Help.xaml
    /// </summary>
    public partial class Help : UserControl {
        public Help() {
            InitializeComponent();

            Loaded += (s, e) => {
                Wave_Frame1();
                Draw_Frame1();
            };
        }

        public void AnimateOpacity(double newOpacity) {
            Storyboard board = new Storyboard();

            DoubleAnimation animation = new DoubleAnimation(newOpacity, new Duration(TimeSpan.FromSeconds(1.5)));
            animation.AccelerationRatio = animation.DecelerationRatio = 0.1;
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath(UserControl.OpacityProperty));
            board.Children.Add(animation);

            board.Begin(this);
        }

        /// <summary>
        /// DRAWING ANIMATION BEGIN
        /// </summary>

        private void Draw_Frame1() {
            drawnLine.Opacity = 1;
            cursorHand.Opacity = 1;
            var handPoint = cursorHand.TransformToAncestor(this)
                .Transform(new Point(0, 0));
            drawnLine.X1 = drawnLine.X2 = handPoint.X + cursorHand.ActualWidth / 2;
            drawnLine.Y1 = drawnLine.Y2 = handPoint.Y + 15;

            cursorHand.Source = new BitmapImage(new Uri("/Resources/Lasso.png", UriKind.Relative));
            Storyboard board = new Storyboard();

            ThicknessAnimation moveRight = new ThicknessAnimation(new Thickness(400, 0, 0, 0), new Duration(TimeSpan.FromSeconds(1.5)));
            moveRight.AccelerationRatio = moveRight.DecelerationRatio = 0.1;
            Storyboard.SetTarget(moveRight, cursorHand);
            Storyboard.SetTargetProperty(moveRight, new PropertyPath(Image.MarginProperty));
            board.Children.Add(moveRight);

            cursorHand.LayoutUpdated += LineEndTracking;
            cursorHand.LayoutUpdated -= LineStartTracking;

            board.Completed += (s, e) => Draw_Frame2();
            board.Begin(this);
        }

        private void Draw_Frame2() {
            cursorHand.Source = new BitmapImage(new Uri("/Resources/Closed.png", UriKind.Relative));
            cursorHand.Opacity = 0.7;
            Storyboard board = new Storyboard();

            ThicknessAnimation moveLeft = new ThicknessAnimation(new Thickness(0, 50, 0, 0), new Duration(TimeSpan.FromSeconds(1)));
            moveLeft.AccelerationRatio = moveLeft.DecelerationRatio = 0.1;
            Storyboard.SetTarget(moveLeft, cursorHand);
            Storyboard.SetTargetProperty(moveLeft, new PropertyPath(Image.MarginProperty));
            board.Children.Add(moveLeft);

            cursorHand.LayoutUpdated -= LineEndTracking;
            cursorHand.LayoutUpdated -= LineStartTracking;

            board.Completed += (s, e) => Draw_Frame3();
            board.Begin(this);
        }

        private void Draw_Frame3() {
            Storyboard board = new Storyboard();

            ThicknessAnimation moveLeft = new ThicknessAnimation(new Thickness(-400, 0, 0, 0), new Duration(TimeSpan.FromSeconds(1)));
            moveLeft.AccelerationRatio = moveLeft.DecelerationRatio = 0.1;
            Storyboard.SetTarget(moveLeft, cursorHand);
            Storyboard.SetTargetProperty(moveLeft, new PropertyPath(Image.MarginProperty));
            board.Children.Add(moveLeft);

            board.Completed += (s, e) => Draw_Frame4();
            board.Begin(this);
        }

        private void Draw_Frame4() {
            cursorHand.Source = new BitmapImage(new Uri("/Resources/Open.png", UriKind.Relative));
            cursorHand.Opacity = 1;
            Storyboard board = new Storyboard();

            ThicknessAnimation moveRight = new ThicknessAnimation(new Thickness(400, 0, 0, 0), new Duration(TimeSpan.FromSeconds(1.5)));
            moveRight.AccelerationRatio = moveRight.DecelerationRatio = 0.1;
            Storyboard.SetTarget(moveRight, cursorHand);
            Storyboard.SetTargetProperty(moveRight, new PropertyPath(Image.MarginProperty));
            board.Children.Add(moveRight);

            cursorHand.LayoutUpdated -= LineEndTracking;
            cursorHand.LayoutUpdated += LineStartTracking;

            board.Completed += (s, e) => {
                drawnLine.Opacity = 0;
                Draw_Frame5();
            };
            board.Begin(this);
        }

        private void Draw_Frame5() {
            cursorHand.Source = new BitmapImage(new Uri("/Resources/Closed.png", UriKind.Relative));
            cursorHand.Opacity = 0.7;
            Storyboard board = new Storyboard();

            ThicknessAnimation moveLeft = new ThicknessAnimation(new Thickness(0, 50, 0, 0), new Duration(TimeSpan.FromSeconds(1)));
            moveLeft.AccelerationRatio = moveLeft.DecelerationRatio = 0.1;
            Storyboard.SetTarget(moveLeft, cursorHand);
            Storyboard.SetTargetProperty(moveLeft, new PropertyPath(Image.MarginProperty));
            board.Children.Add(moveLeft);

            cursorHand.LayoutUpdated -= LineEndTracking;
            cursorHand.LayoutUpdated -= LineStartTracking;

            board.Completed += (s, e) => Draw_Frame6();
            board.Begin(this);
        }

        private void Draw_Frame6() {
            Storyboard board = new Storyboard();

            ThicknessAnimation moveLeft = new ThicknessAnimation(new Thickness(-400, 0, 0, 0), new Duration(TimeSpan.FromSeconds(1)));
            moveLeft.AccelerationRatio = moveLeft.DecelerationRatio = 0.1;
            Storyboard.SetTarget(moveLeft, cursorHand);
            Storyboard.SetTargetProperty(moveLeft, new PropertyPath(Image.MarginProperty));
            board.Children.Add(moveLeft);

            board.Completed += (s, e) => Draw_Frame1();
            board.Begin(this);
        }

        /// <summary>
        /// DRAWING ANIMATION ENDS
        /// </summary>

        /// <summary>
        /// WAVE ANIMATION BEGIN
        /// </summary>

        private void Wave_Frame1() {
            ((Storyboard)waveHand.FindResource("wave")).Begin();
        }

        /// <summary>
        /// WAVE ANIMATION ENDS
        /// </summary>

        private void LineStartTracking(Object source, EventArgs args) {
            LineTracking(false);
        }

        private void LineEndTracking(Object source, EventArgs args) {
            LineTracking(true);
        }

        private void LineTracking(bool end) {
            var handPoint = cursorHand.TransformToAncestor(this)
                .Transform(new Point(0, 0));

            if (end) {
                drawnLine.X2 = handPoint.X + cursorHand.ActualWidth / 2;
                drawnLine.Y2 = handPoint.Y + 15;
            } else {
                drawnLine.X1 = handPoint.X + cursorHand.ActualWidth / 2;
                drawnLine.Y1 = handPoint.Y + 15;
            }
        }
    }
}