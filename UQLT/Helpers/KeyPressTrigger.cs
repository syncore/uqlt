using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace UQLT.Helpers
{
	/// <summary>
	/// Helper class that allows for easy binding of KeyUp and KeyDown actions with specific key specifiers within the View.
	/// See: http://caliburnmicro.codeplex.com/discussions/222164
	/// </summary>

	public class KeyPressTrigger : TriggerBase<FrameworkElement>
	{
		public enum KeyEventAction
		{
			KeyUp,
			KeyDown
		}

		public static readonly DependencyProperty KeyActionProperty =
		    DependencyProperty.Register("KeyAction", typeof(KeyEventAction), typeof(KeyPressTrigger),
		                                new PropertyMetadata(null));

		public static readonly DependencyProperty GestureProperty =
		    DependencyProperty.Register("Gesture", typeof(InputGesture), typeof(KeyPressTrigger),
		                                new PropertyMetadata(null));

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