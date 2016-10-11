using System.Windows;

namespace gbtis {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private Kinect kinect;

        public App() {
            kinect = new Kinect();

            UIController uicont = new UIController(sensor);
        }
    }
}