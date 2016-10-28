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
            kinect.WaveGestureOccured += () => { standby.Hide(); canvas.Show(); };
            kinect.EasterEggGestureOccured += () => { 
                standby.changeText("Bruh!");
                SoundPlayer player = new SoundPlayer(
                    @"..\..\Resources\excellent.wav");
                player.Play();
                standby.Hide();
                canvas.Show(); 
                standby.changeText(""); //change to whatever it was before
            };
        }

        private void canvasHandler() {
            canvas.Cancel += (s, e) => { canvas.Hide(); standby.Show(); };
            canvas.Continue += (s, e) => { canvas.Hide(); standby.Show(); };
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
    }
}
