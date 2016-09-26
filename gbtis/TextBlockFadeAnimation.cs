using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Threading;

namespace gbtis {
    class TextBlockFadeAnimation {
        public static int TICK_INTERVAL = 10;
        public static int DEFAULT_ANIMATION_LENGTH = 1000;
        protected static Timer synchonisedTick = new Timer(TICK_INTERVAL);

        private TextBlock target;
        private String text;
        private double opacityDelta;

        /// <summary>
        /// Default animation length constructor
        /// </summary>
        /// <param name="target">The textblock being animated</param>
        /// <param name="text">The replacement text</param>
        public TextBlockFadeAnimation(TextBlock target, String text) : this(target, text, DEFAULT_ANIMATION_LENGTH) { }

        /// <summary>
        /// Full feature constructor for the animation
        /// </summary>
        /// <param name="target">The textblock being animated</param>
        /// <param name="text">The replacement text</param>
        /// <param name="animationLength">The length of the animation, in ms</param>
        public TextBlockFadeAnimation(TextBlock target, String text, int animationLength) {
            this.target = target;
            this.text = text;

            // Calculate opacity target to meet deadline, but start on a descent
            opacityDelta = (double)TICK_INTERVAL / (double)animationLength;
            opacityDelta = -opacityDelta;

            // Start animation tick timer
            synchonisedTick.Elapsed += handleTick;
            synchonisedTick.Start();
        }

        /// <summary>
        /// Handle one frame of the fading animation
        /// </summary>
        /// <param name="source">The timer causing the event</param>
        /// <param name="e">Arguments for the event</param>
        private void handleTick(Object source, ElapsedEventArgs e) {
            try {
                gbtis.App.Current.Dispatcher.Invoke(() => {
                    target.Opacity += opacityDelta;
                    if (opacityDelta < 0 && target.Opacity <= 0) {
                        opacityDelta = -opacityDelta;
                        target.Text = text;
                    } else if (target.Opacity >= 1) {
                        synchonisedTick.Elapsed -= handleTick;
                    }
                });
            } catch (OperationCanceledException) {
            } catch (NullReferenceException) { }
        }
    }
}
