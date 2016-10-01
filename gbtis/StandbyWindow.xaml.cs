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
using Microsoft.Kinect.VisualGestureBuilder;
using System.IO;
using System.Media;

namespace gbtis {
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class StandbyWindow : Window {
        // Settings for the text fade effect
        public static int nameFadeInterval = 5000;

        Gesture waveGesture, easterEgg;
        VisualGestureBuilderDatabase db;
        Body[] bodies;
        BodyFrameReader bodyReader;
        VisualGestureBuilderFrameSource gestureSource;
        VisualGestureBuilderFrameReader gestureReader;

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
            standbyMsg.Text = (_sensor.IsAvailable) ? 
                gbtis.Properties.Resources.msgStart :
                gbtis.Properties.Resources.msgNoSensor;

            // Prepare sensor feed
            this.sensor = _sensor;
            frameReader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color);
            frameReader.MultiSourceFrameArrived += FrameReader_MultiSourceFrameArrived;
            //OnLoadGestureFromDb();
            OnOpenReaders();

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

        void OnOpenReaders() {
            this.OpenBodyReader();
            this.OpenGestureReader();
        }
        void OpenBodyReader() {
            if (this.bodies == null) {
                this.bodies = new Body[this.sensor.BodyFrameSource.BodyCount];
            }
            this.bodyReader = this.sensor.BodyFrameSource.OpenReader();
            this.bodyReader.FrameArrived += OnBodyFrameArrived;
        }
        void OpenGestureReader() {

            // we assume that this file exists and will load
            db = new VisualGestureBuilderDatabase(
              @"C:\Users\adambatson\Documents\Visual Studio 2015\Projects\gbtis\gbtis\Resources\gbtisg.gbd");

            // we assume that this gesture is in that database (it should be, it's the only
            // gesture in there).
            this.waveGesture =
              db.AvailableGestures.Where(g => g.Name == "wave").Single();

            this.easterEgg =
              db.AvailableGestures.Where(g => g.Name == "dab").Single();
            this.gestureSource = new VisualGestureBuilderFrameSource(this.sensor, 0);
            this.gestureReader = this.gestureSource.OpenReader();

            this.gestureSource.AddGestures(this.db.AvailableGestures);
            this.gestureSource.TrackingIdLost += OnTrackingIdLost;

            this.gestureReader.IsPaused = true;
            this.gestureReader.FrameArrived += OnGestureFrameArrived;
        }
        void OnBodyFrameArrived(object sender, BodyFrameArrivedEventArgs e) {
            using (var frame = e.FrameReference.AcquireFrame()) {
                if (frame != null) {
                    frame.GetAndRefreshBodyData(this.bodies);

                    var trackedBody = this.bodies.Where(b => b.IsTracked).FirstOrDefault();

                    if (trackedBody != null) {
                        if (this.gestureReader.IsPaused) {
                            this.gestureSource.TrackingId = trackedBody.TrackingId;
                            this.gestureReader.IsPaused = false;
                        }
                    } else {
                        OnTrackingIdLost(null, null);
                    }
                }
            }
        }
        void OnTrackingIdLost(object sender, TrackingIdLostEventArgs e) {
            this.gestureReader.IsPaused = true;
        }
        void OnGestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e) {
            using (var frame = e.FrameReference.AcquireFrame()) {
                if (frame != null) {
                    var result = frame.DiscreteGestureResults;

                    if (result != null) {
                        if (result.ContainsKey(this.waveGesture)) {
                            var gesture = result[this.waveGesture];
                            if (gesture.Confidence > 0.5)
                                this.Dispatcher.Invoke(() => {
                                    standbyMsg.Text = "You just waved!";
                                });
                        }

                        if (result.ContainsKey(this.easterEgg)) {
                            var gesture = result[this.easterEgg];
                            if (gesture.Confidence > 0.8) {
                                this.Dispatcher.Invoke(() => {
                                    standbyMsg.Text = "Bruh.";
                                    SoundPlayer player = new SoundPlayer(
                                        @"C:\Users\adambatson\Documents\Visual Studio 2015\Projects\gbtis\gbtis\Resources\excellent.wav");
                                    player.Play();
                                });
                            }
                        }
                    }
                }
            }
        }

        void OnLoadGestureFromDb() {
            // we assume that this file exists and will load
            db = new VisualGestureBuilderDatabase(
              @"C:\Users\adambatson\Documents\Visual Studio 2015\Projects\gbtis\gbtis\Resources\gbtisg.gbd");

            // we assume that this gesture is in that database (it should be, it's the only
            // gesture in there).
            this.waveGesture =
              db.AvailableGestures.Where(g => g.Name == "wave").Single();
            
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
                    this.Dispatcher.Invoke(new Action(() =>
                    sensorFeed.Source = ToBitmap(frame)));
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

                    new TextBlockFadeAnimation(topName, names[i]);
                    new TextBlockFadeAnimation(centerName, names[(i + 1) % names.Count()]);
                    new TextBlockFadeAnimation(bottomName, names[(i + 2) % names.Count()]);
                });
            } catch (OperationCanceledException) { }
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
