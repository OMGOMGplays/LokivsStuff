using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;

namespace LS 
{
	public partial class LSUI : HudEntity<RootPanel> 
	{
		public LSUI()  
		{
			if (!IsClient) return;

			RootPanel.AddChild<MainMenu>();
		}
	}
}
