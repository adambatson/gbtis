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
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Display the about box
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="args">Event args</param>
        private void HelpAbout_Click(object sender, RoutedEventArgs args) {
            MessageBox.Show(
                gbtis.Properties.Resources.aboutText,
                gbtis.Properties.Resources.aboutTitle
            );
        }

        /// <summary>
        /// Return to standby mode
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event args</param>
        private void ReturnToStandby_Click(object sender, RoutedEventArgs e) {
            throw new NotImplementedException("Not yet implemented");
        }

        /// <summary>
        /// Playback test data to simulate the kinect. Button Tag property is the clip's name
        /// </summary>
        /// <param name="sender">A button sending the event</param>
        /// <param name="e">Event arguments</param>
        private void Playback_Click(object sender, RoutedEventArgs e) {
            var btn = sender as Button;
            var tag = btn.Tag as String;

            throw new NotImplementedException("Not yet implemented");
        }
    }
}
