using SaveToolbox.Runtime.BasicSaveableMonoBehaviours;
using UnityEngine;

namespace SaveToolbox.Runtime.CustomComponentSavers
{
	public class RigidbodyComponentSaver : AbstractComponentSaver<Rigidbody>
	{
		public override object Serialize()
		{
			return new RigidbodySaveData(Target);
		}

		public override void Deserialize(object data)
		{
			var rigidBodyData = (RigidbodySaveData)data;
			Target.mass = rigidBodyData.Mass;
			Target.drag = rigidBodyData.Drag;
			Target.angularDrag = rigidBodyData.AngularDrag;
#if STB_ABOVE_2022_2
			Target.automaticCenterOfMass = rigidBodyData.AutomaticCenterOfMass;
			Target.automaticInertiaTensor = rigidBodyData.AutomaticTensor;
#endif
			Target.useGravity = rigidBodyData.UseGravity;
			Target.isKinematic = rigidBodyData.IsKinematic;
			Target.interpolation = (RigidbodyInterpolation)rigidBodyData.Interpolation;
			Target.collisionDetectionMode = (CollisionDetectionMode)rigidBodyData.CollisionDetection;
#if STB_ABOVE_2022_2
			Target.includeLayers = rigidBodyData.IncludeLayers;
			Target.excludeLayers = rigidBodyData.ExcludeLayers;
#endif
			Target.velocity = rigidBodyData.Velocity;
			Target.angularVelocity = rigidBodyData.AngularVelocity;
			Target.centerOfMass = rigidBodyData.CentreOfMass;
			Target.constraints = (RigidbodyConstraints)rigidBodyData.RigidBodyConstraints;
		}
	}
}