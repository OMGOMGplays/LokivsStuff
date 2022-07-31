using Sandbox;

namespace LS 
{
	public class ViewModel : BaseViewModel 
	{
		protected float SwingInfluence => 0.05f;
		protected float ReturnSpeed => 5.0f;
		protected float MaxOffsetLength => 10.0f;
		protected float BobCycleTime => 7;

		protected Vector3 BobDirection => new Vector3(0.0f, 1.0f, 0.5f);
		private Vector3 SwingOffset;

		private float LastPitch;
		private float LastYaw;
		private float BobAnim;

		private bool Activated = false;

		public bool EnableSwingAndBob = true;

		public float YawInertia {get; private set;}
		public float PitchInertia {get; private set;}

		public override void PostCameraSetup( ref CameraSetup camSetup )
		{
			base.PostCameraSetup( ref camSetup );

			if ( !Local.Pawn.IsValid() )
				return;

			if ( !Activated )
			{
				LastPitch = camSetup.Rotation.Pitch();
				LastYaw = camSetup.Rotation.Yaw();

				YawInertia = 0;
				PitchInertia = 0;

				Activated = true;
			}

			Position = camSetup.Position;
			Rotation = camSetup.Rotation;

			var cameraBoneIndex = GetBoneIndex( "camera" );
			if ( cameraBoneIndex != -1 )
			{
				camSetup.Rotation *= (Rotation.Inverse * GetBoneTransform( cameraBoneIndex ).Rotation);
			}

			var newPitch = Rotation.Pitch();
			var newYaw = Rotation.Yaw();

			PitchInertia = Angles.NormalizeAngle( newPitch - LastPitch );
			YawInertia = Angles.NormalizeAngle( LastYaw - newYaw );

			if ( EnableSwingAndBob )
			{
				var playerVelocity = Local.Pawn.Velocity;

				if ( Local.Pawn is Player player )
				{
					var controller = player.GetActiveController();
					if ( controller != null && controller.HasTag( "noclip" ) )
					{
						playerVelocity = Vector3.Zero;
					}
				}

				var verticalDelta = playerVelocity.z * Time.Delta;
				var viewDown = Rotation.FromPitch( newPitch ).Up * -1.0f;
				verticalDelta *= (1.0f - System.MathF.Abs( viewDown.Cross( Vector3.Down ).y ));
				var pitchDelta = PitchInertia - verticalDelta * 1;
				var yawDelta = YawInertia;

				var offset = CalcSwingOffset( pitchDelta, yawDelta );
				offset += CalcBobbingOffset( playerVelocity );

				Position += Rotation * offset;
			}
			else
			{
				SetAnimParameter( "aim_yaw_inertia", YawInertia );
				SetAnimParameter( "aim_pitch_inertia", PitchInertia );
			}

			LastPitch = newPitch;
			LastYaw = newYaw;
		}

		protected Vector3 CalcSwingOffset( float pitchDelta, float yawDelta )
		{
			Vector3 swingVelocity = new Vector3( 0, yawDelta, pitchDelta );

			SwingOffset -= SwingOffset * ReturnSpeed * Time.Delta;
			SwingOffset += (swingVelocity * SwingInfluence);

			if ( SwingOffset.Length > MaxOffsetLength )
			{
				SwingOffset = SwingOffset.Normal * MaxOffsetLength;
			}

			return SwingOffset;
		}

		protected Vector3 CalcBobbingOffset( Vector3 velocity )
		{
			BobAnim += Time.Delta * BobCycleTime;

			var twoPI = System.MathF.PI * 2.0f;

			if ( BobAnim > twoPI )
			{
				BobAnim -= twoPI;
			}

			var speed = new Vector2( velocity.x, velocity.y ).Length;
			speed = speed > 10.0 ? speed : 0.0f;
			var offset = BobDirection * (speed * 0.005f) * System.MathF.Cos( BobAnim );
			offset = offset.WithZ( -System.MathF.Abs( offset.z ) );

			return offset;
		}
	}
}
