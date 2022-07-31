using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;

namespace LS 
{
	public partial class MainMenu : Panel 
	{
		public static MainMenu Instance;

		public MainMenu() 
		{
			Instance = this;

			StyleSheet.Load("/ui/Menus/MainMenu.scss");

			var left = Add.Panel("left");
			{
				var tabs = left.AddChild<ButtonGroup>();
				tabs.AddClass("tabs");

				var body = left.Add.Panel("body");

				{
					var models = body.AddChild<ModelList>();
					tabs.SelectedButton = tabs.AddButtonActive("Playermodels", (b) => models.SetClass("active", b));

					var weapons = body.AddChild<WeaponList>();
					tabs.AddButtonActive("Weapons", (b) => weapons.SetClass("active", b));
				}
			}
		}

		public override void Tick()
		{
			base.Tick();

			Parent.SetClass("mainmenuopen", Input.Down(InputButton.Menu));
		}
	}
}
