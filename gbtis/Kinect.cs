using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace gbtis {

    /// <summary>
    /// Kinect Wrapper Class
    /// </summary>
    public class Kinect {

        public KinectSensor sensor {
            get;
        }

        public Kinect() {
            sensor = KinectSensor.GetDefault();
            sensor.Open();
            //There is a slight delay between when the sensor is opened
            //and when IsAvailable is set to true.  We need to ensure
            //that IsAvailable becomes true before anyone calls the
            //isAvailable() method.
            Thread.Sleep(500);
        }

        public Boolean isAvailable() {
            return sensor != null && sensor.IsAvailable;
        }
    }
}
