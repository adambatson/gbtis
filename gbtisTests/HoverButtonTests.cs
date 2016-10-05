using Microsoft.VisualStudio.TestTools.UnitTesting;
using gbtis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Timers;
using System.Windows.Threading;
using System.Security.Permissions;

namespace gbtis.Tests {
    [TestClass()]
    public class HoverButtonTests {
        public static readonly Rect BUTTON_RECT = new Rect(0, 0, 100, 100);
        WindowStub stub;
        HoverButton button;

        [TestInitialize()]
        public void Initialize() {
            button = new HoverButton();
            button.MinHeight = button.MinWidth = 100;
            button.Measure(new Size(100, 100));
            button.Arrange(new Rect(0, 0, 100, 100));
            stub = new WindowStub(button);

            button.RaiseEvent(new RoutedEventArgs(UserControl.LoadedEvent));
        }

        [TestMethod()]
        public void testBounds() {
            Assert.IsTrue(button.CursorOver(BUTTON_RECT.TopLeft));
            Assert.IsTrue(button.CursorOver(BUTTON_RECT.BottomRight));

            Assert.IsFalse(button.CursorOver(new Point(BUTTON_RECT.Left - 1, BUTTON_RECT.Bottom)));
            Assert.IsFalse(button.CursorOver(new Point(BUTTON_RECT.Right, BUTTON_RECT.Top - 1)));

            Assert.IsFalse(button.CursorOver(new Point(BUTTON_RECT.Right + 1, BUTTON_RECT.Bottom)));
            Assert.IsFalse(button.CursorOver(new Point(BUTTON_RECT.Right, BUTTON_RECT.Bottom + 1)));
        }

        [TestMethod()]
        public void testHover() {
            bool passFlag = false;
            button.Clicked += (s, e) => {
                passFlag = true;
            };

            button.CursorOver(BUTTON_RECT.TopLeft);
            Timer t = new Timer(60000);
            t.Start();
            t.Elapsed += (s, e) => {
                t.Stop();
            };

            while (!passFlag) ;
            Assert.IsTrue(passFlag);
        }

        public class WindowStub : Window {
            public WindowStub(UserControl child) {
                AddVisualChild(child);
                AddLogicalChild(child);
                AddChild(child);
            }
        }
    }
}