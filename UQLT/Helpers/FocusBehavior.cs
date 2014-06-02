using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
