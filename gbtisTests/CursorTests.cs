using Microsoft.VisualStudio.TestTools.UnitTesting;
using gbtis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace gbtis.Tests {
    [TestClass()]
    public class CursorTests {
        WindowStub stub;
        Cursor cursor;

        [TestInitialize()]
        public void Initialize() {
            cursor = new Cursor();
            stub = new WindowStub(cursor);
            
            cursor.RaiseEvent(new RoutedEventArgs(UserControl.LoadedEvent));
        }

        [TestMethod()]
        public void modeTest() {
            cursor.Mode = CursorModes.Idle;
            Assert.AreEqual(Cursor.DEFAULT_SIZE, cursor.Size);

            cursor.Mode = CursorModes.Draw;
            Assert.AreEqual(Cursor.DEFAULT_SIZE, cursor.Size);

            cursor.Mode = CursorModes.Erase;
            Assert.AreEqual(Cursor.ERASER_SIZE, cursor.Size);
        }

        [TestMethod()]
        public void PositionTest() {
            Point p = new Point(100, 100);
            cursor.Position = p;
            Assert.AreEqual(p, cursor.Position);
        }

        /// <summary>
        /// Basic window for tests
        /// </summary>
        public class WindowStub : Window {
            public static readonly Size WINDOW_SIZE = new Size(1000, 1000);
            public WindowStub(UserControl child) {
                Width = WINDOW_SIZE.Width;
                Height = WINDOW_SIZE.Height;
                AddChild(child);
            }
        }
    }
}