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
        Boolean canvasOpen;

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
            canvas = new CanvasWindow();
            canvasOpen = false;
            canvasHandler();

            startTimer();
        }

        public static void startTimer() {
            //Sarting the timer to shoot out event "TOCK" every 50 ms
            Timer timer = new Timer(TICK);
            timer.Elapsed += (s, e) => Tock?.Invoke(timer, new EventArgs()); ;
            timer.Start();
        }

        private void kinectHandler() {
            kinect.WaveGestureOccured += () => { Application.Current.Dispatcher.Invoke(new Action(() => waveOccured() )); };
            //kinect.EasterEggGestureOccured += () => { Application.Current.Dispatcher.Invoke(new Action(() => handleEasterEggOccured() )); };
        }

        //Application.Current.Dispatcher.Invoke(new Action(() =>));
        private void waveOccured() {
            if (!canvasOpen) {
                standby.Hide();
                canvas.clearScreen();
                canvasOpen = true;
                canvas.Show();
            }
        }

        private void handleEasterEggOccured() {
            standby.EasterEggArrived();
            standby.Hide();
            standby.changeText(gbtis.Properties.Resources.msgStart);
            canvas.clearScreen();
            canvasOpen = true;
            canvas.Show();
        }

        private void canvasHandler() {
            canvas.Cancel += () => { Application.Current.Dispatcher.Invoke(new Action(() => cancelOccured() )); };
            canvas.Continue += (s) => { Application.Current.Dispatcher.Invoke(new Action(() => continueOccured() )); };
        }

        private void cancelOccured() {
            canvas.Hide();
            canvasOpen = false;
            standby.Show();
        }

        private void continueOccured() {
            canvas.Hide();
            canvasOpen = false;
            standby.Show();
        }

        private void adminHandler() {
            admin.Exit += (s, e) => { Application.Current.Dispatcher.Invoke(new Action(() => exitAll() )); };
            admin.Standby += (s, e) => { continueOccured(); };
            admin.Input += (s, e) => { waveOccured(); };
        }

        private void standbyHandler() {
            standby.Exit += (s, e) => { exitAll(); };
        }

        private void exitAll() {
            try {
                standby.Close();
                admin.Close();
                canvas.Close();
                Environment.Exit(0);
            } catch (InvalidOperationException e) { Environment.Exit(0); }  
        }
    }
}
