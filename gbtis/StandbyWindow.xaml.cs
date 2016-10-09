using System;
using System.Linq;
using System.Media;
using System.Timers;
using System.Windows;
using System.Windows.Media;

namespace gbtis {
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class StandbyWindow : Window {
        // Settings for the text fade effect
        public static int nameFadeInterval = 5000;

        // Events
        public event EventHandler Exit;

        // For the kinect display
        Kinect kinect;

        /// <summary>
        /// Constructor for the standby window
        /// </summary>
        /// <param name="_sensor">An open kinect sensor, or null</param>
        public StandbyWindow(Kinect kinect) {
            InitializeComponent();
            this.kinect = kinect;

            // Initialize load text
            standbyMsg.Text = (kinect.isAvailable()) ? 
                gbtis.Properties.Resources.msgStart :
                gbtis.Properties.Resources.msgNoSensor;

            kinect.BitMapReady += BitMapArrived;
            kinect.WaveGestureOccured += WaveGestureArrived;
            kinect.EasterEggGestureOccured += EasterEggArrived;

            // Initialize the names
            topName.Text = "";
            centerName.Text = "";
            bottomName.Text = "";

            // Start the timer for the name animation
            Timer timer = new Timer(nameFadeInterval);
            timer.Elapsed += HandleTimer;
            timer.Start();
            HandleTimer(null, null);
        }

        /// <summary>
        /// Update the camera feed from the sensor
        /// </summary>
        /// <param name="img">The latest ImageSource</param>
        void BitMapArrived(ImageSource img) {
            this.Dispatcher.Invoke(new Action(() =>
                sensorFeed.Source = img));
        }

        private void WaveGestureArrived() {
            this.Dispatcher.Invoke(() => {
                standbyMsg.Text = "You just waved!";
            });
        }

        private void EasterEggArrived() {
            this.Dispatcher.Invoke(() => {
                standbyMsg.Text = "Bruh.";
                SoundPlayer player = new SoundPlayer(
                    @"..\..\Resources\excellent.wav");
                player.Play();
            });
        }

        /// <summary>
        /// Handle a tick for the names animation
        /// </summary>
        /// <param name="source">Source of the event</param>
        /// <param name="e">Arguments to the event</param>
        private void HandleTimer(Object source, ElapsedEventArgs e) {
            try {
                this.Dispatcher.Invoke(() => {
                    int i = selector.Next(names.Count());

                    new TextBlockFadeAnimation(topName, names[i]);
                    new TextBlockFadeAnimation(centerName, names[(i + 1) % names.Count()]);
                    new TextBlockFadeAnimation(bottomName, names[(i + 2) % names.Count()]);
                });
            } catch (OperationCanceledException) { }
        }

        /// <summary>
        /// Called when the window is closed
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="args">The event arguments</param>
        private void Window_Closed(object sender, EventArgs args) {
            Exit?.Invoke(this, args);
        }

        // Temporary source of names to display.
        private static Random selector = new Random();
        private static String[] names = {
                "Ringo Starr",
                "Floobo Bopity",
                "Maximum Overdoot",
                "Fred Johnson",
                "Grabby Mcgee",
                "Hugh Mungus",
            };
    }
}
