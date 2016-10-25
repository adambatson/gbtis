using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

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
            kinect = new Kinect();
            kinectHandler();

            /*Starting the standby window
            standby = new StandbyWindow(kinect);
            standbyHandler();
            */

            //Starting and showing the admin window
            admin = new AdminWindow();
            adminHandler();
            admin.Show();

            /*Starting the canvas screen
            canvas = new CanvasWindow(kinect);
            canvasHandler();
            */

            startTimer();
        }

        public static void startTimer() {
            //Sarting the timer to shoot out event "TOCK" every 50 ms
            Timer timer = new Timer(TICK);
            timer.Elapsed += (s, e) => Tock?.Invoke(timer, new EventArgs()); ;
            timer.Start();
        }

        private void kinectHandler() {
            kinect.WaveGestureOccured += () => { startNewCanvas(); closeStandby(); };
            kinect.EasterEggGestureOccured += () => {
                standby.EasterEggArrived();
                standby.changeText(""); //change to whatever it was before
                closeStandby();
                startNewCanvas();
            };
        }

        private void unSubscribekinectHandler() {
            kinect.WaveGestureOccured -= () => { };
            //kinect.EasterEggGestureOccured -= () => { };
        }

        private void startNewCanvas() {
            canvas = new CanvasWindow(kinect);
            canvas.Show();
            canvas.Cancel += (s, e) => { startNewStandby(); closeCanvas(); };
            canvas.Continue += (s, e) => { startNewStandby(); closeCanvas(); };
        }

        private void closeCanvas() {
            if (canvas != null) {
                canvas.Close();
                canvas.Cancel -= (s, e) => { };
                canvas.Continue -= (s, e) => { };
            }
        }

        private void adminHandler() {
            admin.Exit += (s, e) => { exitAll(); };
            admin.Standby += (s, e) => { startNewStandby(); };
            admin.Input += (s, e) => { startNewStandby(); };
        }

        private void startNewStandby() {
            standby = new StandbyWindow(kinect);
            standby.Show();
            standby.Exit += (s, e) => { exitAll(); };
            kinectHandler();
        }

        private void closeStandby() {
            if (standby != null) {
                standby.Close();
                standby.Exit -= (s, e) => { exitAll(); };
                unSubscribekinectHandler();
            }
        }

        private void exitAll() {
            try {
                closeStandby();
                admin.Close();
                closeCanvas();
            }
            catch (InvalidOperationException e){}  
        }
    }
}
