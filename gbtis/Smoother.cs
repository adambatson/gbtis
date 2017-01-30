using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace gbtis {
    /// <summary>
    /// Coordinate smoothing
    /// </summary>
    class Smoother {
        public float Alpha { get; set; } = 0.4f;
        public int Count { get; set; } = 3;

        public List<float> X;
        public List<float> Y;

        private PointF previous;

        public Smoother() {
            Clear();
        }

        public void Clear() {
            X = new List<float>();
            Y = new List<float>();
            previous = new PointF();
        }

        public System.Windows.Point Next(PointF coordinates) {
            X.Add(coordinates.X);
            Y.Add(coordinates.Y);

            int c = (X.Count < Count) ? X.Count : Count;
            if (X.Count > Count) {
                X.RemoveAt(0); Y.RemoveAt(0);
            }

            PointF p = new PointF(
                Median(X, c), Median(Y, c));

            previous = (previous.IsEmpty) ? p : new PointF(
                Alpha * previous.X + (1 - Alpha) * Median(X, c),
                Alpha * previous.Y + (1 - Alpha) * Median(Y, c));
            return new System.Windows.Point((int)previous.X, (int)previous.Y);
        }

        public float Median(List<float> input, int Count) {
            List<float> copy = new List<float>(input);
            copy.Sort();
            if (copy.Count % 2 == 0) {
                return (copy[(int)Math.Floor((float)copy.Count / 2)] + copy[(int)Math.Ceiling((float)copy.Count / 2)]) / 2;
            } else {
                return copy[copy.Count / 2];
            }
        }
    }
}
