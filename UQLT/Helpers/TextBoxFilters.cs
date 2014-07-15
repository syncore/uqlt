using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using System.Windows.Interactivity;

namespace UQLT.Helpers
{
    /// <summary>
    /// A helper class (attached behavior) that allows for easy filtering of textboxes based on certain value types.
    /// Types are: All, AlphabetsOnly, PositiveIntegersOnly, IntegersOnly, DecimalOnly, PositiveDecimalOnly, AlphabetsUpperCaseOnly, Date
    /// </summary>
    /// <remarks>
    /// Attach to the textbox as a property in XAML:
    /// <TextBox helpers:TextBoxFilters.FilterText="AlphabetsOnly" />
    /// <TextBox helpers:TextBoxFilters.FilterText="PositiveIntegersOnly" />
    /// <TextBox helpers:TextBoxFilters.FilterText="IntegersOnly" />
    /// <TextBox helpers:TextBoxFilters.FilterText="PositiveDecimalOnly" />
    /// <TextBox helpers:TextBoxFilters.FilterText="DecimalOnly" />
    /// <TextBox helpers:TextBoxFilters.FilterText="AlphabetsUpperCaseOnly" />
    /// </remarks>
    public class TextBoxFilters : Behavior<Control>
    {
        #region TextBoxFilters
        public static DependencyProperty FilterProperty =
                      DependencyProperty.RegisterAttached("FilterText",
                                                  typeof(FilterTypes),
                                                  typeof(TextBoxFilters),
                                                  new PropertyMetadata(FilterTypes.All, FilterCheck));

        public static void SetFilterText(DependencyObject obj, FilterTypes value)
        {
            obj.SetValue(FilterProperty, value);
        }
        public static FilterTypes GetFilterText(UIElement obj)
        {
            return (FilterTypes)obj.GetValue(FilterProperty);
        }

        private static void FilterCheck(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var txtBox = d as TextBox;
            if (txtBox != null)
            {
                switch ((FilterTypes)e.NewValue)
                {
                    case FilterTypes.PositiveIntegersOnly:
                        txtBox.KeyDown += new KeyEventHandler(Filter_PositiveInteger);
                        break;
                    case FilterTypes.AlphabetsOnly:
                        txtBox.KeyDown += new KeyEventHandler(Filter_AlphabetsOnly);
                        break;
                    case FilterTypes.AlphabetsUpperCaseOnly:
                        txtBox.KeyDown += new KeyEventHandler(Filter_AlphabetsUPPERCaseOnly);
                        txtBox.TextChanged += new TextChangedEventHandler(Filter_AlphabetsUPPERCaseOnly);
                        break;
                    case FilterTypes.Date:
                        txtBox.KeyDown += new KeyEventHandler(Filter_Date);
                        break;
                    case FilterTypes.DecimalOnly:
                        txtBox.KeyDown += new KeyEventHandler(Filter_Decimal);
                        break;
                    case FilterTypes.IntegersOnly:
                        txtBox.KeyDown += new KeyEventHandler(Filter_IntegersOnly);
                        break;
                    case FilterTypes.PositiveDecimalOnly:
                        txtBox.KeyDown += new KeyEventHandler(Filter_PositiveDecimal);
                        break;
                }
            }
        }

        /// <summary>
        /// For filtering Positive Integers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Filter_PositiveInteger(object sender, KeyEventArgs e)
        {
            bool shiftKeyPressd = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            if (!(e.Key >= Key.D0 && e.Key <= Key.D9 && !shiftKeyPressd))
            {
                e.Handled = true;
            }
        }
        /// <summary>
        /// For filtering Integers Only
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Filter_IntegersOnly(object sender, KeyEventArgs e)
        {
            bool shiftKeyPressd = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            if (!((e.Key >= Key.D0 && e.Key <= Key.D9 && !shiftKeyPressd)))
            {
                e.Handled = true;
            }
        }
        /// <summary>
        /// For filtering Date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Filter_Date(object sender, KeyEventArgs e)
        {
            bool shiftKeyPressd = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            if (!((e.Key >= Key.D0 && e.Key <= Key.D9 && !shiftKeyPressd)))
            {
                e.Handled = true;
            }
        }
        /// <summary>
        /// For filtering Decimal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Filter_Decimal(object sender, KeyEventArgs e)
        {
            bool shiftKeyPressd = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            if (!((e.Key >= Key.D0 && e.Key <= Key.D9 && !shiftKeyPressd)))
            {
                e.Handled = true;
            }
        }
        /// <summary>
        /// For filtering Positive Decimal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Filter_PositiveDecimal(object sender, KeyEventArgs e)
        {
            bool shiftKeyPressd = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            if (!((e.Key >= Key.D0 && e.Key <= Key.D9 && !shiftKeyPressd)))
            {
                e.Handled = true;
            }
        }
        /// <summary>
        /// For filtering Alphabets Only
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Filter_AlphabetsOnly(object sender, KeyEventArgs e)
        {
            var allAlphabets = new List<string>("q,w,e,r,t,y,u,i,o,p,a,s,d,f,g,h,j,k,l,z,x,c,v,b,n,m,Q,W,E,R,T,Y,U,I,O,P,A,S,D,F,G,H,J,K,L,Z,X,C,V,B,N,M,Space".Split(','));
            if (!allAlphabets.Contains(e.Key.ToString()))
            {
                e.Handled = true;
            }
        }
        /// <summary>
        /// For filtering Alphabets UPPER Case Only
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Filter_AlphabetsUPPERCaseOnly(object sender, KeyEventArgs e)
        {
            var allAlphabets = new List<string>("q,w,e,r,t,y,u,i,o,p,a,s,d,f,g,h,j,k,l,z,x,c,v,b,n,m,Q,W,E,R,T,Y,U,I,O,P,A,S,D,F,G,H,J,K,L,Z,X,C,V,B,N,M,Space".Split(','));
            if (!allAlphabets.Contains(e.Key.ToString()))
            {
                e.Handled = true;
            }
        }
        /// <summary>
        /// Change entered to upper case
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Filter_AlphabetsUPPERCaseOnly(object sender, TextChangedEventArgs e)
        {
            var textbox1 = sender as TextBox;
            int intCurrentPos = textbox1.SelectionStart;
            textbox1.Text = textbox1.Text.ToUpper();
            textbox1.SelectionStart = intCurrentPos;
        }
        #endregion
    }

    public enum FilterTypes
    {
        All,
        AlphabetsOnly,
        PositiveIntegersOnly,
        IntegersOnly,
        DecimalOnly,
        PositiveDecimalOnly,
        AlphabetsUpperCaseOnly,
        Date
    }
}
