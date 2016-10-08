using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace gbtis {
    //Deleagtes for custom event handlers
    public delegate void BitMapReadyHandler(ImageSource img);
    public delegate void WaveGestureHandler();
    public delegate void EasterEggHandler();

    /// <summary>
    /// Kinect Wrapper Class
    /// </summary>
    public class Kinect {

        //Events
        public event BitMapReadyHandler BitMapReady;
        public event WaveGestureHandler WaveGestureOccured;
        public event EasterEggHandler EasterEggGestureOccured;

        private KinectSensor sensor;
        private MultiSourceFrameReader frameReader;

        //Gestures
        private Gesture waveGesture, easterEgg;
        private VisualGestureBuilderDatabase db;
        private Body[] bodies;
        private BodyFrameReader bodyReader;
        private VisualGestureBuilderFrameSource gestureSource;
        private VisualGestureBuilderFrameReader gestureReader;

        public Kinect() {
            sensor = KinectSensor.GetDefault();
            sensor.Open();
            //There is a slight delay between when the sensor is opened
            //and when IsAvailable is set to true.  We need to ensure
            //that IsAvailable becomes true before anyone calls the
            //isAvailable() method.
            Thread.Sleep(500);

            // Prepare sensor feed
            frameReader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color);
            frameReader.MultiSourceFrameArrived += frameReader_frameArrived;

            OpenBodyReader();
            OpenGestureReader();

        }

        /// <summary>
        /// Determines if the Kinect Sensor if available for use
        /// </summary>
        /// <returns>true if the sensor is available else false</returns>
        public Boolean isAvailable() {
            return sensor != null && sensor.IsAvailable;
        }

        /// <summary>
        /// Generates an ImageSource based on the latest MultiSourceFrame
        /// and triggers a BitMapReady event
        /// </summary>
        /// <param name="sender">The Sender of the frame (Kinect.sensor)</param>
        /// <param name="e">The MultiSourceFrameEventArgs</param>
        private void frameReader_frameArrived(Object sender, MultiSourceFrameArrivedEventArgs e) {
            var reference = e.FrameReference.AcquireFrame();
            using (var frame = reference.ColorFrameReference.AcquireFrame()) {
                if (frame != null) {
                    BitMapReadyHandler handler = BitMapReady;
                    ImageSource img = ToBitmap(frame);
                    //Allow the image to be accessible outside this thread
                    img.Freeze();
                    handler?.Invoke(img);
                }
            }
        }

        /// <summary>
        /// Convert a frame of kinect color video to bitmap for display
        /// Conversion code from http://pterneas.com/2014/02/20/kinect-for-windows-version-2-color-depth-and-infrared-streams/
        /// Under Apache License 2.0
        /// </summary>
        /// <param name="frame">The frame received from the kinect</param>
        /// <returns>A bitmap format source for a WPF image control</returns>
        public static ImageSource ToBitmap(ColorFrame frame) {
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

        private void OpenBodyReader() {
            if (bodies == null) {
                bodies = new Body[this.sensor.BodyFrameSource.BodyCount];
            }
            bodyReader = this.sensor.BodyFrameSource.OpenReader();
            bodyReader.FrameArrived += OnBodyFrameArrived;
        }

        private void OpenGestureReader() {

            // we assume that this file exists and will load
            db = new VisualGestureBuilderDatabase(
              @"C:\Users\adambatson\Documents\Visual Studio 2015\Projects\gbtis\gbtis\Resources\gbtisg.gbd");

            // we assume that this gesture is in that database (it should be, it's the only
            // gesture in there).
            waveGesture = db.AvailableGestures.Where(g => g.Name == "wave").Single();

            easterEgg = db.AvailableGestures.Where(g => g.Name == "dab").Single();
            gestureSource = new VisualGestureBuilderFrameSource(sensor, 0);
            gestureReader = this.gestureSource.OpenReader();

            gestureSource.AddGestures(db.AvailableGestures);
            gestureSource.TrackingIdLost += OnTrackingIdLost;

            gestureReader.IsPaused = true;
            gestureReader.FrameArrived += OnGestureFrameArrived;
        }

        private void OnBodyFrameArrived(object sender, BodyFrameArrivedEventArgs e) {
            using (var frame = e.FrameReference.AcquireFrame()) {
                if (frame != null) {
                    frame.GetAndRefreshBodyData(bodies);

                    var trackedBody = bodies.Where(b => b.IsTracked).FirstOrDefault();

                    if (trackedBody != null) {
                        if (gestureReader.IsPaused) {
                            gestureSource.TrackingId = trackedBody.TrackingId;
                            gestureReader.IsPaused = false;
                        }
                    } else {
                        OnTrackingIdLost(null, null);
                    }
                }
            }
        }

        private void OnTrackingIdLost(object sender, TrackingIdLostEventArgs e) {
            gestureReader.IsPaused = true;
        }

        private void OnGestureFrameArrived(object sender,
            VisualGestureBuilderFrameArrivedEventArgs e) {
            using (var frame = e.FrameReference.AcquireFrame()) {
                if (frame != null) {
                    var result = frame.DiscreteGestureResults;

                    if (result != null) {
                        if (result.ContainsKey(waveGesture)) {
                            var gesture = result[waveGesture];
                            if (gesture.Confidence > 0.5) {
                                WaveGestureHandler handler = WaveGestureOccured;
                                handler?.Invoke();
                            }
                        }
                        if (result.ContainsKey(easterEgg)) {
                            var gesture = result[easterEgg];
                            if (gesture.Confidence > 0.5) {
                                EasterEggHandler handler = EasterEggGestureOccured;
                                handler?.Invoke();
                            }
                        }
                    }
                }
            }
        }
    }
}
