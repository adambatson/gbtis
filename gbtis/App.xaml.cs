using System.Windows;
using System.Configuration;
using System.Collections.Specialized;

namespace gbtis {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        public App() {
            UIController uicont;

            if (ConfigurationManager.AppSettings.Get("DemoMode").Equals("1")) {
                uicont = new UIController(true);
            } else {
                uicont = new UIController(false);
            }
        }
    }
}