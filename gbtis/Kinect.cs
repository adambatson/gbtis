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

        //Constants
        private const double WAVE_CONFIDENCE = 0.5;
        private const double EASTER_EGG_CONFIDENCE = 0.5;

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
        private HandState lastRightHandState;

        public Kinect() {
            sensor = KinectSensor.GetDefault();
            sensor.Open();

            // Prepare sensor feed
            frameReader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color);
            frameReader.MultiSourceFrameArrived += frameReader_frameArrived;

            OpenBodyReader();
            OpenGestureReader();
            bodies = new Body[this.sensor.BodyFrameSource.BodyCount];

            //Coordinate Mapping
            coordinateMapper = sensor.CoordinateMapper;

            //sensor.IsAvailableChanged += OnIsAvailableChanged;

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
            if(activeBody != null && activeBody.IsTracked) {
                var rightHand = activeBody.Joints[JointType.HandTipRight];
                var colorPoint = coordinateMapper.MapCameraPointToColorSpace(
                    rightHand.Position);
                Point point = new Point(
                    colorPoint.X,
                    colorPoint.Y
                );
                if (!point.Equals(prevPoint)) {
                    FingerPositionChanged?.Invoke(point);
                    FingerPosition = point;
                    prevPoint = point;
                }
                if(activeBody.HandRightState != lastRightHandState) {
                    ModeEnd?.Invoke(CursorMode);

                    switch (activeBody.HandRightState) {
                        case HandState.NotTracked:
                        case HandState.Closed:
                            CursorMode = CursorModes.Idle;
                            break;
                        case HandState.Open:
                            CursorMode = CursorModes.Erase;
                            break;
                        default:
                            CursorMode = CursorModes.Draw;
                            break;
                    }

                    ModeStart?.Invoke(CursorMode);
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

            // we assume that this file exists and will load
            db = new VisualGestureBuilderDatabase(
              @"..\..\Resources\gbtisg.gbd");

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
                                WaveGestureHandler handler = WaveGestureOccured;
                                handler?.Invoke();
                            }
                        }
                        if (result.ContainsKey(easterEgg)) {
                            var gesture = result[easterEgg];
                            if (gesture.Confidence > EASTER_EGG_CONFIDENCE) {
                                EasterEggHandler handler = EasterEggGestureOccured;
                                handler?.Invoke();
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
            handler?.Invoke(isAvailable());
        }
    }
}
