using System.Windows;

namespace gbtis {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private Kinect kinect;

        public App() {
            kinect = new Kinect();

            // Start the admin window
            AdminWindow admin = new AdminWindow();
            admin.Show();

            // Start the standby window
            StandbyWindow standby = new StandbyWindow(kinect);
            standby.Show();
        }
    }
}