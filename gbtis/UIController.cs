﻿using Microsoft.Kinect;
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
            canvas = new CanvasWindow();
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
            kinect.EasterEggGestureOccured += () => { Application.Current.Dispatcher.Invoke(new Action(() => handleEasterEggOccured() )); };
        }

        //Application.Current.Dispatcher.Invoke(new Action(() =>));
        private void waveOccured() {
            standby.Hide();
            canvas.clearScreen();
            canvas.Show();
        }

        private void handleEasterEggOccured() {
            standby.EasterEggArrived();
            standby.Hide();
            standby.changeText(gbtis.Properties.Resources.msgStart);
            canvas.clearScreen();
            canvas.Show();
        }

        private void canvasHandler() {
            canvas.Cancel += () => { Application.Current.Dispatcher.Invoke(new Action(() => cancelOccured() )); };
            canvas.Continue += (s) => { Application.Current.Dispatcher.Invoke(new Action(() => continueOccured() )); };
        }

        private void cancelOccured() {
            canvas.Hide();
            standby.Show();
        }

        private void continueOccured() {
            canvas.Hide();
            standby.Show();
        }

        private void adminHandler() {
            admin.Exit += (s, e) => { Application.Current.Dispatcher.Invoke(new Action(() => exitAll() )); };
            admin.Standby += (s, e) => { standby.Show(); };
            admin.Input += (s, e) => { standby.Show(); };
        }

        private void standbyHandler() {
            standby.Exit += (s, e) => { exitAll(); };
        }

        private void exitAll() {
            try {
                standby.Close();
                admin.Close();
                canvas.Close();
            } catch (InvalidOperationException e) { }  
        }
    }
}
