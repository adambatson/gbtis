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
using System.Windows.Shapes;

namespace gbtis {
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window {

        public event EventHandler Exit;
        public event EventHandler About;
        public event EventHandler ReturnToStandby;

        /// <summary>
        /// Initialize the window
        /// </summary>
        public AdminWindow() {
            InitializeComponent();
        }

        /// <summary>
        /// Close the program
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="args">Event args</param>
        private void FileExit_Click(object sender, EventArgs args) {
            EventHandler handler = Exit;
            if (handler != null)
                handler(this, args);
        }

        /// <summary>
        /// Display the about box
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="args">Event args</param>
        private void HelpAbout_Click(object sender, EventArgs args) {
            EventHandler handler = About;
            if (handler != null)
                handler(this, args);
        }

        /// <summary>
        /// Return to standby mode
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event args</param>
        private void ReturnToStandby_Click(object sender, EventArgs args) {
            EventHandler handler = ReturnToStandby;
            if (handler != null)
                handler(this, args);
        }
    }
}
