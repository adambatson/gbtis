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

namespace gbtis.Controls {
    /// <summary>
    /// Interaction logic for Pip.xaml
    /// </summary>
    public partial class Pip : UserControl {
        /// <summary>
        /// Set the pip as an active pip
        /// </summary>
        public bool Active {
            set {
                background.Color = value ? Colors.Red : Colors.Blue;
        }
        }

        /// <summary>
        /// Set the location of the pip
        /// </summary>
        public Point Position {
            set {
                try {
                    Margin = new Thickness(value.X - Width / 2, value.Y - Height / 2, 0, 0);
                } catch (Exception) { return; }
            }
            get { return new Point(Margin.Left, Margin.Top); }
        }

        /// <summary>
        /// Set the location of the pip
        /// </summary>
        public string Text {
            set {
                label.Text = value;
            }
        }

        public Pip() {
            InitializeComponent();
        }

        public Pip(String label) : this() {
            Text = label;
        }
    }
}
