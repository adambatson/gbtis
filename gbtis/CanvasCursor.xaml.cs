using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public partial class CanvasCursor : UserControl {
        public int Size { get; set; }

        private CursorType type;
        public CursorType Type {
            get { return type; }
            set {
                type = value;
                brushProperties(value);
            }
        }

        private Point position;
        public Point Position {
            get { return position; }
            set {
                position = value;

                // The actual position is the center
                Margin = new Thickness(
                    value.X - Width / 2,
                    value.Y - Height / 2,
                    0, 0
                );
            }
        }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public CanvasCursor() {
            InitializeComponent();
            Type = CursorType.Missing;
        }

        /// <summary>
        /// Set a brush's properties to match a type
        /// </summary>
        /// <param name="type">The type of cursor</param>
        private void brushProperties(CursorType type) {
            content.CornerRadius = new CornerRadius(type.round ? type.size / 2 : 0);
            content.Background = new SolidColorBrush(type.background);
            content.BorderBrush = new SolidColorBrush(type.border);

            //Resize
            Width = Height = type.size;
            Position = Position;
            Size = type.size;
        }

        /// <summary>
        /// Represents one of the cursor's modes
        /// </summary>
        public class CursorType {
            public Color background { get; set; }
            public Color border { get; set; }
            public bool round { get; set; }
            public int size { get; set; }

            /// <summary>
            /// Full constructor
            /// </summary>
            /// <param name="background">Backround color for the brush</param>
            /// <param name="border">Border color for the brush</param>
            /// <param name="size">Diameter of the brush</param>
            /// <param name="round">If true, round the brush</param>
            public CursorType(Color background, Color border, int size, bool round) {
                this.background = background;
                this.border = border;
                this.size = size;
                this.round = round;
            }

            /// <summary>
            /// Static constructor
            /// </summary>
            /// <param name="background">Backround color for the brush</param>
            /// <param name="border">Border color for the brush</param>
            /// <param name="size">Diameter of the brush</param>
            /// <param name="round">If true, round the brush</param>
            /// <returns></returns>
            public static CursorType Create(Color background, Color border, int size, bool round) {
                return new CursorType(background, border, size, round);
            }

            public static CursorType Missing = Create(Colors.White, Colors.White, 0, false);
            public static CursorType Idle = Create(Colors.Gray, Colors.DarkSlateGray, 15, true);
            public static CursorType Draw = Create(Colors.Black, Colors.White, 10, true);
            public static CursorType Erase = Create(Colors.White, Colors.Black, 50, false);
        }
    }
}
