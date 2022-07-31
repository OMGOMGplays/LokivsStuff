using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Tests;

namespace LS 
{
	public partial class ModelList : Panel 
	{
		VirtualScrollPanel Canvas;

		public ModelList() 
		{
			AddClass("spawnpage");
			AddChild(out Canvas, "canvas");

			Canvas.Layout.AutoColumns = true;
			Canvas.Layout.ItemWidth = 100;
			Canvas.Layout.ItemHeight = 100;

			Canvas.OnCreateCell = (cell, data) => 
			{
				var player = Local.Pawn as LSPlayer;

				var file = (string)data;
				var panel = cell.Add.Panel("icon");
				panel.AddEventListener("onclick", () => ConsoleSystem.Run($"changepmto models/playermodels/{file}"));
				panel.AddEventListener("onclick", () => Log.Info($"Model should now be set to 'models/playermodels/{file}'"));

				panel.Style.BackgroundImage = Texture.Load(FileSystem.Mounted, $"/models/playermodels/{file}_c.png", false);
			};

			foreach (var file in FileSystem.Mounted.FindFile("models/playermodels", "*.vmdl_c.png", true)) 
			{
				if (string.IsNullOrWhiteSpace(file)) continue;
				if (file.Contains("_lod0")) continue;
				if (file.Contains("clothes")) continue;

				Canvas.AddItem(file.Remove(file.Length - 6));
			}
		}
	}
}
