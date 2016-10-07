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

            // Force button update
            namesBox.RaiseEvent(new SelectionChangedEventArgs(
                ListBox.SelectionChangedEvent, new List<string>(), new List<string>()));
        }

        public void bindEventsSource(ObservableCollection<string> src) {
            eventsBox.ItemsSource = src;
        }

        public void bindNameSources(ObservableCollection<string> srcPending, ObservableCollection<string> srcApproved) {
            pendingBox.ItemsSource = srcPending;
            namesBox.ItemsSource = srcApproved;
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

        private void Event_Selected(object sender, RoutedEventArgs e) {
            ComboBox box = (ComboBox)sender;
            string eventName = (String)box.SelectedItem;

            SelectEvent?.Invoke(this, new NamedEventArgs(eventName));
        }

        private void NewEvent_Click(object sender, RoutedEventArgs e) {
            string eventName = txtEvent.Text;
            if (eventName.Trim().Length > 0) {
                AddEvent?.Invoke(this, new NamedEventArgs(eventName));
            }
        }

        private void DeleteEvent_Click(object sender, RoutedEventArgs e) {
            string eventName = (String)eventsBox.SelectedItem;
            if (eventName == null) return;

            MessageBoxResult result = MessageBox.Show(
                string.Format("Really delete event {0} and all associated names?\nThis cannot be undone.", eventName),
                "Confirm event deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
                DeleteEvent?.Invoke(this, new NamedEventArgs(eventName));
        }

        private void DeleteName_Click(object sender, RoutedEventArgs e) {
            string name = (String)namesBox.SelectedItem;
            if (name == null) return;

            MessageBoxResult result = MessageBox.Show(
                string.Format("Remove '{0}' from the current event?", name),
                "Confirm name deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
                DeleteName?.Invoke(this, new NamedEventArgs(name));
        }

        private void ApproveName_Click(object sender, RoutedEventArgs e) {
            string name = (String)pendingBox.SelectedItem;
            if (name == null) return;

            ApproveName?.Invoke(this, new NamedEventArgs(name));
        }

        private void namesBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ListBox box = (ListBox)sender;
            deleteNameButton.IsEnabled = (box.SelectedIndex != -1) ? true : false;
            approveNameButton.IsEnabled = (box.SelectedIndex != -1) ? true : false;
        }

        public delegate void NamedEventHandler(object sender, NamedEventArgs args);
        public class NamedEventArgs : EventArgs {
            public String Text { get; set; }
            public NamedEventArgs(String text) : base() {
                Text = text;
            }
        }
    }
}
