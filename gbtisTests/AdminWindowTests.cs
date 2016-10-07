using Microsoft.VisualStudio.TestTools.UnitTesting;
using gbtis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Timers;
using System.Windows.Controls;

namespace gbtis.Tests {
    [TestClass()]
    public class AdminWindowTests {
        private AdminWindow window;
        private Boolean eventFlag;

        [TestInitialize()]
        public void Initialize() {
            eventFlag = false;

            window = new AdminWindow();
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

            window.fileExitButton.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Timer t = new Timer(100);
            t.Start();
            t.Elapsed += (s, e) => {
                t.Stop();
            };

            while (t.Enabled) ;
            Assert.IsTrue(eventFlag);
        }

        [TestMethod()]
        public void TestCloseEvent() {
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

        [TestMethod()]
        public void TestStandbyEvent() {
            window.Standby += (s, e) => {
                eventFlag = true;
            };

            window.fileStandbyButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
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