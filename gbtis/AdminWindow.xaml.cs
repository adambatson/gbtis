using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        // Window events
        public event EventHandler Exit;
        public event EventHandler Standby;
        public event EventHandler Input;

        /// <summary>
        /// Initialize the window
        /// </summary>
        public AdminWindow() {
            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Set the status bar's text
        /// </summary>
        /// <param name="text"></param>
        public void SetStatus(String text) {
            statusText.Text = text;
        }

        /// <summary>
        /// Update the frame in the video feed
        /// </summary>
        /// <param name="frame">The new frame</param>
        public void FrameArrived(ImageSource frame) {
            sensorFeed.Source = frame;
        }

        /// <summary>
        /// Close the program
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="args">Event args</param>
        private void FileExit_Click(object sender, EventArgs args) {
            Exit?.Invoke(this, args);
        }

        /// <summary>
        /// Display the about box
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="args">Event args</param>
        private void HelpAbout_Click(object sender, EventArgs args) {
            MessageBox.Show(
                Properties.Resources.aboutText,
                Properties.Resources.aboutTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        /// <summary>
        /// Return to standby mode
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event args</param>
        private void FileStandby_Click(object sender, EventArgs args) {
            Standby?.Invoke(this, args);
        }

        /// <summary>
        /// Start the input mode
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event args</param>
        private void FileInput_Click(object sender, EventArgs args) {
            Input?.Invoke(this, args);
        }
    }
}
