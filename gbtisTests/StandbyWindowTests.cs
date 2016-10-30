using Microsoft.VisualStudio.TestTools.UnitTesting;
using gbtis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Kinect;

namespace gbtis.Tests {
    [TestClass()]
    public class StandbyWindowTests {
        private StandbyWindow window;
        private Boolean eventFlag;

        [TestInitialize()]
        public void Initialize() {
            eventFlag = false;

            window = new StandbyWindow();
            window.Show();
        }

        [TestCleanup()]
        public void Cleanup() {
            window.Close();
        }

        [TestMethod()]
        public void TestExitEvent() {
            window.Exit += (s, e) => {
                eventFlag = true;
            };

            window.Close();
            Timer t = new Timer(100);
            t.Start();
            t.Elapsed += (s, e) => {
                t.Stop();
            };

            while (t.Enabled) ;
            Assert.IsTrue(eventFlag);
        }
    }
}