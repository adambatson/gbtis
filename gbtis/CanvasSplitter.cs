using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace gbtis {
    class CanvasSplitter {
        BitmapImage src;
        private byte[] pixels;

        public CanvasSplitter(BitmapImage _src) {
            src = _src;

            int stride = (int)src.PixelWidth * PixelFormats.Bgr32.BitsPerPixel / 8;
            pixels = new byte[src.PixelHeight * stride];

            src.CopyPixels(pixels, stride, 0);
        }

        public Rect topLeftmost() {
            // First active row
            int top = nextMatching(0, true, false);

            // Next clear row

            // First active column
            int left = nextMatching(0, false, false);
        }

        public int nextMatching(int offset, bool row, bool clear) {
            for (int i = 0; i < (row ? src.PixelHeight : src.PixelWidth); i++) {
                if (!(isClear(i, row) ^ clear)) return i;
            }

            return (row ? src.PixelHeight : src.PixelWidth) - 1;
        }

        public bool isPixelClear(Point p) {
            int root = ((int)p.X + (int)p.Y * (int)src.PixelWidth) * PixelFormats.Bgr32.BitsPerPixel / 8;
            for (int i=0; i<PixelFormats.Bgr32.BitsPerPixel/8; i++) {
                if (pixels[root + i] < 0xFF) return false;
            }

            return true;
        }

        public bool isClear(int i, bool row) {
            return isClear(i, row, 0, (row ? src.PixelHeight : src.PixelWidth));
        }

        public bool isClear(int i, bool row, int start, int end) {
            for (int j = start; j < end; j++)
                if (!isPixelClear(new Point(i, j)))
                    return false;
            return true;
        }
    }
}
