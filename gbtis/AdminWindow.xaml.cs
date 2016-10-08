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

        // Event events
        public event NamedEventHandler AddEvent;
        public event NamedEventHandler SelectEvent;
        public event NamedEventHandler DeleteEvent;

        // Name events
        public event NamedEventHandler ApproveName;
        public event NamedEventHandler DeleteName;

        /// <summary>
        /// Initialize the window
        /// </summary>
        public AdminWindow() {
            DataContext = this;
            InitializeComponent();

            nameControl.AddEvent += (s, e) => AddEvent?.Invoke(this, e);
            nameControl.SelectEvent += (s, e) => SelectEvent?.Invoke(this, e);
            nameControl.DeleteEvent += (s, e) => DeleteEvent?.Invoke(this, e);

            nameControl.ApproveName += (s, e) => ApproveName?.Invoke(this, e);
            nameControl.DeleteName += (s, e) => DeleteName?.Invoke(this, e);
        }

        /// <summary>
        /// Bind the lists for the window's database manager
        /// </summary>
        /// <param name="events">List of events</param>
        /// <param name="namesPending">List of pendiing names</param>
        /// <param name="namesApproved">List of approved names</param>
        public void BindDataSources(ObservableCollection<string> events, ObservableCollection<string> namesPending, ObservableCollection<string> namesApproved) {
            nameControl.bindEventsSource(events);
            nameControl.bindNameSources(namesPending, namesApproved);
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

        // Events that pass along a string
        public delegate void NamedEventHandler(object sender, NamedEventArgs args);
    }
}
