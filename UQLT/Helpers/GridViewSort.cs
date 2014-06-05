using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace UQLT.Helpers
{
    /// <summary>
    /// Helper class that automatically sorts a grid view column
    /// By: Thomas Levesque
    /// http://www.thomaslevesque.com/2009/08/04/wpf-automatically-sort-a-gridview-continued/
    /// </summary>
    public class GridViewSort
    {
        #region Public attached properties

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static ICommand GetCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(CommandProperty);
        }

        /// <summary>
        /// Sets the command.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(GridViewSort),
                new UIPropertyMetadata(
                    null,
                    (o, e) =>
                    {
                        ItemsControl listView = o as ItemsControl;
                        if (listView != null)
                        {
                            if (!GetAutoSort(listView)) // Don't change click handler if AutoSort enabled
                            {
                                if (e.OldValue != null && e.NewValue == null)
                                {
                                    listView.RemoveHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                                }
                                if (e.OldValue == null && e.NewValue != null)
                                {
                                    listView.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                                }
                            }
                        }
                    }
                )
            );

        /// <summary>
        /// Determines whether automatic sorting is enabled or disabled.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static bool GetAutoSort(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoSortProperty);
        }

        /// <summary>
        /// Sets the automatic sort.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetAutoSort(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoSortProperty, value);
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for AutoSort.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty AutoSortProperty =
            DependencyProperty.RegisterAttached(
                "AutoSort",
                typeof(bool),
                typeof(GridViewSort),
                new UIPropertyMetadata(
                    false,
                    (o, e) =>
                    {
                        ListView listView = o as ListView;
                        if (listView != null)
                        {
                            if (GetCommand(listView) == null) // Don't change click handler if a command is set
                            {
                                bool oldValue = (bool)e.OldValue;
                                bool newValue = (bool)e.NewValue;
                                if (oldValue && !newValue)
                                {
                                    listView.RemoveHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                                }
                                if (!oldValue && newValue)
                                {
                                    listView.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                                }
                            }
                        }
                    }
                )
            );

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static string GetPropertyName(DependencyObject obj)
        {
            return (string)obj.GetValue(PropertyNameProperty);
        }

        /// <summary>
        /// Sets the name of the property.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetPropertyName(DependencyObject obj, string value)
        {
            obj.SetValue(PropertyNameProperty, value);
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for PropertyName.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.RegisterAttached(
                "PropertyName",
                typeof(string),
                typeof(GridViewSort),
                new UIPropertyMetadata(null)
            );

        /// <summary>
        /// Determines whether to show the sort glyph or not.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static bool GetShowSortGlyph(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShowSortGlyphProperty);
        }

        /// <summary>
        /// Sets the show sort glyph property.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetShowSortGlyph(DependencyObject obj, bool value)
        {
            obj.SetValue(ShowSortGlyphProperty, value);
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for ShowSortGlyph.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty ShowSortGlyphProperty =
            DependencyProperty.RegisterAttached("ShowSortGlyph", typeof(bool), typeof(GridViewSort), new UIPropertyMetadata(true));

        /// <summary>
        /// Gets the sort glyph ascending property.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static ImageSource GetSortGlyphAscending(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(SortGlyphAscendingProperty);
        }

        /// <summary>
        /// Sets the sort glyph ascending property.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetSortGlyphAscending(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(SortGlyphAscendingProperty, value);
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for SortGlyphAscending.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty SortGlyphAscendingProperty =
            DependencyProperty.RegisterAttached("SortGlyphAscending", typeof(ImageSource), typeof(GridViewSort), new UIPropertyMetadata(null));

        /// <summary>
        /// Gets the sort glyph descending property.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static ImageSource GetSortGlyphDescending(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(SortGlyphDescendingProperty);
        }

        /// <summary>
        /// Sets the sort glyph descending property.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetSortGlyphDescending(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(SortGlyphDescendingProperty, value);
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for SortGlyphDescending.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty SortGlyphDescendingProperty =
            DependencyProperty.RegisterAttached("SortGlyphDescending", typeof(ImageSource), typeof(GridViewSort), new UIPropertyMetadata(null));

        #endregion Public attached properties

        #region Private attached properties

        /// <summary>
        /// Gets the sorted column header property.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        private static GridViewColumnHeader GetSortedColumnHeader(DependencyObject obj)
        {
            return (GridViewColumnHeader)obj.GetValue(SortedColumnHeaderProperty);
        }

        /// <summary>
        /// Sets the sorted column header property.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        private static void SetSortedColumnHeader(DependencyObject obj, GridViewColumnHeader value)
        {
            obj.SetValue(SortedColumnHeaderProperty, value);
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for SortedColumn.  This enables animation, styling, binding, etc...
        /// </summary>
        private static readonly DependencyProperty SortedColumnHeaderProperty =
            DependencyProperty.RegisterAttached("SortedColumnHeader", typeof(GridViewColumnHeader), typeof(GridViewSort), new UIPropertyMetadata(null));

        #endregion Private attached properties

        #region Column header click event handler

        /// <summary>
        /// Handles the Click event of the ColumnHeader control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private static void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked != null && headerClicked.Column != null)
            {
                string propertyName = GetPropertyName(headerClicked.Column);
                if (!string.IsNullOrEmpty(propertyName))
                {
                    ListView listView = GetAncestor<ListView>(headerClicked);
                    if (listView != null)
                    {
                        ICommand command = GetCommand(listView);
                        if (command != null)
                        {
                            if (command.CanExecute(propertyName))
                            {
                                command.Execute(propertyName);
                            }
                        }
                        else if (GetAutoSort(listView))
                        {
                            ApplySort(listView.Items, propertyName, listView, headerClicked);
                        }
                    }
                }
            }
        }

        #endregion Column header click event handler

        #region Helper methods

        /// <summary>
        /// Gets the ancestor.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reference">The reference.</param>
        /// <returns></returns>
        public static T GetAncestor<T>(DependencyObject reference) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(reference);
            while (!(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            if (parent != null)
            {
                return (T)parent;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Applies the sort.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="listView">The list view.</param>
        /// <param name="sortedColumnHeader">The sorted column header.</param>
        public static void ApplySort(ICollectionView view, string propertyName, ListView listView, GridViewColumnHeader sortedColumnHeader)
        {
            ListSortDirection direction = ListSortDirection.Ascending;
            if (view.SortDescriptions.Count > 0)
            {
                SortDescription currentSort = view.SortDescriptions[0];
                if (currentSort.PropertyName == propertyName)
                {
                    if (currentSort.Direction == ListSortDirection.Ascending)
                    {
                        direction = ListSortDirection.Descending;
                    }
                    else
                    {
                        direction = ListSortDirection.Ascending;
                    }
                }
                view.SortDescriptions.Clear();

                GridViewColumnHeader currentSortedColumnHeader = GetSortedColumnHeader(listView);
                if (currentSortedColumnHeader != null)
                {
                    RemoveSortGlyph(currentSortedColumnHeader);
                }
            }
            if (!string.IsNullOrEmpty(propertyName))
            {
                view.SortDescriptions.Add(new SortDescription(propertyName, direction));
                if (GetShowSortGlyph(listView))
                    AddSortGlyph(
                        sortedColumnHeader,
                        direction,
                        direction == ListSortDirection.Ascending ? GetSortGlyphAscending(listView) : GetSortGlyphDescending(listView));
                SetSortedColumnHeader(listView, sortedColumnHeader);
            }
        }

        /// <summary>
        /// Adds the sort glyph.
        /// </summary>
        /// <param name="columnHeader">The column header.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="sortGlyph">The sort glyph.</param>
        private static void AddSortGlyph(GridViewColumnHeader columnHeader, ListSortDirection direction, ImageSource sortGlyph)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(columnHeader);
            adornerLayer.Add(
                new SortGlyphAdorner(
                    columnHeader,
                    direction,
                    sortGlyph
                ));
        }

        /// <summary>
        /// Removes the sort glyph.
        /// </summary>
        /// <param name="columnHeader">The column header.</param>
        private static void RemoveSortGlyph(GridViewColumnHeader columnHeader)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(columnHeader);
            Adorner[] adorners = adornerLayer.GetAdorners(columnHeader);
            if (adorners != null)
            {
                foreach (Adorner adorner in adorners)
                {
                    if (adorner is SortGlyphAdorner)
                    {
                        adornerLayer.Remove(adorner);
                    }
                }
            }
        }

        #endregion Helper methods

        #region SortGlyphAdorner nested class

        private class SortGlyphAdorner : Adorner
        {
            private GridViewColumnHeader _columnHeader;
            private ListSortDirection _direction;
            private ImageSource _sortGlyph;

            /// <summary>
            /// Initializes a new instance of the <see cref="SortGlyphAdorner"/> class.
            /// </summary>
            /// <param name="columnHeader">The column header.</param>
            /// <param name="direction">The direction.</param>
            /// <param name="sortGlyph">The sort glyph.</param>
            public SortGlyphAdorner(GridViewColumnHeader columnHeader, ListSortDirection direction, ImageSource sortGlyph)
                : base(columnHeader)
            {
                _columnHeader = columnHeader;
                _direction = direction;
                _sortGlyph = sortGlyph;
            }

            /// <summary>
            /// Gets the default glyph.
            /// </summary>
            /// <returns></returns>
            private Geometry GetDefaultGlyph()
            {
                double x1 = _columnHeader.ActualWidth - 13;
                double x2 = x1 + 10;
                double x3 = x1 + 5;
                double y1 = _columnHeader.ActualHeight / 2 - 3;
                double y2 = y1 + 5;

                if (_direction == ListSortDirection.Ascending)
                {
                    double tmp = y1;
                    y1 = y2;
                    y2 = tmp;
                }

                PathSegmentCollection pathSegmentCollection = new PathSegmentCollection();
                pathSegmentCollection.Add(new LineSegment(new Point(x2, y1), true));
                pathSegmentCollection.Add(new LineSegment(new Point(x3, y2), true));

                PathFigure pathFigure = new PathFigure(
                    new Point(x1, y1),
                    pathSegmentCollection,
                    true);

                PathFigureCollection pathFigureCollection = new PathFigureCollection();
                pathFigureCollection.Add(pathFigure);

                PathGeometry pathGeometry = new PathGeometry(pathFigureCollection);
                return pathGeometry;
            }

            /// <summary>
            /// When overridden in a derived class, participates in rendering operations that are directed by the layout system. The rendering instructions for this element are not used directly when this method is invoked, and are instead preserved for later asynchronous use by layout and drawing.
            /// </summary>
            /// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                if (_sortGlyph != null)
                {
                    double x = _columnHeader.ActualWidth - 13;
                    double y = _columnHeader.ActualHeight / 2 - 5;
                    Rect rect = new Rect(x, y, 10, 10);
                    drawingContext.DrawImage(_sortGlyph, rect);
                }
                else
                {
                    drawingContext.DrawGeometry(Brushes.LightGray, new Pen(Brushes.Gray, 1.0), GetDefaultGlyph());
                }
            }
        }

        #endregion SortGlyphAdorner nested class
    }
}