using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UQLT.Interfaces
{
	/// <summary>
	/// Necessary methods for saving various configuration options.
	/// </summary>
	interface IUQLTConfiguration
	{
		bool ConfigExists();
		void SaveConfig();
		void LoadConfig();
		void LoadDefaultConfig();
	}
}