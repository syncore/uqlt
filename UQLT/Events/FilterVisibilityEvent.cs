namespace UQLT.Events
{
    /// <summary>
    /// Event: User decides to hide or display the filter menu
    /// </summary>
    public class FilterVisibilityEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterVisibilityEvent" /> class.
        /// </summary>
        /// <param name="visibility">if set to <c>true</c> then the filter view is visible.</param>
        public FilterVisibilityEvent(bool visibility)
        {
            FilterViewVisibility = visibility;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the filter view is visible.
        /// </summary>
        /// <value><c>true</c> if filter view is visible otherwise, <c>false</c>.</value>
        public bool FilterViewVisibility
        {
            get;
            set;
        }
    }
}