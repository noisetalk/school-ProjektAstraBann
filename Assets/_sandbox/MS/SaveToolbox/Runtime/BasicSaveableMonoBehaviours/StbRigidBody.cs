using System;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using UnityEngine;

namespace SaveToolbox.Runtime.BasicSaveableMonoBehaviours
{
	/// <summary>
	/// Saves data about the RigidBody that is referenced. Saves all data such as velocities, constraints etc.
	/// </summary>
	[AddComponentMenu("SaveToolbox/SavingBehaviours/StbRigidBody")]
	public class StbRigidBody : SaveableMonoBehaviour
	{
		[SerializeField]
		private Rigidbody rigidBody;

		public override object Serialize()
		{
			if (rigidBody == null)
			{
				if (!TryGetComponent(out rigidBody)) throw new Exception($"Could not serialize object of type rigidBody as there isn't one referenced or attached to the game object.");
			}
			return new RigidbodySaveData(rigidBody);
		}

		public override void Deserialize(object data)
		{
			if (rigidBody == null)
			{
				if (!TryGetComponent(out rigidBody)) throw new Exception($"Could not deserialize object of type rigidBody as there isn't one referenced or attached to the game object.");
			}
			var rigidBodyData = (RigidbodySaveData)data;
			rigidBody.mass = rigidBodyData.Mass;
			rigidBody.drag = rigidBodyData.Drag;
			rigidBody.angularDrag = rigidBodyData.AngularDrag;
#if STB_ABOVE_2022_2
			rigidBody.automaticCenterOfMass = rigidBodyData.AutomaticCenterOfMass;
			rigidBody.automaticInertiaTensor = rigidBodyData.AutomaticTensor;
#endif
			rigidBody.useGravity = rigidBodyData.UseGravity;
			rigidBody.isKinematic = rigidBodyData.IsKinematic;
			rigidBody.interpolation = (RigidbodyInterpolation)rigidBodyData.Interpolation;
			rigidBody.collisionDetectionMode = (CollisionDetectionMode)rigidBodyData.CollisionDetection;
#if STB_ABOVE_2022_2
			rigidBody.includeLayers = rigidBodyData.IncludeLayers;
			rigidBody.excludeLayers = rigidBodyData.ExcludeLayers;
#endif
			rigidBody.velocity = rigidBodyData.Velocity;
			rigidBody.angularVelocity = rigidBodyData.AngularVelocity;
			rigidBody.centerOfMass = rigidBodyData.CentreOfMass;
			rigidBody.constraints = (RigidbodyConstraints)rigidBodyData.RigidBodyConstraints;
		}
	}

	[Serializable]
	public struct RigidbodySaveData
	{
		[SerializeField, StbSerialize]
		private float mass;
		public float Mass => mass;

		[SerializeField, StbSerialize]
		private float drag;
		public float Drag => drag;

		[SerializeField, StbSerialize]
		private float angularDrag;
		public float AngularDrag => angularDrag;

		[SerializeField, StbSerialize]
		private bool automaticCenterOfMass;
		public bool AutomaticCenterOfMass => automaticCenterOfMass;

		[SerializeField, StbSerialize]
		private bool automaticTensor;
		public bool AutomaticTensor => automaticTensor;

		[SerializeField, StbSerialize]
		private bool useGravity;
		public bool UseGravity => useGravity;

		[SerializeField, StbSerialize]
		private bool isKinematic;
		public bool IsKinematic => isKinematic;

		[SerializeField, StbSerialize]
		private int interpolation;
		public int Interpolation => interpolation;

		[SerializeField, StbSerialize]
		private int collisionDetection;
		public int CollisionDetection => collisionDetection;

		[SerializeField, StbSerialize]
		private int includeLayers;
		public int IncludeLayers => includeLayers;

		[SerializeField, StbSerialize]
		private int excludeLayers;
		public int ExcludeLayers => excludeLayers;

		[SerializeField, StbSerialize]
		private Vector3 velocity;
		public Vector3 Velocity => velocity;

		[SerializeField, StbSerialize]
		private Vector3 angularVelocity;
		public Vector3 AngularVelocity => angularVelocity;

		[SerializeField, StbSerialize]
		private Vector3 centreOfMass;
		public Vector3 CentreOfMass => centreOfMass;

		[SerializeField, StbSerialize]
		private int rigidBodyConstraints;
		public int RigidBodyConstraints => rigidBodyConstraints;

		public RigidbodySaveData(float mass, float drag, float angularDrag, bool automaticCenterOfMass, bool automaticTensor, bool useGravity, bool isKinematic, int interpolation, int collisionDetection, int includeLayers, int excludeLayers, Vector3 velocity, Vector3 angularVelocity, Vector3 centreOfMass, int rigidBodyConstraints)
		{
			this.mass = mass;
			this.drag = drag;
			this.angularDrag = angularDrag;
			this.automaticCenterOfMass = automaticCenterOfMass;
			this.automaticTensor = automaticTensor;
			this.useGravity = useGravity;
			this.isKinematic = isKinematic;
			this.interpolation = interpolation;
			this.collisionDetection = collisionDetection;
			this.includeLayers = includeLayers;
			this.excludeLayers = excludeLayers;
			this.velocity = velocity;
			this.angularVelocity = angularVelocity;
			this.centreOfMass = centreOfMass;
			this.rigidBodyConstraints = rigidBodyConstraints;
		}

		public RigidbodySaveData(Rigidbody rigidbody)
		{
			mass = rigidbody.mass;
			drag = rigidbody.drag;
			angularDrag = rigidbody.angularDrag;
#if STB_ABOVE_2022_2
			automaticCenterOfMass = rigidbody.automaticCenterOfMass;
			automaticTensor = rigidbody.automaticInertiaTensor;
#else
			automaticCenterOfMass = default;
			automaticTensor = default;
#endif
			useGravity = rigidbody.useGravity;
			isKinematic = rigidbody.isKinematic;
			interpolation = (int)rigidbody.interpolation;
			collisionDetection = (int)rigidbody.collisionDetectionMode;
#if STB_ABOVE_2022_2
			includeLayers = rigidbody.includeLayers;
			excludeLayers = rigidbody.excludeLayers;
#else
			includeLayers = default;
			excludeLayers = default;
#endif
			velocity = rigidbody.velocity;
			angularVelocity = rigidbody.angularVelocity;
			centreOfMass = rigidbody.centerOfMass;
			rigidBodyConstraints = (int)rigidbody.constraints;
		}
	}
}