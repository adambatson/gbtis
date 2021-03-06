﻿using gbtis.Controls;
using System;
using System.Linq;
using System.Media;
using System.Timers;
using System.Windows;
using System.Windows.Media;

namespace gbtis.Windows {
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class StandbyWindow : Window {
        // Settings for the text fade effect
        public static int nameFadeInterval = 5000;

        // Events
        public event EventHandler Exit;

        //names displayed
        private String[] names;

        /// <summary>
        /// Constructor for the standby window
        /// </summary>
        /// <param name="_sensor">An open kinect sensor, or null</param>
        public StandbyWindow(String[] names) {
            InitializeComponent();
            Kinect kinect = Kinect.getInstance();

            // Initialize load text
            standbyMsg.Text = (kinect.isAvailable()) ?
                gbtis.Properties.Resources.msgStart :
                gbtis.Properties.Resources.msgNoSensor;

            kinect.BitMapReady += BitMapArrived;
            kinect.SensorStatusChanged += OnSensorStatusChanged;

            // Initialize the names
            topName.Text = "";
            centerName.Text = "";
            bottomName.Text = "";
            if (names.Length == 0) {
                this.names = DEFAULT_NAMES;
            } else {
                this.names = names;
            }

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

        public void changeText(String newText) {
            this.Dispatcher.Invoke(new Action(() => {
                standbyMsg.Text = newText;
            }));
        }

        /// <summary>
        /// Hanldes a change in the  availability status of the kinect
        /// </summary>
        /// <param name="IsAvailable"></param>
        private void OnSensorStatusChanged(Boolean IsAvailable) {
            this.Dispatcher.Invoke(() => {
                if (IsAvailable) {
                    standbyMsg.Text = gbtis.Properties.Resources.msgStart;
                } else {
                    standbyMsg.Text = gbtis.Properties.Resources.msgNoSensor;
                    sensorFeed.Source = null;
                }
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
        /// update the list of names with newly updated names
        /// </summary>
        /// <param name="names">new list of names to be displayed</param>
        public void setNames(String[] names) {
            this.names = names;
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

        // Default names for testing purposes
        private static String[] DEFAULT_NAMES = {
                "Adam Batson",
                "Max DeMelo",
                "Richard Carson",
                "Eranga Ukwatta",
                "Sreeraman Rajan"
            };
    }
}
