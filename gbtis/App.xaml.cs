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
                uicont = new UIController(true, ConfigurationManager.AppSettings.Get("GBTISaaSAddress"), ConfigurationManager.AppSettings.Get("AuthorizationKey"));
            } else {
                uicont = new UIController(false, ConfigurationManager.AppSettings.Get("GBTISaaSAddress"), ConfigurationManager.AppSettings.Get("AuthorizationKey"));
            }
        }
    }
}