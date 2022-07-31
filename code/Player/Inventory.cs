using Sandbox;
using System;
using System.Linq;

namespace LS 
{
	partial class Inventory : BaseInventory 
	{
		public Inventory(Player player) : base(player) 
		{

		}

		public override bool CanAdd( Entity ent )
		{
			if (!ent.IsValid()) return false;

			if (!base.CanAdd(ent)) return false;

			return !IsCarryingType(ent.GetType());
		}

		public override bool Add( Entity ent, bool makeActive = false )
		{
			if (!ent.IsValid()) return false;

			if (IsCarryingType(ent.GetType())) return false;

			return base.Add(ent, makeActive);
		}

		public bool IsCarryingType(Type t) 
		{
			return List.Any(x => x?.GetType() == t);
		}

		public override bool Drop( Entity ent )
		{
			if (!Host.IsServer) return false;

			if (!Contains(ent)) return false;

			if (ent is BaseCarriable bc) 
			{
				bc.OnCarryDrop(Owner);
			}

			return ent.Parent == null;
		}
	}
}
