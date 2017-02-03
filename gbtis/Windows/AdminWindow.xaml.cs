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

            Kinect kinect = Kinect.getInstance();
            kinect.BitMapReady += BitMapArrived;

            // Monitor selection
            PrimaryMonitor = System.Windows.Forms.Screen.PrimaryScreen;
            ScreenChanged?.Invoke();
            var i = 0;
            foreach (var screen in System.Windows.Forms.Screen.AllScreens) {
                MenuItem item = new MenuItem();
                item.Header = String.Format("Display {0} ({1}x{2})", ++i, screen.WorkingArea.Width, screen.WorkingArea.Height);
                item.Tag = screen;
                item.Click += (s, e) => SetScreen((System.Windows.Forms.Screen)((MenuItem)s).Tag);

                windowMenu.Items.Add(item);
            }
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
        /// Update the camera feed from the sensor
        /// </summary>
        /// <param name="img">The latest ImageSource</param>
        void BitMapArrived(ImageSource img) {
            this.Dispatcher.Invoke(new Action(() =>
                sensorFeed.Source = img));
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
