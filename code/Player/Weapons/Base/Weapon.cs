using Sandbox;
using System.Collections.Generic;

namespace LS 
{
	public partial class Weapon : BaseWeapon, IUse 
	{
		public virtual float ReloadTime => 3.0f;

		public PickupTrigger PickupTrigger {get; protected set;}

		[Net, Predicted] public TimeSince TimeSinceReload {get; set;}

		[Net, Predicted] public bool IsReloading {get; set;}

		[Net, Predicted] public TimeSince TimeSinceDeployed {get; set;}

		public override void Spawn()
		{
			base.Spawn();

			PickupTrigger = new PickupTrigger 
			{
				Parent = this,
				Position = Position,
				EnableTouch = true,
				EnableSelfCollisions = false
			};

			PickupTrigger.PhysicsBody.AutoSleep = false;
		}

		public override void ActiveStart( Entity ent )
		{
			base.ActiveStart( ent );

			TimeSinceDeployed = 0;
		}

		public override void Reload()
		{
			if (IsReloading) return;

			TimeSinceReload = 0;
			IsReloading = true;

			(Owner as AnimatedEntity)?.SetAnimParameter("b_reload", true);

			StartReloadEffects();
		}

		public override void Simulate( Client player )
		{
			if (TimeSinceDeployed < 0.6f) return;

			if (!IsReloading) 
			{
				base.Simulate(player);
			}

			if (IsReloading && TimeSinceReload >= ReloadTime) 
			{
				OnReloadFinish();
			}
		}

		public virtual void OnReloadFinish() 
		{
			IsReloading = false;
		}

		[ClientRpc]
		public virtual void StartReloadEffects() 
		{
			ViewModelEntity?.SetAnimParameter("reload", true);
		}

		public override void CreateViewModel()
		{
			Host.AssertClient();

			if (string.IsNullOrEmpty(ViewModelPath)) return;

			ViewModelEntity = new ViewModel 
			{
				Position = Position,
				Owner = Owner,
				EnableViewmodelRendering = true
			};
	
			ViewModelEntity.SetModel(ViewModelPath);
		}

		public bool OnUse(Entity user) 
		{
			if (Owner != null) return false;

			if (!user.IsValid()) return false;

			user.StartTouch(this);

			return false;
		}

		public virtual bool IsUsable(Entity user) 
		{
			var player = user as Player;
			if (Owner != null) return false;

			if (player.Inventory is Inventory inventory) 
			{
				return inventory.CanAdd(this);
			}

			return true;
		}

		public void Remove() 
		{
			Delete();
		}

		[ClientRpc]
		protected virtual void ShootEffects() 
		{
			Host.AssertClient();

			Particles.Create("particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle");

			ViewModelEntity?.SetAnimParameter("fire", true);
		}

		public override IEnumerable<TraceResult> TraceBullet(Vector3 start, Vector3 end, float radius = 2.0f) 
		{
			bool underWater = Trace.TestPoint(start, "water");

			var trace = Trace.Ray(start, end)
							.UseHitboxes()
							.WithAnyTags("solid", "player", "npc", "glass")
							.Ignore(this)
							.Size(radius);

			if (!underWater) trace = trace.WithAnyTags("water");

			var tr = trace.Run();

			if (tr.Hit) 
			{
				yield return tr;
			}
			else 
			{
				trace = trace.Size(radius);

				tr = trace.Run();

				if (tr.Hit) 
				{
					yield return tr;
				}
			}
		}

		public IEnumerable<TraceResult> TraceMelee(Vector3 start, Vector3 end, float radius = 2.0f) 
		{
			var trace = Trace.Ray(start, end)
							.UseHitboxes()
							.WithAnyTags("solid", "player", "npc", "glass")
							.Ignore(this);

			var tr = trace.Run();

			if (tr.Hit) 
			{
				yield return tr;
			}
			else 
			{
				trace = trace.Size(radius);

				tr = trace.Run();

				if (tr.Hit) 
				{
					yield return tr;
				}
			}
		}

		public virtual void ShootBullet(Vector3 pos, Vector3 dir, float spread, float force, float damage, float bullSize) 
		{
			var forward = dir;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			forward = forward.Normal;

			foreach (var tr in TraceBullet(pos, pos + forward * 9999, bullSize)) 
			{
				tr.Surface.DoBulletImpact(tr);

				if (!IsServer) continue;
				if (!tr.Entity.IsValid()) continue;

				using (Prediction.Off()) 
				{
					var damageInfo = DamageInfo.FromBullet(tr.EndPosition, forward * 100 * force, damage)
							.UsingTraceResult(tr)
							.WithAttacker(Owner)
							.WithWeapon(this);

					tr.Entity.TakeDamage(damageInfo);
				}
			}
		}
	
		public virtual void ShootBullet(float spread, float force, float damage, float bullSize) 
		{
			Rand.SetSeed(Time.Tick);
			ShootBullet(Owner.EyePosition, Owner.EyeRotation.Forward, spread, force, damage, bullSize);
		}

		public virtual void ShootBullets(int numBullets, float spread, float force, float damage, float bullSize) 
		{
			var pos = Owner.EyePosition;
			var dir = Owner.EyeRotation.Forward;

			for (int i = 0; i < numBullets; i++) 
			{
				ShootBullet(pos, dir, spread, force / numBullets, damage, bullSize);
			}
		}
	}
}
