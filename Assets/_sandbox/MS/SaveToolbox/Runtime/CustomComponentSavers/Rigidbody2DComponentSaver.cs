using SaveToolbox.Runtime.BasicSaveableMonoBehaviours;
using UnityEngine;

namespace SaveToolbox.Runtime.CustomComponentSavers
{
	public class Rigidbody2DComponentSaver : AbstractComponentSaver<Rigidbody2D>
	{
		public override object Serialize()
		{
			return new Rigidbody2DSaveData(Target);
		}

		public override void Deserialize(object data)
		{
			var rigidBodyData = (Rigidbody2DSaveData)data;
			Target.bodyType = (RigidbodyType2D)rigidBodyData.RigidBody2DBodyType;
			Target.simulated = rigidBodyData.Simulated;
			Target.useAutoMass = rigidBodyData.UseAutoMass;
			Target.mass = rigidBodyData.Mass;
			Target.drag = rigidBodyData.Drag;
			Target.angularDrag = rigidBodyData.AngularDrag;
			Target.isKinematic = rigidBodyData.IsKinematic;
			Target.interpolation = (RigidbodyInterpolation2D)rigidBodyData.Interpolation;
			Target.collisionDetectionMode = (CollisionDetectionMode2D)rigidBodyData.CollisionDetection;
#if STB_ABOVE_2022_2
			Target.includeLayers = rigidBodyData.IncludeLayers;
			Target.excludeLayers = rigidBodyData.ExcludeLayers;
#endif
			Target.velocity = rigidBodyData.Velocity;
			Target.angularVelocity = rigidBodyData.AngularVelocity;
			Target.centerOfMass = rigidBodyData.CentreOfMass;
			Target.constraints = (RigidbodyConstraints2D)rigidBodyData.RigidBodyConstraints;
		}
	}
}