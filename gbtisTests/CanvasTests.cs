using Microsoft.VisualStudio.TestTools.UnitTesting;
using gbtis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace gbtis.Tests {
    [TestClass()]
    public class CanvasTests {
        WindowStub stub;
        Canvas canvas;

        [TestInitialize()]
        public void Initialize() {
            canvas = new Canvas();

            canvas.MinHeight = canvas.MinWidth = 100;
            canvas.Measure(new Size(100, 100));
            canvas.Arrange(new Rect(0, 0, 100, 100));
            stub = new WindowStub(canvas);

            canvas.InitializeCanvas();
            canvas.RaiseEvent(new RoutedEventArgs(UserControl.LoadedEvent));
        }

        [TestMethod()]
        public void MarkTest() {
            int bpp = PixelFormats.Bgr32.BitsPerPixel / 8;
            canvas.Mark(new Point(0, 0), 1, false);
            byte[] pixels = canvas.Pixels();

            for (int i = 0; i < bpp - 1; i++)
                Assert.AreEqual(0x0, pixels[i]);
            Assert.AreEqual(0xFF, pixels[bpp - 1]);
        }

        [TestMethod()]
        public void EraseTest() {
            int bpp = PixelFormats.Bgr32.BitsPerPixel / 8;
            canvas.Mark(new Point(0, 0), 1, false);
            canvas.Erase(new Point(0, 0), 1, false);
            byte[] pixels = canvas.Pixels();

            for (int i = 0; i < bpp - 1; i++)
                Assert.AreEqual(0xFF, pixels[i]);
            Assert.AreEqual(0xFF, pixels[bpp - 1]);
        }

        [TestMethod()]
        public void ClearCanvasTest() {
            int bpp = PixelFormats.Bgr32.BitsPerPixel / 8;
            canvas.Mark(new Point(0, 0), 1, false);
            canvas.ClearCanvas();
            byte[] pixels = canvas.Pixels();

            for (int i = 0; i < pixels.Length; i++)
                Assert.AreEqual(0xFF, pixels[i]);
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