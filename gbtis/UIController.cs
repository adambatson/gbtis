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
using System.Net.Http;
using System.Configuration;
using System.Net.Http.Headers;

namespace gbtis { 
    class UIController {

        private String AUTHKEY;

        private Boolean demoMode;
        private AdminWindow admin;
        private StandbyWindow standby;
        private CanvasWindow canvas;
        private Kinect kinect;
        private bool canWave;
        private Timer timeoutTimer;

        private static int demoTimeout = 180000;
        static HttpClient client = new HttpClient();

        //TESTING PURPOSES
        private List<String> names;

        public UIController(Boolean demoMode, String GBTISAASADDRESS, String AUTHKEY) {
            this.AUTHKEY = AUTHKEY;
            client.BaseAddress = new Uri(GBTISAASADDRESS);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            names = new List<string>();
            namesInit();
            //How GBTIS operates
            this.demoMode = demoMode;
            canWave = false;

            //TESTING PURPOSES
            names = new List<String>();

            //Making the kinect Controller
            kinect = Kinect.getInstance();
            kinectHandler();

            //Starting and showing the admin window
            admin = new AdminWindow();
            //Starting and showing the admin window only if its normal mode
            if (!demoMode) {
                adminHandler();
                admin.Show();
            }
            else {
                //Demomode will close in 3 min due to inactivity
                timeoutTimerInit();
                kinect.ModeEnd += resetTimeoutTimer;
            }

            //Starting the standby window
            showStandby();

            //Starting the canvas screen
            canvas = null;
        }

        private void timeoutTimerInit() {
            timeoutTimer = new Timer(demoTimeout);
            timeoutTimer.Elapsed += HandleTimer;
            timeoutTimer.Start();
        }

        private void resetTimeoutTimer(CursorModes mode) {
            timeoutTimer.Stop();
            timeoutTimerInit();
        }

        private void HandleTimer(Object source, ElapsedEventArgs e) {
            exitAll();
        }

        private void enableWave() {
            Timer t = new Timer(5000);
            t.Elapsed += (s,e) => canWave = true;
            t.Start();
        }

        private async void namesInit() {
            Application.Current.Dispatcher.Invoke(new Action(async() => {
                names = new List<string>();
                String uri = "guestbooks?key=" + AUTHKEY;
                List<message> messages = await getMessageAsync(uri);
                foreach(message m in messages) {
                    names.Add(m.content);
                }
                if ((standby != null) && (names.Count != 0))
                    standby.setNames(names.ToArray());
            }));
        } 

        static async void createMessageAsync(message message) {
            HttpResponseMessage response = await client.PostAsJsonAsync("messages", message);
            response.EnsureSuccessStatusCode();
        }

        static async Task<List<message>> getMessageAsync(String path) {
            List<message> content = new List<message>();
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode) {
                content = await response.Content.ReadAsAsync<List<message>>();
            }
            return content;
        }

        /// <summary>
        /// Aligns the screens to the screen the user has choosen
        /// </summary>
        private void alignScreens() {
            if (standby != null)
                admin.AlignWindow(standby);

            if (canvas != null)
                admin.AlignWindow(canvas);
        }

        /// <summary>
        /// opens a new canvas screen
        /// </summary>
        private void showCanvas() {
            if (canvas == null) {
                canvas = new CanvasWindow();
                subscribeToCanvasHandler();
            }
            canvas.Show();
            alignScreens();
        }

        /// <summary>
        /// closes current canvas screen
        /// </summary>
        private void closeCanvas() {
            if (canvas != null) {
                canvas.Hide();
                unsubscribeToCanvasHandler();
                canvas.Close();
                canvas = null;
            }
        }

        /// <summary>
        /// opens a new standby screen
        /// </summary>
        private void showStandby() {
            enableWave();
            namesInit();
            if (standby == null) {
                standby = new StandbyWindow(names.ToArray());
            }
            standby.Show();
            alignScreens();
        }

        /// <summary>
        /// closes current standby screen
        /// </summary>
        private void closeStandby() {
            if (standby != null) {
                standby.Hide();
                standby.Close();
                standby = null;
            }
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
            if (!canWave) return;
            canWave = false;

            if (canvas == null) {
                closeStandby();
                showCanvas();
                if (kinect.getActiveBodyId() != bodyId) {
                    kinect.SetActiveBody(bodyId);
                }
            }
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
                    closeCanvas();
                    showStandby();
                }
            }));
        }

        /// <summary>
        /// When the cancel button is pressed in the canvas Screen
        /// Switch back to standby from canvas 
        /// As well as save the name from what was inputted
        /// </summary>
        private async void saveName(String s) {
            try {
              createMessageAsync(new message(s, AUTHKEY));
            } catch (Exception e) {
                Console.Out.WriteLine(e.Message);
            }
            names.Add(s);
            goToStandby();
        }

        /// <summary>
        /// Handles all the UI related Admin events
        /// </summary>
        private void adminHandler() {
            admin.Exit += (s, e) => {  exitAll(); };
            admin.Standby += (s, e) => { goToStandby(); };
            admin.ScreenChanged += () => { alignScreens(); };
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

    public class message {
        public String content { get; set; }
        public String key { get; set; }

        public message(String content, String key) {
            this.content = content;
            this.key = key;
        }
    }
}
