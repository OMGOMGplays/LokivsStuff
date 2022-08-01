using Sandbox;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LS
{
	public partial class LSGame : Game 
	{
		public LSGame() 
		{
			if (IsServer) 
			{
				_ = new LSUI();
			}
		}

		public override void ClientJoined( Client cl )
		{
			base.ClientJoined( cl );

			var pawn = new LSPlayer(cl);
			pawn.Respawn();

			cl.Pawn = pawn;
		}

		[ConCmd.Server("changepmto")]
		public static void ChangePMTo(string pmname) 
		{
			var owner = ConsoleSystem.Caller?.Pawn;

			if (ConsoleSystem.Caller == null) return;

			(owner as LSPlayer).CurrModel = pmname;
		}

		[ConCmd.Server("ls_setpmtocitizen")] // Best way to reliably (and quickly (through code)) switch back to the citizen pm - Lokiv
		public static void SetPmCitizen() 
		{
			var owner = ConsoleSystem.Caller?.Pawn;

			if (ConsoleSystem.Caller == null) return;

			var lsOwner = owner as LSPlayer;

			if (lsOwner.CurrModel == "models/citizen/citizen.vmdl") 
			{
				Log.Error($"Player '{ConsoleSystem.Caller.Name}' is already the citizen playermodel!");
				
				return;
			}

			(owner as LSPlayer).CurrModel = "models/citizen/citizen.vmdl";

			Log.Info("Returning to the citizen playermodel!");
		}

		[ConCmd.Server("spawn")]
		public static async Task Spawn(string modelname) 
		{
			var owner = ConsoleSystem.Caller?.Pawn;

			if (ConsoleSystem.Caller == null) return;

			var tr = Trace.Ray(owner.EyePosition, owner.EyePosition + owner.EyeRotation.Forward * 500)
						.UseHitboxes()
						.Ignore(owner)
						.Run();

			var modelRotation = Rotation.From(new Angles(0, owner.EyeRotation.Angles().yaw, 0));

			if (modelname.Count(x => x == '.') == 1 && !modelname.EndsWith(".vmdl", System.StringComparison.OrdinalIgnoreCase) && !modelname.EndsWith(".vmdl_c", System.StringComparison.OrdinalIgnoreCase)) 
			{
				modelname = await SpawnPackageModel(modelname, tr.EndPosition, modelRotation, owner);
				if (modelname == null) return;
			}

			var model = Model.Load(modelname);
			if (model == null || model.IsError) return;

			var ent = new Prop 
			{
				Position = tr.EndPosition + Vector3.Down * model.PhysicsBounds.Mins.z,
				Rotation = modelRotation,
				Model = model
			};

			ent.SetupPhysicsFromModel(PhysicsMotionType.Dynamic);

			if (!ent.PhysicsBody.IsValid()) 
			{
				ent.SetupPhysicsFromOBB(PhysicsMotionType.Dynamic, ent.CollisionBounds.Mins, ent.CollisionBounds.Maxs);
			}
		}

		static async Task<string> SpawnPackageModel(string packageName, Vector3 pos, Rotation rotation, Entity source) 
		{
			var package = await Package.Fetch(packageName, false);
			if (package == null || package.PackageType != Package.Type.Model || package.Revision == null) 
			{
				return null;
			}

			if (!source.IsValid()) return null;

			var model = package.GetMeta("PrimaryAsset", "models/dev/error.vmdl");
			var mins = package.GetMeta("RenderMins", Vector3.Zero);
			var maxs = package.GetMeta("RenderMaxs", Vector3.Zero);

			await package.MountAsync();

			return model;
		}

		[ConCmd.Server("spawn_entity")]
		public static void SpawnEntity(string entName) 
		{
			var owner = ConsoleSystem.Caller.Pawn as Player;

			if (owner == null) return;

			var entityType = TypeLibrary.GetTypeByName<Entity>(entName);

			if (entityType == null) 
			{
				if (!TypeLibrary.Has<SpawnableAttribute>(entityType)) return;
			}

			var tr = Trace.Ray(owner.EyePosition, owner.EyePosition + owner.EyeRotation.Forward * 200)
						.UseHitboxes()
						.Ignore(owner)
						.Size(2)
						.Run();

			var ent = TypeLibrary.Create<Entity>(entityType);
			if (ent is BaseCarriable && owner.Inventory != null) 
			{
				if (owner.Inventory.Add(ent, true)) return;
			}

			ent.Position = tr.EndPosition;
			ent.Rotation = Rotation.From(new Angles(0, owner.EyeRotation.Angles().yaw, 0));
		}
	}
}
