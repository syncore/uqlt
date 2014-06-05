using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace UQLT.Helpers
{
    /// <summary>
    /// Helper class that allows for easy binding of KeyUp and KeyDown actions with specific key
    /// specifiers within the View.
    /// See: http://caliburnmicro.codeplex.com/discussions/222164
    /// </summary>

    public class KeyPressTrigger : TriggerBase<FrameworkElement>
    {
        /// <summary>
        /// The gesture property
        /// </summary>
        public static readonly DependencyProperty GestureProperty =
            DependencyProperty.Register("Gesture", typeof(InputGesture), typeof(KeyPressTrigger),
                                        new PropertyMetadata(null));

        /// <summary>
        /// The key action property
        /// </summary>
        public static readonly DependencyProperty KeyActionProperty =
            DependencyProperty.Register("KeyAction", typeof(KeyEventAction), typeof(KeyPressTrigger),
                                        new PropertyMetadata(null));

        public enum KeyEventAction
        {
            KeyUp,
            KeyDown
        }

        /// <summary>
        /// Gets or sets the gesture.
        /// </summary>
        /// <value>The gesture.</value>
        [TypeConverterAttribute(typeof(KeyGestureConverter)), Category("KeyPress Properties")]
        public InputGesture Gesture
        {
            get
            {
                return (InputGesture)GetValue(GestureProperty);
            }
            set
            {
                SetValue(GestureProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the key action.
        /// </summary>
        /// <value>The key action.</value>
        [Category("KeyPress Properties")]
        public KeyEventAction KeyAction
        {
            get
            {
                return (KeyEventAction)GetValue(KeyActionProperty);
            }
            set
            {
                SetValue(KeyActionProperty, value);
            }
        }

        /// <summary>
        /// Called after the trigger is attached to an AssociatedObject.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">KeyAction</exception>
        protected override void OnAttached()
        {
            base.OnAttached();

            if (KeyAction == KeyEventAction.KeyUp)
            {
                AssociatedObject.KeyUp += OnKeyPress;
            }
            else if (KeyAction == KeyEventAction.KeyDown)
            {
                AssociatedObject.KeyDown += OnKeyPress;
            }
            else
            {
                throw new ArgumentOutOfRangeException("KeyAction", string.Format("{0} is not support.", KeyAction));
            }
        }

        /// <summary>
        /// Called when the trigger is being detached from its AssociatedObject, but before it has
        /// actually occurred.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">KeyAction</exception>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (KeyAction == KeyEventAction.KeyUp)
            {
                AssociatedObject.KeyUp -= OnKeyPress;
            }
            else if (KeyAction == KeyEventAction.KeyDown)
            {
                AssociatedObject.KeyDown -= OnKeyPress;
            }
            else
            {
                throw new ArgumentOutOfRangeException("KeyAction", string.Format("{0} is not support.", KeyAction));
            }
        }

        /// <summary>
        /// Called when a given key is pressed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        private void OnKeyPress(object sender, KeyEventArgs args)
        {
            KeyGesture kGesture = Gesture as KeyGesture;
            if (kGesture == null)
            {
                return;
            }

            if (kGesture.Matches(null, args))
            {
                this.InvokeActions(null);
            }
        }
    }
}