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
using System.Windows.Input;

namespace gbtis.Tests {
    [TestClass()]
    public class CanvasCursorTests {
        WindowStub stub;
        CanvasCursor cursor;
        
        [TestInitialize()]
        public void Initialize() {
            cursor = new CanvasCursor();
            stub = new WindowStub(cursor);
            
            cursor.RaiseEvent(new RoutedEventArgs(UserControl.LoadedEvent));
        }

        [TestMethod()]
        public void TestMovedMouseEvent() {
            bool passFlag = false;
            cursor.Moved += (s, e) => {
                if (cursor.Position == Mouse.GetPosition(stub))
                    passFlag = true;
            };

            stub.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, Environment.TickCount) {
                RoutedEvent = UIElement.MouseMoveEvent
            });
            Timer t = new Timer(100);
            t.Start();
            t.Elapsed += (s, e) => {
                t.Stop();
            };

            while (t.Enabled) ;
            Assert.IsTrue(passFlag);
        }

        [TestMethod()]
        public void TestDrawMouseEvent() {
            bool passFlag = false;
            cursor.Draw += (s, e) => {
                if (cursor.Type == CanvasCursor.CursorType.Draw)
                    passFlag = true;
            };

            cursor.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent
            });
            Timer t = new Timer(100);
            t.Start();
            t.Elapsed += (s, e) => {
                t.Stop();
            };

            while (t.Enabled) ;
            Assert.IsTrue(passFlag);
        }

        [TestMethod()]
        public void TestEraseMouseEvent() {
            bool passFlag = false;
            cursor.Erase += (s, e) => {
                if (cursor.Type == CanvasCursor.CursorType.Erase)
                    passFlag = true;
            };

            cursor.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right) {
                RoutedEvent = UIElement.MouseRightButtonDownEvent
            });
            Timer t = new Timer(100);
            t.Start();
            t.Elapsed += (s, e) => {
                t.Stop();
            };

            while (t.Enabled) ;
            Assert.IsTrue(passFlag);
        }
        
        [TestMethod()]
        public void TestCentering() {
            cursor.Type = CanvasCursor.CursorType.Idle;
            Point p1 = cursor.Position;

            cursor.Type = CanvasCursor.CursorType.Erase;
            Point p2 = cursor.Position;

            Assert.AreEqual(p1, p2);
        }

        public class WindowStub : Window {
            public WindowStub(UserControl child) {
                AddChild(child);
            }
        }
    }
}