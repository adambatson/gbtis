using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Data;
using gbtis.Windows;
//using System.Data.SQLite;

namespace gbtis { 
    class UIController {
        // Timer set to tick every 50 ms
        public static int TICK = 50;
        public static event EventHandler Tock;

        //private SQLiteConnection sqlite;

        AdminWindow admin;
        StandbyWindow standby;
        CanvasWindow canvas;
        Kinect kinect;

        public UIController() {
            //Starting SQlite Connection
            //sqlite = new SQLiteConnection("location of datebase");  //Not fully implemented

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

            //Starting the screens
            canvas = null;

            startTimer();
            admin.ScreenChanged += () => alignScreens();
        }

        private void alignScreens() {
            if (standby != null)
                admin.AlignWindow(standby);

            if (canvas != null)
                admin.AlignWindow(canvas);
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
            kinect.WaveGestureOccured += (id, rightHand) => { Application.Current.Dispatcher.Invoke(new Action(() => waveOccured(id, rightHand) )); };
        }

        /// <summary>
        /// When the wave gestured is recognized by the kinect
        /// Switch from Standby to Canvas
        /// </summary>
        private void waveOccured(ulong bodyId, bool rightHand) {
            if ((canvas == null)&&(standby.IsVisible)) {
                standby.Hide();
                canvas = new CanvasWindow();
                admin.AlignWindow(canvas);
                subscribeToCanvasHandler();
                canvas.Show();
                if (kinect.getActiveBodyId() != bodyId) {
                    kinect.SetActiveBody(bodyId);
                } else kinect.setHand(rightHand);
            }
            else {
                /*standby.Hide();
                canvas.clearScreen();
                canvas.Show();*/
                if (kinect.getActiveBodyId() == bodyId)
                    kinect.setHand(rightHand);
            }
        }

        /// <summary>
        /// Handles all the UI related Canvas events
        /// </summary>
        private void subscribeToCanvasHandler() {
            canvas.Cancel += goToStandby; 
            canvas.Continue += saveName;
        }

        /// <summary>
        /// Handles all the UI related Canvas events
        /// </summary>
        private void unsubscribeToCanvasHandler() {
            canvas.Cancel -= goToStandby;
            canvas.Continue -= saveName;
        }

        /// <summary>
        /// When the cancel button is pressed in the canvas Screen
        /// Switch back to standby from canvas
        /// </summary>
        private void goToStandby() {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                if (canvas != null) {
                    canvas.Hide();
                    unsubscribeToCanvasHandler();
                    canvas.Close();
                    canvas = null;
                }
                alignScreens();
                standby.Show();
            }));
        }

        /// <summary>
        /// When the cancel button is pressed in the canvas Screen
        /// Switch back to standby from canvas 
        /// As well as save the name from what was inputted
        /// </summary>
        private void saveName(String s) {
            goToStandby();
        }

        /// <summary>
        /// Handles all the UI related Admin events
        /// </summary>
        private void adminHandler() {
            admin.Exit += (s, e) => {  exitAll(); };
            admin.Standby += (s, e) => { goToStandby(); };
            //Broken with new body setting features
            //admin.Input += (s, e) => { waveOccured(); };
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
