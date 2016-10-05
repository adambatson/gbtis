using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public UIController(KinectSensor _sensor) {
            //Starting and showing the standby window
            standby = new StandbyWindow(_sensor);
            standbyHandler();

            //Starting and showing the admin window
            admin = new AdminWindow();
            adminHandler();
            admin.Show();

            startTimer();
        }

        public static void startTimer() {
            //Sarting the timer to shoot out event "TOCK" every 50 ms
            Timer timer = new Timer(TICK);
            timer.Elapsed += ticker;
            timer.Start();
            ticker(null, null);
        }

        private void adminHandler() {
            admin.Exit += (s, e) => { exitAll(); };
            admin.Standby += (s, e) => { standby.Show(); };
        }

        private void standbyHandler() {
            standby.Exit += (s, e) => { exitAll(); };
        }

        private void exitAll() {
            try {
                standby.Close();
                admin.Close();
            }
            catch (InvalidOperationException e){}  
        }

        private static void ticker(object sender, EventArgs args) {
            Tock?.Invoke(null, args);
        }

    }
}
