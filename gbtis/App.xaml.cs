using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace gbtis {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private KinectSensor sensor;

        public App() {
            sensor = KinectSensor.GetDefault();
            sensor.Open();

            // Start the admin window
            AdminWindow admin = new AdminWindow();
            admin.Show();

            // Start the standby window
            //StandbyWindow standby = new StandbyWindow(sensor);
            //standby.Show();
        }
    }
}