using Sandbox;

namespace LS 
{
	public partial class LSPlayer : Player 
	{
		public ClothingContainer Clothing = new();

		[Net] public string CurrModel {get; set;} = "models/citizen/citizen.vmdl";

		private TimeSince TimeSinceDropped = 0;

		private DamageInfo LastDamage;

		public LSPlayer() 
		{
			Inventory = new Inventory(this);
		}

		public LSPlayer(Client cl) : this() 
		{
			Clothing.LoadFromClient(cl);
		}

		public override void Respawn()
		{
			base.Respawn();

			SetModel(CurrModel);

			Log.Info("The player's current model is: " + CurrModel);

			if (CurrModel == "models/citizen/citizen.vmdl")
			{
				Clothing.DressEntity(this);
			}
			else if (CurrModel != "models/citizen/citizen.vmdl") 
			{
				Clothing.ClearEntities();
			}

			Controller = new WalkController();
			Animator = new StandardPlayerAnimator();
			CameraMode = new ThirdPersonCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			if (LifeState != LifeState.Alive) return;

			SimulateActiveChild(cl, ActiveChild);

			if (Input.ActiveChild != null) 
			{
				ActiveChild = Input.ActiveChild;
			}

			if (Input.Pressed(InputButton.Drop)) 
			{
				var dropped = Inventory.DropActive();
				if (dropped != null) 
				{
					dropped.PhysicsGroup.ApplyImpulse(Velocity + EyeRotation.Forward * 500.0f + Vector3.Up * 100.0f, true);
					dropped.PhysicsGroup.ApplyAngularImpulse(Vector3.Random * 100.0f, true);


					TimeSinceDropped = 0;
				}
			}

			TickPlayerUse();
		}

		public override void OnKilled()
		{
			base.OnKilled();

			EnableAllCollisions = false;
			EnableDrawing = false;

			BecomeRagdollOnClient(Velocity, LastDamage.Flags, LastDamage.Position, LastDamage.Force, GetHitboxBone(LastDamage.HitboxIndex));

			Controller = null;

			CameraMode = new SpectateRagdollCamera();

			foreach (var child in Children) 
			{
				child.EnableDrawing = false;
			}

			Inventory.DropActive();
			Inventory.DeleteContents();
		}
	}
}
