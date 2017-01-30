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
        public static float DEFAULT_ALPHA = 0.5f;
        public static int DEFAULT_COUNT = 3;

        public float Alpha { get; set; }
        public int Count { get; set; }

        /// <summary>
        /// Turn the median filter on and off
        /// </summary>
        public bool MedianFiltering {
            get { return Count == 1; }
            set { Count = (value) ? DEFAULT_COUNT : 1; }
        }

        /// <summary>
        /// Turn the moving average filter on and off
        /// </summary>
        public bool MovingAverageFiltering {
            get { return Alpha == 1; }
            set { Alpha = (value) ? DEFAULT_ALPHA : 1; }
        }

        public List<float> X;
        public List<float> Y;
        private PointF previous;

        /// <summary>
        /// Build a new smoother
        /// </summary>
        public Smoother() : this(DEFAULT_ALPHA, DEFAULT_COUNT) {
        }

        /// <summary>
        /// Build a new smoother
        /// </summary>
        /// <param name="alpha">Moving average filtering level</param>
        /// <param name="count">Median filtering level</param>
        public Smoother(float alpha, int count) {
            Alpha = alpha;
            Count = count;
            Clear();
        }

        /// <summary>
        /// Reset the filtering histories
        /// </summary>
        public void Clear() {
            X = new List<float>();
            Y = new List<float>();
            previous = new PointF();
        }

        /// <summary>
        /// Filter a new point
        /// </summary>
        /// <param name="coordinates">New input point</param>
        /// <returns>Filtered point</returns>
        public System.Windows.Point Next(PointF coordinates) {
            // Append to history
            X.Add(coordinates.X);
            Y.Add(coordinates.Y);

            // Trim to history size
            if (X.Count > Count) {
                X.RemoveAt(0); Y.RemoveAt(0);
            }

            // Apply the filters
            bool empty = previous.IsEmpty;
            previous.X = (empty) ? Median(X) : (1 - Alpha) * previous.X + Alpha * Median(X);
            previous.Y = (empty) ? Median(Y) : (1 - Alpha) * previous.Y + Alpha * Median(Y);
            return new System.Windows.Point((int)previous.X, (int)previous.Y);
        }

        /// <summary>
        /// Get the median of a set of inputs
        /// </summary>
        /// <param name="input">Input set</param>
        /// <returns>Median value</returns>
        public float Median(List<float> input) {
            // Get a sorted version of the input
            List<float> copy = new List<float>(input);
            copy.Sort();

            if (copy.Count % 2 == 0) {
                // Even number. Average the 2 center values
                return (copy[(int)Math.Floor((float)copy.Count / 2)] + copy[(int)Math.Ceiling((float)copy.Count / 2)]) / 2;
            } else {
                // Odd. Just the center value
                return copy[copy.Count / 2];
            }
        }
    }
}
