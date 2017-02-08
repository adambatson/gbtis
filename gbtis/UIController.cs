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
using System.Collections;

namespace gbtis { 
    class UIController {
        private Boolean demoMode;
        private AdminWindow admin;
        private StandbyWindow standby;
        private CanvasWindow canvas;
        private Kinect kinect;

        //TESTING PURPOSES
        private List<String> names;

        public UIController(Boolean demoMode) {
            //How GBTIS operates
            this.demoMode = demoMode;

            //Making the kinect Controller
            kinect = Kinect.getInstance();
            kinectHandler();

            //Starting the standby window
            standby = new StandbyWindow();
            standbyHandler();

            //Starting and showing the admin window only if its normal mode
            if (!demoMode) {
                admin = new AdminWindow();
                adminHandler();
                admin.Show();
            }
            else {
                standby.Show();
            }

            //Starting the canvas screen
            canvas = null;

<<<<<<< HEAD
            //TESTING PURPOSES
            names = new List<String>();
=======
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
>>>>>>> 4bb5d981358823412f6c3018618542b3be84f4b7
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
            if ( (canvas == null) && (standby.IsVisible) ) {
                standby.Hide();
                canvas = new CanvasWindow();
                subscribeToCanvasHandler();
                canvas.Show();
<<<<<<< HEAD
                //only sets new user if the canvas is starting with a new user
=======
                alignScreens();
>>>>>>> 4bb5d981358823412f6c3018618542b3be84f4b7
                if (kinect.getActiveBodyId() != bodyId) {
                    kinect.SetActiveBody(bodyId);
                }
            }
            //happens when changing hands with either screen
            if (kinect.getActiveBodyId() == bodyId) {
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
                if (demoMode) {
                    exitAll();
                } else {
                    if (canvas != null) {
                        canvas.Hide();
                        unsubscribeToCanvasHandler();
                        canvas.Close();
                        canvas = null;
                    }
                    standby.Show();
                }
<<<<<<< HEAD
=======
                standby.Show();
                alignScreens();
>>>>>>> 4bb5d981358823412f6c3018618542b3be84f4b7
            }));
        }

        /// <summary>
        /// When the cancel button is pressed in the canvas Screen
        /// Switch back to standby from canvas 
        /// As well as save the name from what was inputted
        /// </summary>
        private void saveName(String s) {
            names.Add(s);
            String[] party = names.ToArray();
            standby.setNames(party);
            goToStandby();
        }

        /// <summary>
        /// Handles all the UI related Admin events
        /// </summary>
        private void adminHandler() {
            admin.Exit += (s, e) => {  exitAll(); };
            admin.Standby += (s, e) => { goToStandby(); };
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
