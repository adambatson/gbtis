using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;

namespace gbtis {
    //Deleagtes for custom event handlers
    public delegate void BitMapReadyHandler(ImageSource img);
    public delegate void WaveGestureHandler(ulong bodyID, bool rightHand);
    public delegate void EasterEggHandler();
    public delegate void SensorStatusHandler(Boolean isAvailable);
    public delegate void ModeChangedHandler(CursorModes mode);
    public delegate void FingerPositionHandler(Point point);

    /// <summary>
    /// Kinect Wrapper Class
    /// </summary>
    public class Kinect {

        //Singleton Instance
        private static readonly Kinect instance = new Kinect();

        //Constants
        private const double WAVE_CONFIDENCE = 0.7;
        private const double EASTER_EGG_CONFIDENCE = 0.5;
        private const float SMOOTHING_FACTOR = 0.35f;
        private const int FRAME_SKIP_HAND_STATUS = 5;

        //TODO Should these be private?
        public Point FingerPosition { get; set; }
        public CursorModes CursorMode { get; set; }

        //Mode switch frame skip
        private CursorModes NextMode;
        private int ModeFrameSkip;

        //Events
        public event BitMapReadyHandler BitMapReady;
        public event WaveGestureHandler WaveGestureOccured;
        public event EasterEggHandler EasterEggGestureOccured;
        public event SensorStatusHandler SensorStatusChanged;
        public event ModeChangedHandler ModeStart;
        public event ModeChangedHandler ModeEnd;
        public event FingerPositionHandler FingerPositionChanged;

        private KinectSensor sensor;
        private MultiSourceFrameReader frameReader;

        //Gestures
        private Gesture waveGesture, easterEgg;
        private VisualGestureBuilderDatabase db;
        private Body[] bodies;
        private Body activeBody;
        private BodyFrameReader bodyReader;
        private VisualGestureBuilderFrameSource[] gestureSources;
        private VisualGestureBuilderFrameReader[] gestureReaders;
        private int numBodies;

        //Just the tip
        private Point? prevPoint;
        private CoordinateMapper coordinateMapper;

        //Rolling average finger positions
        private float xAvg, yAvg;

        //Handedness
        private JointType hand, handTip;
        private bool rightHand;

        private Kinect() {
            sensor = KinectSensor.GetDefault();
            sensor.Open();

            // Prepare sensor feed
            frameReader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth);
            frameReader.MultiSourceFrameArrived += frameReader_frameArrived;

            numBodies = this.sensor.BodyFrameSource.BodyCount;
            gestureSources = new VisualGestureBuilderFrameSource[numBodies];
            gestureReaders = new VisualGestureBuilderFrameReader[numBodies];
            OpenBodyReader();
            OpenGestureReader();
            bodies = new Body[numBodies];

            //Coordinate Mapping
            coordinateMapper = sensor.CoordinateMapper;

            sensor.IsAvailableChanged += OnIsAvailableChanged;
            xAvg = 0; yAvg = 0;

            setHand(true);
            NextMode = CursorModes.Idle;
            ModeFrameSkip = 0;
        }

        /// <summary>
        /// Returns the singleton kinect instance
        /// </summary>
        /// <returns></returns>
        public static Kinect getInstance() {
            return instance;
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
                    Application.Current.Dispatcher.Invoke(new Action(() => handler?.Invoke(img)));
                }
            }

            if (activeBody != null && activeBody.IsTracked) {
                HandState handState = (rightHand) ? activeBody.HandRightState :
                    activeBody.HandLeftState;
                
                var colorPoint = coordinateMapper.MapCameraPointToColorSpace(
                    activeBody.Joints[handTip].Position);

                Point point = getAverageFingerTipPosition(colorPoint.X, colorPoint.Y);
                if (!point.Equals(prevPoint)) {
                    FingerPosition = point;
                    prevPoint = point;
                    Application.Current.Dispatcher.Invoke(new Action(() => FingerPositionChanged?.Invoke(point)));
                }

                CursorModes mode;
                switch (handState) {
                    case HandState.Lasso:
                        mode = CursorModes.Draw;
                        break;
                    case HandState.Open:
                        mode = CursorModes.Erase;
                        break;
                    case HandState.Closed:
                    case HandState.NotTracked:
                        mode = CursorModes.Idle;
                        break;
                    default:
                        return;
                }

                if (mode != CursorMode) {

                    if (mode == CursorModes.Idle) {
                        //Compare the tip of the hand to the hand blob
                        //Attempt to see if a finger is extended
                        if (Math.Abs(activeBody.Joints[hand].Position.Z - activeBody.Joints[handTip].Position.Z) > 0.05) {
                            mode = CursorModes.Draw;
                            return;
                        }
                    }
                    
                    if (mode == NextMode) {
                        if (++ModeFrameSkip == FRAME_SKIP_HAND_STATUS) {
                            Application.Current.Dispatcher.Invoke(new Action(() => ModeEnd?.Invoke(CursorMode)));
                            Application.Current.Dispatcher.Invoke(new Action(() => ModeStart?.Invoke(mode)));
                            CursorMode = mode;
                            ModeFrameSkip = 0;
                        }
                    } else NextMode = mode;
                }
            }
        }

        /// <summary>
        /// Convert color camera coordinates to arbitrary coordinates
        /// </summary>
        /// <param name="p">Point to convert</param>
        /// <param name="size">Size of the new region</param>
        /// <returns></returns>
        public Point ColorToInterface(Point p, Size size) {
            ColorFrameSource c = sensor.ColorFrameSource;
            return new Point(
                p.X * size.Width / c.FrameDescription.Width,
                p.Y * size.Height / c.FrameDescription.Height
            );
        }

        /// <summary>
        /// Exponential moving average of fingertip position
        /// </summary>
        /// <param name="x">The latest x coordinate</param>
        /// <param name="y">The latest y coordinate</param>
        /// <returns>The average position</returns>
        private Point getAverageFingerTipPosition(float x, float y) {
            xAvg = SMOOTHING_FACTOR * x + (1 - SMOOTHING_FACTOR) * xAvg;
            yAvg = SMOOTHING_FACTOR * y + (1 - SMOOTHING_FACTOR) * yAvg;

            return new Point(xAvg, yAvg);
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

        /// <summary>
        /// Opens the BodyReader
        /// </summary>
        private void OpenBodyReader() {
            bodyReader = this.sensor.BodyFrameSource.OpenReader();
            bodyReader.FrameArrived += OnBodyFrameArrived;
        }

        /// <summary>
        /// Opens the GestureReader and loads the Gestures
        /// </summary>
        private void OpenGestureReader() {
            try {
                db = new VisualGestureBuilderDatabase(
                  @"..\..\Resources\gbtisg.gbd");
            } catch (Exception) {
                return;
            }

            // we assume that this gesture is in that database (it should be, it's the only
            // gesture in there).
            waveGesture = db.AvailableGestures.Where(g => g.Name == "wave").Single();

            easterEgg = db.AvailableGestures.Where(g => g.Name == "dab").Single();
            for (int i = 0; i < numBodies; i++) {
                gestureSources[i] = new VisualGestureBuilderFrameSource(sensor, 0);
                gestureReaders[i] = gestureSources[i].OpenReader();

                gestureSources[i].AddGestures(db.AvailableGestures);

                gestureReaders[i].IsPaused = true;
                gestureReaders[i].FrameArrived += OnGestureFrameArrived;
            }
        }

        /// <summary>
        /// Registers that a new body has entered the frame
        /// TODO: This will need to be modified to support the body tracking
        ///     that we want to do in the Admin Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBodyFrameArrived(object sender, BodyFrameArrivedEventArgs e) {
            using (var frame = e.FrameReference.AcquireFrame()) {
                if (frame != null) {
                    frame.GetAndRefreshBodyData(bodies);

                    for(int i = 0; i < numBodies; i++) {
                        gestureSources[i].TrackingId = bodies[i].TrackingId;
                        gestureReaders[i].IsPaused = !bodies[i].IsTracked;
                    }

                    var trackedBodies = bodies.Where(b => b.IsTracked);

                    if (!trackedBodies.Contains(activeBody))
                        activeBody = null;
                }
            }
        }

        /// <summary>
        /// Parse the Gesture that arrived and trigger the appropriate event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGestureFrameArrived(object sender,
            VisualGestureBuilderFrameArrivedEventArgs e) {
            using (var frame = e.FrameReference.AcquireFrame()) {
                if (frame != null) {
                    var result = frame.DiscreteGestureResults;
                    if (result != null) {
                        if (result.ContainsKey(waveGesture)) {
                            var gesture = result[waveGesture];
                            if (gesture.Confidence > WAVE_CONFIDENCE) {
                                Body b = getBodyByTrackingID(frame.TrackingId);
                                Application.Current.Dispatcher.Invoke(new Action(() => {
                                    WaveGestureHandler handler = WaveGestureOccured;
                                    handler?.Invoke(frame.TrackingId, 
                                        (b.Joints[JointType.HandRight].Position.Y >=
                                        b.Joints[JointType.HandLeft].Position.Y));
                                }));
                            }
                        }
                    }
                }
            }
        }

        private Body getBodyByTrackingID(ulong id) {
            foreach(Body b in bodies){
                if (b.TrackingId == id)
                    return b;
            }
            return bodies[0];
        }

        public void SetActiveBody(ulong trackingId) {
            foreach(Body b in bodies) {
                if (b.TrackingId == trackingId) {
                    activeBody = b;
                    return;
                }
            }
        }

        public ulong? getActiveBodyId() {
            if (activeBody != null)
                return activeBody.TrackingId;
            return null;
        }

        /// <summary>
        /// Triggered when the KinectSensors availability changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIsAvailableChanged(Object sender, EventArgs e) {
            SensorStatusHandler handler = SensorStatusChanged;
            Application.Current.Dispatcher.Invoke(new Action(() => handler?.Invoke(isAvailable())));
        }

        public void setHand(bool right) {
            rightHand = right;
            hand = (rightHand) ? JointType.HandRight : JointType.HandLeft;
            handTip = (rightHand) ? JointType.HandTipRight : JointType.HandTipLeft;            
        }
    }
}
