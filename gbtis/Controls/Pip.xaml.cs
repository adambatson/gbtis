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
                Background = new SolidColorBrush(value ? Colors.Red : Colors.Blue);
        } }

        /// <summary>
        /// Set the location of the pip
        /// </summary>
        public Point Position {
            set {
                Margin = new Thickness(value.X - Width/2, value.Y - Height/2, 0, 0);
        } }

        /// <summary>
        /// Register the Text property of the control
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(Pip), new PropertyMetadata("#"));
        public string Text {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Register the Color property of the control
        /// </summary>
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color", typeof(Color), typeof(Pip), new PropertyMetadata(Colors.Blue));
        public Color Color {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public Pip() {
            InitializeComponent();
        }

        public Pip(String label) : this() {
            Text = label;
        }
    }
}
