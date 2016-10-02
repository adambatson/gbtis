using Microsoft.Kinect;
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
        public event EventHandler Standby;

        /// <summary>
        /// Initialize the window
        /// </summary>
        public AdminWindow() {
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
        /// Signal that a body has arrived in the status monitor
        /// </summary>
        /// <param name="id"></param>
        public void BodyArrived(uint id) {
            Border child = GetChild(id);
            if (child != null) {
                child.Background = new SolidColorBrush(Colors.Gray);
            }
        }

        /// <summary>
        /// Signal that a body left in the status monitor
        /// </summary>
        /// <param name="id"></param>
        public void BodyLeft(uint id) {
            Border child = GetChild(id);
            if (child != null) {
                child.Background = new SolidColorBrush(Colors.White);
                DeactivateBody(id);
            }
        }

        /// <summary>
        /// Set the active body in the status monitor
        /// </summary>
        /// <param name="id">An ID from 0 to 5</param>
        public void ActivateBody(uint id) {
            Border child = GetChild(id);
            if (child != null) {
                foreach (Border b in bodyStatus.Children) {
                    b.BorderBrush = new SolidColorBrush(Colors.Black);
                }

                child.BorderBrush = new SolidColorBrush(Colors.Red);
            }
        }

        /// <summary>
        /// Deactivate a body in the status monitor
        /// </summary>
        /// <param name="id">An ID from 0 to 5</param>
        public void DeactivateBody(uint id) {
            Border child = GetChild(id);
            if (child != null) {
                child.BorderBrush = new SolidColorBrush(Colors.Black);
            }
        }

        /// <summary>
        /// Get a body monitor
        /// </summary>
        /// <param name="id">An ID from 0 to 5</param>
        /// <returns>The Border object of the child</returns>
        private Border GetChild(uint id) {
            if (id >= bodyStatus.Children.Count)
                return null;

            return (Border)bodyStatus.Children[(int)id];
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
            About?.Invoke(this, args);
        }

        /// <summary>
        /// Return to standby mode
        /// </summary>
        /// <param name="sender">Event source</param>
        /// <param name="e">Event args</param>
        private void FileStandby_Click(object sender, EventArgs args) {
            Standby?.Invoke(this, args);
        }
    }
}
