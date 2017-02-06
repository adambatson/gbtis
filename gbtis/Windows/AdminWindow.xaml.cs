using gbtis.Controls;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace gbtis.Windows {
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window {
        private Kinect kinect;
        private Size frameSize;

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
            kinect.BodyPositionsChanged += Kinect_BodyPositionsChanged;
        }

        /// <summary>
        /// Kinect lost / arrived
        /// </summary>
        /// <param name="isAvailable">True if the kinect is present</param>
        private void Kinect_SensorStatusChanged(bool isAvailable) {
            this.Dispatcher.Invoke(new Action(() =>
                noKinect.Visibility = isAvailable ? Visibility.Visible : Visibility.Hidden));
        }

        /// <summary>
        /// Change in body tracking
        /// </summary>
        /// <param name="bodyPositions">List of valid bodies</param>
        /// <param name="activeBodyID">ID kinect is tracking actively</param>
        void Kinect_BodyPositionsChanged(Dictionary<ulong, ColorSpacePoint> bodyPositions, ulong? activeBodyID) {
            this.Dispatcher.Invoke(new Action(() => {
                // Update pips
                pipContainer.Children.Clear();
                usersMenu.Items.Clear();

                foreach (ulong bodyID in bodyPositions.Keys) {
                    Pip pip = new Controls.Pip(bodyID.ToString());
                    pip.Position = kinect.ScaleToSize(new Point(bodyPositions[bodyID].X, bodyPositions[bodyID].Y), new Size(frameSize.Width, frameSize.Height), new Size(this.ActualWidth, this.ActualHeight));
                    pip.Active = bodyID == activeBodyID;
                    pipContainer.Children.Add(pip);

                    MenuItem item = new MenuItem();
                    item.Header = bodyID.ToString();
                    item.Tag = bodyID;
                    item.Click += (s, e) => kinect.SetActiveBody((ulong)((MenuItem)s).Tag);
                    usersMenu.Items.Add(item);
                }

                usersMenu.IsEnabled = usersMenu.Items.Count > 0;
            }));
        }

        /// <summary>
        /// Update the camera feed from the sensor
        /// </summary>
        /// <param name="img">The latest ImageSource</param>
        void Kinect_BitMapArrived(ImageSource img) {
            this.Dispatcher.Invoke(new Action(() => {
                sensorFeed.Source = img;
                noKinect.Visibility = Visibility.Hidden;
                
                if (frameSize.IsEmpty) {
                    frameSize = new Size(img.Width, img.Height);
                }
            }));
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
