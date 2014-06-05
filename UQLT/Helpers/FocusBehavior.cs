using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace UQLT.Helpers
{
    /// <summary>
    /// Helper class that allows setting the focus of a control from the viewmodel.
    /// From: https://caliburnmicro.codeplex.com/discussions/222892
    /// </summary>
    public class FocusBehavior : Behavior<Control>
    {
        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>
        /// Override this to hook up functionality to the AssociatedObject.
        /// </remarks>
        protected override void OnAttached()
        {
            AssociatedObject.GotFocus += (sender, args) => IsFocused = true;
            AssociatedObject.LostFocus += (sender, a) => IsFocused = false;
            AssociatedObject.Loaded += (o, a) =>
            {
                if (HasInitialFocus || IsFocused)
                {
                    AssociatedObject.Focus();
                }
            };

            base.OnAttached();
        }

        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.Register(
                "IsFocused",
                typeof(bool),
                typeof(FocusBehavior),
                new PropertyMetadata(false, (d, e) =>
        {
            if ((bool)e.NewValue)
            {
                ((FocusBehavior)d).AssociatedObject.Focus();
            }
        }));

        /// <summary>
        /// Gets or sets a value indicating whether this instance is focused.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is focused; otherwise, <c>false</c>.
        /// </value>
        public bool IsFocused
        {
            get
            {
                return (bool)GetValue(IsFocusedProperty);
            }
            set
            {
                SetValue(IsFocusedProperty, value);
            }
        }

        public static readonly DependencyProperty HasInitialFocusProperty =
            DependencyProperty.Register(
                "HasInitialFocus",
                typeof(bool),
                typeof(FocusBehavior),
                new PropertyMetadata(false, null));

        /// <summary>
        /// Gets or sets a value indicating whether this instance has initial focus.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has initial focus; otherwise, <c>false</c>.
        /// </value>
        public bool HasInitialFocus
        {
            get
            {
                return (bool)GetValue(HasInitialFocusProperty);
            }
            set
            {
                SetValue(HasInitialFocusProperty, value);
            }
        }
    }
}