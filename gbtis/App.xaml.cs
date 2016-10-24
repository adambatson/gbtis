using System.Windows;

namespace gbtis {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private Kinect kinect;

        public App() {
            //UIController uicont = new UIController();

            kinect = new Kinect();
            CanvasWindow canvas = new CanvasWindow(kinect);
            canvas.Show();
        }
    }
}