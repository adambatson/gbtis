﻿using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Windows;

namespace gbtis {
    //Deleagtes for custom event handlers
    public delegate void BitMapReadyHandler(ImageSource img);
    public delegate void WaveGestureHandler();
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
        private const double WAVE_CONFIDENCE = 0.9;
        private const double EASTER_EGG_CONFIDENCE = 0.5;
        private const float SMOOTHING_FACTOR = 0.35f;
        private const float BORDER_SIZE = 0.1f;

        public Point FingerPosition { get; set; }
        public CursorModes CursorMode { get; set; }

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
        private VisualGestureBuilderFrameSource gestureSource;
        private VisualGestureBuilderFrameReader gestureReader;

        //Just the tip
        private Point? prevPoint;
        private CoordinateMapper coordinateMapper;

        //Rolling average finger positionsz
        private float xAvg, yAvg;

        //Handedness
        private JointType hand, handTip;

        private Kinect() {
            sensor = KinectSensor.GetDefault();
            sensor.Open();

            // Prepare sensor feed
            frameReader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth);
            frameReader.MultiSourceFrameArrived += frameReader_frameArrived;

            OpenBodyReader();
            OpenGestureReader();
            bodies = new Body[this.sensor.BodyFrameSource.BodyCount];

            //Coordinate Mapping
            coordinateMapper = sensor.CoordinateMapper;

            sensor.IsAvailableChanged += OnIsAvailableChanged;
            xAvg = 0; yAvg = 0;

            setHand(true);
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
                HandState handState = (hand == JointType.HandRight) ?
                    activeBody.HandRightState :
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
                        if (Math.Abs(activeBody.Joints[hand].Position.Z - activeBody.Joints[JointType.HandRight].Position.Z) > 0.05) {
                            mode = CursorModes.Draw;
                            return;
                        }
                    }

                    Application.Current.Dispatcher.Invoke(new Action(() => ModeEnd?.Invoke(CursorMode)));
                    Application.Current.Dispatcher.Invoke(new Action(() => ModeStart?.Invoke(mode)));
                    CursorMode = mode;
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
            gestureSource = new VisualGestureBuilderFrameSource(sensor, 0);
            gestureReader = gestureSource.OpenReader();

            gestureSource.AddGestures(db.AvailableGestures);
            gestureSource.TrackingIdLost += OnTrackingIdLost;

            gestureReader.IsPaused = true;
            gestureReader.FrameArrived += OnGestureFrameArrived;
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

                    var trackedBodies = bodies.Where(b => b.IsTracked);
                    if (trackedBodies.Count() == 0) {
                        OnTrackingIdLost(null, null);
                        return;
                    }

                    if (trackedBodies.Where(b => b.Equals(activeBody)).Count() == 0) {
                        activeBody = trackedBodies.FirstOrDefault();
                    }

                    if (gestureReader.IsPaused) {
                        gestureSource.TrackingId = activeBody.TrackingId;
                        gestureReader.IsPaused = false;
                    }
                }
            }
        }

        /// <summary>
        /// All bodies have left the frame, therefore stop reading gestures for now
        /// TODO: Again this will need to be modified to support the body tracking
        ///     in Admin Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTrackingIdLost(object sender, TrackingIdLostEventArgs e) {
            gestureReader.IsPaused = true;
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
                                Application.Current.Dispatcher.Invoke(new Action(() => {
                                    setHand(
                                    activeBody.Joints[JointType.HandRight].Position.Y >=
                                    activeBody.Joints[JointType.HandLeft].Position.Y
                                    );
                                    WaveGestureHandler handler = WaveGestureOccured;
                                    handler?.Invoke();
                                }));
                            }
                        }
                    }
                }
            }
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

        private void setHand(bool right) {
            hand = (right) ? JointType.HandRight : JointType.HandLeft;
            handTip = (right) ? JointType.HandTipRight : JointType.HandTipLeft;            
        }
    }
}
