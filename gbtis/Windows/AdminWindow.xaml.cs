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

        public bool DefaultToPrimaryScreen { get; set; } = false;
        public System.Windows.Forms.Screen PrimaryMonitor { get; private set; }

        // Window events
        public event EventHandler Exit;
        public event EventHandler Standby;

        public delegate void ScreenEvent();
        public event ScreenEvent ScreenChanged;

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

            // Monitor selection
            var i = 0;
            foreach (var screen in System.Windows.Forms.Screen.AllScreens) {
                MenuItem item = new MenuItem();
                item.Header = String.Format("Display {0} ({1}x{2})", ++i, screen.WorkingArea.Width, screen.WorkingArea.Height);
                item.Tag = screen;
                item.Click += (s, e) => {
                    foreach (MenuItem mi in windowMenu.Items) mi.IsChecked = false;
                    ((MenuItem)s).IsChecked = true;
                    SetScreen((System.Windows.Forms.Screen)((MenuItem)s).Tag);
                };

                // Set the default to the first non-primary display
                if (PrimaryMonitor == null) {
                    if (!DefaultToPrimaryScreen && !screen.Primary || DefaultToPrimaryScreen && screen.Primary) {
                        PrimaryMonitor = screen;
                    }
                }

                item.IsChecked = (screen == PrimaryMonitor);
                windowMenu.Items.Add(item);
            }

            // Realign
            ScreenChanged?.Invoke();
        }

        /// <summary>
        /// Change the active screen
        /// </summary>
        /// <param name="s"></param>
        private void SetScreen(System.Windows.Forms.Screen s) {
            PrimaryMonitor = s;
            ScreenChanged?.Invoke();
        }

        /// <summary>
        /// Align a window to the active monitor
        /// </summary>
        /// <param name="w">Window to align</param>
        public void AlignWindow(Window w) {
            w.WindowState = WindowState.Normal;
            var area = PrimaryMonitor.WorkingArea;

            w.Left = area.Left;
            w.Top = area.Top;
            w.Width = area.Width;
            w.Height = area.Height;
            w.WindowState = WindowState.Maximized;
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
                int i = 1;
                foreach (ulong bodyID in bodyPositions.Keys) {
                    Pip pip = new Controls.Pip(i.ToString());
                    pip.Position = kinect.ScaleToSize(new Point(bodyPositions[bodyID].X, bodyPositions[bodyID].Y), new Size(frameSize.Width, frameSize.Height), new Size(this.ActualWidth, this.ActualHeight));
                    pip.Active = bodyID == activeBodyID;
                    pipContainer.Children.Add(pip);
                    pip.Tag = bodyID;
                    pip.MouseDoubleClick += (s, e) => kinect.SetActiveBody((ulong)((Pip)s).Tag);
                    i++;
                }
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
                
                if (frameSize.Width == 0) {
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
