using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Tests;

namespace LS 
{
	[Library]
	public partial class PropList : Panel 
	{
		VirtualScrollPanel Canvas;

		public PropList() 
		{
			AddClass("spawnpage");
			AddChild(out Canvas, "canvas");

			Canvas.Layout.AutoColumns = true;
			Canvas.Layout.ItemWidth = 100;
			Canvas.Layout.ItemHeight = 100;

			Canvas.OnCreateCell = (cell, data) => 
			{
				var file = (string)data;
				var panel = cell.Add.Panel("icon");
				panel.AddEventListener("onclick", () => ConsoleSystem.Run("spawn", "models/props/" + file));
				panel.Style.BackgroundImage = Texture.Load(FileSystem.Mounted, $"/models/props/{file}_c.png", false);
			};

			foreach (var file in FileSystem.Mounted.FindFile("models/props", "*.vmdl_c.png", true)) 
			{
				if (string.IsNullOrWhiteSpace(file)) continue;
				if (file.Contains("_lod0")) continue;
				if (file.Contains("clothes")) continue;

				Canvas.AddItem(file.Remove(file.Length - 6));
			}
		}
	}
}
