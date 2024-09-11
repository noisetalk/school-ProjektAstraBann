using System;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using UnityEngine;

namespace SaveToolbox.Runtime.BasicSaveableMonoBehaviours
{
	/// <summary>
	/// Saves data about the RigidBody2D that is referenced. Saves all data such as velocities, constraints etc.
	/// </summary>
	[AddComponentMenu("SaveToolbox/SavingBehaviours/StbRigidBody2D")]
	public class StbRigidBody2D : SaveableMonoBehaviour
	{
		[SerializeField]
		private Rigidbody2D rigidBody2D;

		public override object Serialize()
		{
			if (rigidBody2D == null)
			{
				if (!TryGetComponent(out rigidBody2D)) throw new Exception($"Could not serialize object of type rigidBody2D as there isn't one referenced or attached to the game object.");
			}

			return new Rigidbody2DSaveData(rigidBody2D);
		}

		public override void Deserialize(object data)
		{
			if (rigidBody2D == null)
			{
				if (!TryGetComponent(out rigidBody2D)) throw new Exception($"Could not deserialize object of type rigidBody2D as there isn't one referenced or attached to the game object.");
			}

			var rigidBodyData = (Rigidbody2DSaveData)data;
			rigidBody2D.bodyType = (RigidbodyType2D)rigidBodyData.RigidBody2DBodyType;
			rigidBody2D.simulated = rigidBodyData.Simulated;
			rigidBody2D.useAutoMass = rigidBodyData.UseAutoMass;
			rigidBody2D.mass = rigidBodyData.Mass;
			rigidBody2D.drag = rigidBodyData.Drag;
			rigidBody2D.gravityScale = rigidBodyData.GravityScale;
			rigidBody2D.angularDrag = rigidBodyData.AngularDrag;
			rigidBody2D.isKinematic = rigidBodyData.IsKinematic;
			rigidBody2D.interpolation = (RigidbodyInterpolation2D)rigidBodyData.Interpolation;
			rigidBody2D.collisionDetectionMode = (CollisionDetectionMode2D)rigidBodyData.CollisionDetection;
#if STB_ABOVE_2022_2
			rigidBody2D.includeLayers = rigidBodyData.IncludeLayers;
			rigidBody2D.excludeLayers = rigidBodyData.ExcludeLayers;
#endif
			rigidBody2D.velocity = rigidBodyData.Velocity;
			rigidBody2D.angularVelocity = rigidBodyData.AngularVelocity;
			rigidBody2D.centerOfMass = rigidBodyData.CentreOfMass;
			rigidBody2D.constraints = (RigidbodyConstraints2D)rigidBodyData.RigidBodyConstraints;
		}
	}

	[Serializable]
	public struct Rigidbody2DSaveData
	{
		[SerializeField, StbSerialize]
		private int rigidBody2DBodyType;
		public int RigidBody2DBodyType => rigidBody2DBodyType;

		[SerializeField, StbSerialize]
		private bool simulated;
		public bool Simulated => simulated;

		[SerializeField, StbSerialize]
		private bool useAutoMass;
		public bool UseAutoMass => useAutoMass;

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
		private float gravityScale;
		public float GravityScale => gravityScale;

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
		private Vector2 velocity;
		public Vector2 Velocity => velocity;

		[SerializeField, StbSerialize]
		private float angularVelocity;
		public float AngularVelocity => angularVelocity;

		[SerializeField, StbSerialize]
		private Vector3 centreOfMass;
		public Vector3 CentreOfMass => centreOfMass;

		[SerializeField]
		private int rigidBodyConstraints;
		public int RigidBodyConstraints => rigidBodyConstraints;

		public Rigidbody2DSaveData(int rigidBody2DBodyType, bool simulated, bool useAutoMass, float mass, float drag, float angularDrag, float gravityScale, bool isKinematic, int interpolation, int collisionDetection, int includeLayers, int excludeLayers, Vector2 velocity, float angularVelocity, Vector3 centreOfMass, int rigidBodyConstraints)
		{
			this.rigidBody2DBodyType = rigidBody2DBodyType;
			this.simulated = simulated;
			this.useAutoMass = useAutoMass;
			this.mass = mass;
			this.drag = drag;
			this.angularDrag = angularDrag;
			this.gravityScale = gravityScale;
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

		public Rigidbody2DSaveData(Rigidbody2D rigidbody)
		{
			rigidBody2DBodyType = (int)rigidbody.bodyType;
			simulated = rigidbody.simulated;
			useAutoMass = rigidbody.useAutoMass;
			mass = rigidbody.mass;
			drag = rigidbody.drag;
			angularDrag = rigidbody.angularDrag;
			gravityScale = rigidbody.gravityScale;
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