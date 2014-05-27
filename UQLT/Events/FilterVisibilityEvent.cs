using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Events
{

	/// <summary>
	/// Event: User decides to hide or display the filter menu
	/// </summary>
	public class FilterVisibilityEvent
	{

		public FilterVisibilityEvent(bool visibility)
		{
			FilterViewVisibility = visibility;
		}

		public bool FilterViewVisibility
		{
			get;
			set;
		}
	}
}