using System.Windows;

namespace gbtis {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        public App() {
            //UIController uicont = new UIController();
            
            CanvasWindow canvas = new CanvasWindow();
            canvas.Show();
        }
    }
}