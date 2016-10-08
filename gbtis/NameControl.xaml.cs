using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// <summary>
    /// Interaction logic for NameControl.xaml
    /// </summary>
    public partial class NameControl : UserControl {
        // Event events
        public event NamedEventHandler AddEvent;
        public event NamedEventHandler SelectEvent;
        public event NamedEventHandler DeleteEvent;

        // Name events
        public event NamedEventHandler ApproveName;
        public event NamedEventHandler DeleteName;

        public NameControl() {
            InitializeComponent();

            // Force button updates
            eventsBox.RaiseEvent(new SelectionChangedEventArgs(
                ListBox.SelectionChangedEvent, new List<string>(), new List<string>()));
            txtEvent.RaiseEvent(new TextChangedEventArgs(
                TextBox.TextChangedEvent, UndoAction.None, new List<TextChange>()));
            pendingBox.RaiseEvent(new SelectionChangedEventArgs(
                ListBox.SelectionChangedEvent, new List<string>(), new List<string>()));
            namesBox.RaiseEvent(new SelectionChangedEventArgs(
                ListBox.SelectionChangedEvent, new List<string>(), new List<string>()));
        }

        /// <summary>
        /// Bind the list of events to the view
        /// </summary>
        /// <param name="src">List of events</param>
        public void bindEventsSource(ObservableCollection<string> src) {
            eventsBox.ItemsSource = src;
        }

        /// <summary>
        /// Bind the name lists to the view
        /// </summary>
        /// <param name="srcPending">List of pending names</param>
        /// <param name="srcApproved">List of approved names</param>
        public void bindNameSources(ObservableCollection<string> srcPending, ObservableCollection<string> srcApproved) {
            pendingBox.ItemsSource = srcPending;
            namesBox.ItemsSource = srcApproved;
        }

        /// <summary>
        /// A new event was selected
        /// </summary>
        /// <param name="sender">The events box</param>
        /// <param name="e">Event args</param>
        private void Event_Selected(object sender, RoutedEventArgs e) {
            ComboBox box = (ComboBox)sender;
            string eventName = (String)box.SelectedItem;

            // Button status
            deleteEventButton.IsEnabled = (box.SelectedIndex != -1);

            SelectEvent?.Invoke(this, new NamedEventArgs(eventName));
        }

        /// <summary>
        /// User clicked the new event button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewEvent_Click(object sender, RoutedEventArgs e) {
            string eventName = txtEvent.Text;
            if (eventName.Trim().Length > 0) {
                AddEvent?.Invoke(this, new NamedEventArgs(eventName));
            }
        }

        /// <summary>
        /// Contents of the event name box changed
        /// </summary>
        /// <param name="sender">The box</param>
        /// <param name="e">Event args</param>
        private void txtEvent_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox box = (TextBox)sender;
            addEventButton.IsEnabled = box.Text.Length != 0;
        }

        /// <summary>
        /// User clicked the delete event button
        /// </summary>
        /// <param name="sender">The button</param>
        /// <param name="e">Event args</param>
        private void DeleteEvent_Click(object sender, RoutedEventArgs e) {
            string eventName = (String)eventsBox.SelectedItem;
            if (eventName == null) return;

            // Confirm deletion action
            MessageBoxResult result = MessageBox.Show(
                string.Format("Really delete event {0} and all associated names?\nThis cannot be undone.", eventName),
                "Confirm event deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
                DeleteEvent?.Invoke(this, new NamedEventArgs(eventName));
        }

        /// <summary>
        /// User clicked the delete name button
        /// </summary>
        /// <param name="sender">The button</param>
        /// <param name="e">Event args</param>
        private void DeleteName_Click(object sender, RoutedEventArgs e) {
            string name = (namesBox.SelectedIndex != -1) ?
                (String)namesBox.SelectedItem :
                (String)pendingBox.SelectedItem;
            if (name == null) return;

            // Confirm deletion action
            MessageBoxResult result = MessageBox.Show(
                string.Format("Remove '{0}' from the current event?", name),
                "Confirm name deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
                DeleteName?.Invoke(this, new NamedEventArgs(name));
        }

        /// <summary>
        /// Approve a name
        /// </summary>
        /// <param name="sender">The button</param>
        /// <param name="e">Event args</param>
        private void ApproveName_Click(object sender, RoutedEventArgs e) {
            string name = (String)pendingBox.SelectedItem;
            if (name == null) return;

            ApproveName?.Invoke(this, new NamedEventArgs(name));
        }

        /// <summary>
        /// Activated when a name is selected/unselected
        /// </summary>
        /// <param name="sender">The names box</param>
        /// <param name="e">Event args</param>
        private void namesBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ListBox box = (ListBox)sender;

            // Enable relevant buttons
            deleteNameButton.IsEnabled = (box.SelectedIndex == -1 && pendingBox.SelectedIndex == -1) ? false : true;

            // Clear the pending box' selections
            if (e.AddedItems.Count > 0)
                pendingBox.UnselectAll();
        }

        /// <summary>
        /// Activated when a pending name is selected/unselected
        /// </summary>
        /// <param name="sender">The pending box</param>
        /// <param name="e">Event args</param>
        private void pendingBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ListBox box = (ListBox)sender;

            // Enable relevant buttons
            approveNameButton.IsEnabled = (box.SelectedIndex != -1) ? true : false;
            deleteNameButton.IsEnabled = (box.SelectedIndex == -1 && namesBox.SelectedIndex == -1) ? false : true;

            // Clear the name box' selections
            if (e.AddedItems.Count > 0) {
                namesBox.UnselectAll();
            }
        }

        // Events that pass along a string
        public delegate void NamedEventHandler(object sender, NamedEventArgs args);
    }

    /// <summary>
    /// Event arguments with a string
    /// </summary>
    public class NamedEventArgs : EventArgs {
        public String Text { get; set; }
        public NamedEventArgs(String text) : base() {
            Text = text;
        }
    }
}