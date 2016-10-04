using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace gbtis {
    /// <summary>
    /// Interaction logic for Canvas.xaml
    /// </summary>
    public partial class Canvas : UserControl {
        Point? previous;
        byte[] pixels;
        int stride;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Canvas() {
            InitializeComponent();
            
            // Update timer
            Timer t = new Timer(50);
            t.Elapsed += (s, e) => updateSource();
            t.Start();
        }

        /// <summary>
        /// Init pixel array
        /// </summary>
        public void InitializeCanvas() {
            if ((int)ActualWidth == 0)
                return;

            // Initialize canvas to the right size
            pixels = new byte[(int)ActualWidth * (int)ActualHeight * ((PixelFormats.Bgr32.BitsPerPixel + 7) / 8)];
            stride = (int)ActualWidth * PixelFormats.Bgr32.BitsPerPixel / 8;
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = 0xFF;
        }
        
        /// <summary>
        /// Mark a point
        /// </summary>
        /// <param name="p">Center point</param>
        /// <param name="size">Brush size</param>
        public void Mark(Point p, int size) {
            radialUpdate(p, size, true, true);
        }

        /// <summary>
        /// Erase a point
        /// </summary>
        /// <param name="p">Center point</param>
        /// <param name="size">Brush size</param>
        public void Erase(Point p, int size) {
            radialUpdate(p, size, false, false);
        }

        /// <summary>
        /// Clear the previous position of the cursor
        /// </summary>
        public void ClearPrevious() {
            previous = null;
        }

        /// <summary>
        /// Get the slope of the line between 2 points
        /// </summary>
        /// <param name="src">Source point</param>
        /// <param name="dst">Destination point</param>
        /// <returns>The slope</returns>
        private double slope(Point src, Point dst) {
            return (dst.Y - src.Y) / (dst.X - src.X);
        }

        /// <summary>
        /// Get the y intercept of a line
        /// </summary>
        /// <param name="p">A point on the line</param>
        /// <param name="slope">The slope of the line</param>
        /// <returns></returns>
        private double yIntercept(Point p, double slope) {
            return p.Y - slope * p.X;
        }

        /// <summary>
        /// Update around a point
        /// </summary>
        /// <param name="p">Center point</param>
        /// <param name="size">Brush size</param>
        /// <param name="rounded">True if round</param>
        /// <param name="mode">True for on, false for off</param>
        private void radialUpdate(Point p, int size, bool rounded, bool mode) {
            // Adjust size
            if (!rounded)
                size /= 2;

            // Go over every point in range
            for (int x = (int)p.X - size; x < (int)p.X + size; x++) {
                for (int y = (int)p.Y - size; y < (int)p.Y + size; y++) {
                    // Check corners
                    if (rounded) {
                        int d = (int)(Math.Pow((int)p.X - x, 2) + Math.Pow((int)p.Y - y, 2));
                        if (d > size * size / 4) continue;
                    }

                    // Write to point
                    if (x >= 0 && x < (int)ActualWidth && y >= 0 && y < (int)ActualHeight) {
                        set(x, y, (byte) (mode ? 0x00 : 0xFF));
                    }
                }
            }

            if (previous.HasValue) {
                Point src = previous.Value;
                previous = null;
                double m = slope(src, p);
                double b = yIntercept(p, m);
                if (src.X < p.X) {
                    for (int x = (int)p.X; x > (int)src.X; x--) {
                        int y = (int)(m * x + b);
                        radialUpdate(new Point(x, y), size, rounded, mode);
                    }
                } else {
                    for (int x = (int)p.X; x < (int)src.X; x++) {
                        int y = (int)(m * x + b);
                        radialUpdate(new Point(x, y), size, rounded, mode);
                    }
                }
            }

            previous = p;
        }

        /// <summary>
        /// Draw to a given pixel
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="value">Value to write</param>
        private void set(int x, int y, byte value) {
            if (pixels == null)
                InitializeCanvas();
  
            int root = ((int)x + (int)y * (int)ActualWidth) * 4;
            pixels[root] = value;
            pixels[root + 1] = value;
            pixels[root + 2] = value;
            pixels[root + 3] = 0xFF;
        }

        /// <summary>
        /// Update the canvas
        /// </summary>
        private void updateSource() {
            try {
                this.Dispatcher.Invoke(() => {
                    if (pixels == null)
                        InitializeCanvas();

                    content.Source = BitmapSource.Create((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Bgr32, null, pixels, stride);
                });
            } catch (OperationCanceledException) { }
        }
    }
}
