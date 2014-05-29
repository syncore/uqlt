using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace UQLT.ViewModels
{
	/// <summary>
	/// ViewModel representing a ChatMessageView
	/// </summary>
	public class ChatMessageViewModel : PropertyChangedBase, IHaveDisplayName
	{

		private string _displayName = "Chatting with";

		public string DisplayName
		{
			get
			{
				return _displayName;
			}
			set
			{
				_displayName = value;
			}
		}


		private string _chattingWithTitle;

		public string ChattingWithTitle
		{
			get
			{
				return _chattingWithTitle;
			}
			set
			{
				_chattingWithTitle = value;
				NotifyOfPropertyChange(() => ChattingWithTitle);
			}
		}

		private string _fromMessage;
		public string FromMessage
		{
			get
			{
				return _fromMessage;
			}
			set
			{
				_fromMessage = value;
				NotifyOfPropertyChange(() => FromMessage);
			}
		}

		private string _toMessage;
		public string ToMessage
		{
			get
			{
				return _toMessage;
			}
			set
			{
				_toMessage = value;
				NotifyOfPropertyChange(() => ToMessage);
			}
		}

		[ImportingConstructor]
		public ChatMessageViewModel()
		{
		}


		public void SendMessage(string message)
		{
			MessageBox.Show(message);
			ToMessage = string.Empty;
		}
	}
}
