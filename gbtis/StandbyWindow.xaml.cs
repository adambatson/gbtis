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
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace gbtis {
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class StandbyWindow : Window {
        // Settings for the text fade effect
        public static int nameTimerInterval = 100;
        public static double nameOpacityDelta = 0.05;

        // Selects random start indices for the text
        private static Random selector = new Random();

        // For the kinect display
        KinectSensor sensor;
        MultiSourceFrameReader frameReader;

        /// <summary>
        /// Constructor for the standby window
        /// </summary>
        /// <param name="_sensor">An open kinect sensor, or null</param>
        public StandbyWindow(KinectSensor _sensor) {
            InitializeComponent();

            // Initialize load text
            standbyMsg.Text = (sensor == null) ? 
                gbtis.Properties.Resources.msgNoSensor :
                gbtis.Properties.Resources.msgStart;

            this.sensor = _sensor;
            frameReader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color);
            frameReader.MultiSourceFrameArrived += FrameReader_MultiSourceFrameArrived;

            // Start the timer for the name animation
            Timer timer = new Timer(nameTimerInterval);
            timer.Elapsed += HandleTimer;
            timer.Start();
            HandleTimer(null, null);
        }

        /// <summary>
        /// Update the camera feed from the sensor
        /// </summary>
        /// <param name="sender">Object sending the event</param>
        /// <param name="e">Arguments for the event object</param>
        void FrameReader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e) {
            var reference = e.FrameReference.AcquireFrame();
            using (var frame = reference.ColorFrameReference.AcquireFrame()) {
                if (frame != null) {
                    sensorFeed.Source = ToBitmap(frame);
                }
            }
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

                    updateName(topName, names[i]);
                    updateName(centerName, names[(i + 1) % names.Count()]);
                    updateName(bottomName, names[(i + 2) % names.Count()]);
                });
            } catch (OperationCanceledException) { }
        }

        /// <summary>
        /// Update one of the names to perform the animated fading
        /// </summary>
        /// <param name="block">The block containing the name</param>
        /// <param name="newName">Replacement text, if applicable</param>
        private void updateName(TextBlock block, String newName) {
            if ((double)block.GetValue(OpactityDeltaProperty) == 0) {
                block.Text = newName;
                block.SetValue(OpactityDeltaProperty, -nameOpacityDelta);
            } else if ((double)block.GetValue(OpactityDeltaProperty) < 0) {
                if (block.Opacity <= 0) {
                    block.SetValue(OpactityDeltaProperty, nameOpacityDelta);
                    block.Text = newName;
                }

                block.Opacity += (double)block.GetValue(OpactityDeltaProperty);
            } else if ((double)block.GetValue(OpactityDeltaProperty) > 0) {
                if (block.Opacity >= 1) {
                    block.SetValue(OpactityDeltaProperty, -nameOpacityDelta);
                }

                block.Opacity += (double)block.GetValue(OpactityDeltaProperty);
            }
        }

        /// <summary>
        /// Convert a frame of kinect color video to bitmap for display
        /// Conversion code from http://pterneas.com/2014/02/20/kinect-for-windows-version-2-color-depth-and-infrared-streams/
        /// Under Apache License 2.0
        /// </summary>
        /// <param name="frame">The frame received from the kinect</param>
        /// <returns>A bitmap format source for a WPF image control</returns>
        private static ImageSource ToBitmap(ColorFrame frame) {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;

            byte[] pixels = new byte[width * height * ((PixelFormats.Bgr32.BitsPerPixel + 7) / 8)];

            if (frame.RawColorImageFormat == ColorImageFormat.Bgra) {
                frame.CopyRawFrameDataToArray(pixels);
            } else {
                frame.CopyConvertedFrameDataToArray(pixels, ColorImageFormat.Bgra);
            }

            int stride = width * PixelFormats.Bgr32.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
        }

        // Property used to store the change in opacity for the name
        public static readonly DependencyProperty OpactityDeltaProperty = DependencyProperty.RegisterAttached(
          "OpactityDelta",
          typeof(Double),
          typeof(TextBlock),
          new PropertyMetadata(null)
        );
           
        // Temporary source of names to display.
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
