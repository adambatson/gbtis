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
    /// <summary>
    /// Interaction logic for BodyCounter.xaml
    /// </summary>
    public partial class BodyCounter : UserControl {
        public BodyCounter() {
            InitializeComponent();

            // TODO: Subscribe to body presence events
            // TODO: Subscribe to body activation events
        }

        /// <summary>
        /// Body element border by id
        /// </summary>
        /// <param name="body">The id of the body - 0 - 5</param>
        /// <returns>The body border control</returns>
        private Border getBody(uint body) {
            object border = FindName(
                string.Format("body{0}", body)
            );
            return (Border)border;
        }

        /// <summary>
        /// Mark a body as the active body
        /// </summary>
        /// <param name="body">The id of the body - 0 - 5</param>
        /// <param name="present">True if is the active body</param>
        private void markActive(uint body, bool active) {
            Border border = getBody(body);
            if (border != null) {
                try {
                    this.Dispatcher.Invoke(() => {
                        border.BorderBrush = new SolidColorBrush(active ? Colors.Red : Colors.Black);
                    });
                } catch (OperationCanceledException) { }
            }

            if (active) {
                for (uint i = 0; i < 6; i++)
                    if (i != body) markActive(i, false);
            }
        }

        /// <summary>
        /// Mark a body as present
        /// </summary>
        /// <param name="body">The id of the body - 0 - 5</param>
        /// <param name="present">True if present</param>
        private void markPresent(uint body, bool present) {
            Border border = getBody(body);
            if (border != null) {
                try {
                    this.Dispatcher.Invoke(() => {
                        border.Background = new SolidColorBrush(present ? Colors.Gray : Colors.White);
                    });
                } catch (OperationCanceledException) { }
            }
        }

        private void activate_body(object sender, MouseButtonEventArgs args) {
            FrameworkElement e = (FrameworkElement)sender;
            UInt32 id = Convert.ToUInt32(e.Tag);

            // TODO: Trigger active body set on kinect
            markActive(id, true);
        }
    }
}
