using gbtis.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace gbtis.Windows {
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window {
        private Kinect kinect;

        // Window events
        public event EventHandler Exit;
        public event EventHandler Standby;

        /// <summary>
        /// Initialize the window
        /// </summary>
        public AdminWindow() {
            DataContext = this;
            InitializeComponent();

            kinect = Kinect.getInstance();
            kinect.BitMapReady += Kinect_BitMapArrived;
            kinect.SensorStatusChanged += Kinect_SensorStatusChanged;
        }

        private void Kinect_SensorStatusChanged(bool isAvailable) {
            this.Dispatcher.Invoke(new Action(() =>
                noKinect.Visibility = isAvailable ? Visibility.Visible : Visibility.Hidden));
        }

        /// <summary>
        /// Update the camera feed from the sensor
        /// </summary>
        /// <param name="img">The latest ImageSource</param>
        void Kinect_BitMapArrived(ImageSource img) {
            this.Dispatcher.Invoke(new Action(() => {
                sensorFeed.Source = img;
                noKinect.Visibility = Visibility.Visible;

                // Update pips
                pipContainer.Children.Clear();
                usersMenu.Items.Clear();
                ////// Foreach body
                Pip pip = new Controls.Pip(1.ToString()); //tag
                pip.Position = kinect.ScaleToSize(new Point(), new Size(img.Width, img.Height), new Size(this.ActualWidth, this.ActualHeight)); // Head location
                pip.Active = true; // If is active body
                pipContainer.Children.Add(pip);

                MenuItem item = new MenuItem();
                item.Header = 1.ToString(); //tag
                item.Tag = 1; //tag
                item.Click += (s,e) => { /* kinect.ActiveBody = (int)((MenuItem)s).Tag;*/ };
                usersMenu.Items.Add(item);
                //////

                usersMenu.IsEnabled = usersMenu.Items.Count > 0;
            }));
        }

        /// <summary>
        /// Update the frame in the video feed
        /// </summary>
        /// <param name="frame">The new frame</param>
        public void FrameArrived(ImageSource frame) {
            sensorFeed.Source = frame;
        }

        /// <summary>
        /// Close the program
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="args">Event args</param>
        private void FileExit_Click(object sender, EventArgs args) {
            Exit?.Invoke(this, args);
        }

        /// <summary>
        /// Return to standby mode
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event args</param>
        private void FileStandby_Click(object sender, EventArgs args) {
            Standby?.Invoke(this, args);
        }
    }
}
