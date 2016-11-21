using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace gbtis { 
    class UIController {
        // Timer set to tick every 50 ms
        public static int TICK = 50;
        public static event EventHandler Tock;

        AdminWindow admin;
        StandbyWindow standby;
        CanvasWindow canvas;
        Kinect kinect;

        public UIController() {
            //Making the kinect Controller
            kinect = Kinect.getInstance();
            kinectHandler();

            //Starting the standby window
            standby = new StandbyWindow();
            standbyHandler();

            //Starting and showing the admin window
            admin = new AdminWindow();
            adminHandler();
            admin.Show();

            //Starting the canvas screen
            canvas = null;
            canvasHandler();

            startTimer();
        }

        /// <summary>
        /// A timer that shoots out an event "TOCK" every 50 ms
        /// </summary>
        public static void startTimer() {
            Timer timer = new Timer(TICK);
            timer.Elapsed += (s, e) => Tock?.Invoke(timer, new EventArgs()); ;
            timer.Start();
        }

        /// <summary>
        /// Handles all the UI related Kinect events
        /// </summary>
        private void kinectHandler() {
            kinect.WaveGestureOccured += () => { Application.Current.Dispatcher.Invoke(new Action(() => waveOccured() )); };
        }

        /// <summary>
        /// When the wave gestured is recognized by the kinect
        /// Switch from Standby to Canvas
        /// </summary>
        private void waveOccured() {
            if (canvas == null) {
                standby.Hide();
                canvas = new CanvasWindow();
                canvas.Show();
            }
            else {
                Standby.Hide();
                canvas.clearScreen();
                canvas.Show();
            }
        }

        /// <summary>
        /// Handles all the UI related Canvas events
        /// </summary>
        private void canvasHandler() {
            canvas.Cancel += () => { Application.Current.Dispatcher.Invoke(new Action(() => cancelOccured() )); };
            canvas.Continue += (s) => { Application.Current.Dispatcher.Invoke(new Action(() => continueOccured() )); };
        }

        /// <summary>
        /// When the cancel button is pressed in the canvas Screen
        /// Switch back to standby from canvas
        /// </summary>
        private void cancelOccured() {
            canvas.Hide();
            standby.Show();
            canvas.Close();
            canvas = null;
        }

        /// <summary>
        /// When the cancel button is pressed in the canvas Screen
        /// Switch back to standby from canvas 
        /// As well as save the name from what was inputted
        /// </summary>
        private void continueOccured() {
            canvas.Hide();
            standby.Show();
            canvas.Close();
            canvas = null;
        }

        /// <summary>
        /// Handles all the UI related Admin events
        /// </summary>
        private void adminHandler() {
            admin.Exit += (s, e) => { Application.Current.Dispatcher.Invoke(new Action(() => exitAll() )); };
            admin.Standby += (s, e) => { continueOccured(); };
            admin.Input += (s, e) => { waveOccured(); };
        }

        /// <summary>
        /// Handles all the UI related Standby events
        /// </summary>
        private void standbyHandler() {
            standby.Exit += (s, e) => { exitAll(); };
        }

        /// <summary>
        /// When any of the screens are closed (except Canvas)
        /// Fully Exit program closing everything first
        /// Severing the connection between the program and the database
        /// </summary>
        private void exitAll() {
            Environment.Exit(0);
        }
    }
}
