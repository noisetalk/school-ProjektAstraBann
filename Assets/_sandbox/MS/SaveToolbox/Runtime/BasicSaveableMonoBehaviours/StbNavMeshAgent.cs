using System;
using SaveToolbox.Runtime.Attributes;
using SaveToolbox.Runtime.Core.MonoBehaviours;
using UnityEngine;
using UnityEngine.AI;

namespace SaveToolbox.Runtime.BasicSaveableMonoBehaviours
{
	/// <summary>
	/// Saves data about the nav mesh agent that is referenced.
	/// </summary>
	[AddComponentMenu("SaveToolbox/SavingBehaviours/StbNavMeshAgent")]
	public class StbNavMeshAgent : SaveableMonoBehaviour
	{
		/// <summary>
		/// The referenced nav mesh agent.
		/// </summary>
		[SerializeField]
		private NavMeshAgent navMeshAgent;

		public override object Serialize()
		{
			if (navMeshAgent == null)
			{
				if (!TryGetComponent(out navMeshAgent)) throw new Exception($"Could not serialize object of type navMeshAgent as there isn't one referenced or attached to the game object.");
			}
			return new NavMeshAgentSaveData(navMeshAgent);
		}

		public override void Deserialize(object data)
		{
			if (navMeshAgent == null)
			{
				if (!TryGetComponent(out navMeshAgent)) throw new Exception($"Could not deserialize object of type navMeshAgent as there isn't one referenced or attached to the game object.");
			}

			var navMeshAgentData = (NavMeshAgentSaveData)data;

			navMeshAgent.acceleration = navMeshAgentData.Acceleration;
			navMeshAgent.destination = navMeshAgentData.Destination;
			navMeshAgent.height = navMeshAgentData.Height;
			navMeshAgent.radius = navMeshAgentData.Radius;
			navMeshAgent.speed = navMeshAgentData.Speed;
			navMeshAgent.velocity = navMeshAgentData.Velocity;
			navMeshAgent.angularSpeed = navMeshAgentData.AngularSpeed;
			navMeshAgent.areaMask = navMeshAgentData.AreaMask;
			navMeshAgent.autoBraking = navMeshAgentData.AutoBraking;
			navMeshAgent.autoRepath = navMeshAgentData.AutoRepath;
			navMeshAgent.avoidancePriority = navMeshAgentData.AvoidancePriority;
			navMeshAgent.baseOffset = navMeshAgentData.BaseOffset;
			navMeshAgent.isStopped = navMeshAgentData.IsStopped;
			navMeshAgent.nextPosition = navMeshAgentData.NextPosition;
			navMeshAgent.stoppingDistance = navMeshAgentData.StoppingDistance;
			navMeshAgent.updatePosition = navMeshAgentData.UpdatePosition;
			navMeshAgent.updateRotation = navMeshAgentData.UpdateRotation;
		}
	}

	[Serializable]
	public struct NavMeshAgentSaveData
	{
		[SerializeField, StbSerialize]
		private float acceleration;
		public float Acceleration => acceleration;

		[SerializeField, StbSerialize]
		private Vector3 destination;
		public Vector3 Destination => destination;

		[SerializeField, StbSerialize]
		private float height;
		public float Height => height;

		[SerializeField, StbSerialize]
		private float radius;
		public float Radius => radius;

		[SerializeField, StbSerialize]
		private float speed;
		public float Speed => speed;

		[SerializeField]
		private Vector3 velocity;
		public Vector3 Velocity => velocity;

		[SerializeField]
		private float angularSpeed;
		public float AngularSpeed => angularSpeed;

		[SerializeField]
		private int areaMask;
		public int AreaMask => areaMask;

		[SerializeField]
		private bool autoBraking;
		public bool AutoBraking => autoBraking;

		[SerializeField]
		private bool autoRepath;
		public bool AutoRepath => autoRepath;

		[SerializeField]
		private int avoidancePriority;
		public int AvoidancePriority => avoidancePriority;

		[SerializeField]
		private float baseOffset;
		public float BaseOffset => baseOffset;

		[SerializeField]
		private bool isStopped;
		public bool IsStopped => isStopped;

		[SerializeField]
		private Vector3 nextPosition;
		public Vector3 NextPosition => nextPosition;

		[SerializeField]
		private float stoppingDistance;
		public float StoppingDistance => stoppingDistance;

		[SerializeField]
		private bool updatePosition;
		public bool UpdatePosition => updatePosition;

		[SerializeField]
		private bool updateRotation;
		public bool UpdateRotation => updateRotation;

		public NavMeshAgentSaveData(float acceleration, Vector3 destination, float height, float radius, float speed, Vector3 velocity, float angularSpeed, int areaMask, bool autoBraking, bool autoRepath, int avoidancePriority, float baseOffset, bool isStopped, Vector3 nextPosition, float stoppingDistance, bool updatePosition, bool updateRotation)
		{
			this.acceleration = acceleration;
			this.destination = destination;
			this.height = height;
			this.radius = radius;
			this.speed = speed;
			this.velocity = velocity;
			this.angularSpeed = angularSpeed;
			this.areaMask = areaMask;
			this.autoBraking = autoBraking;
			this.autoRepath = autoRepath;
			this.avoidancePriority = avoidancePriority;
			this.baseOffset = baseOffset;
			this.isStopped = isStopped;
			this.nextPosition = nextPosition;
			this.stoppingDistance = stoppingDistance;
			this.updatePosition = updatePosition;
			this.updateRotation = updateRotation;
		}

		public NavMeshAgentSaveData(NavMeshAgent navMeshAgent)
		{
			acceleration = navMeshAgent.acceleration;
			destination = navMeshAgent.destination;
			height = navMeshAgent.height;
			radius = navMeshAgent.radius;
			speed = navMeshAgent.speed;
			velocity = navMeshAgent.velocity;
			angularSpeed = navMeshAgent.angularSpeed;
			areaMask = navMeshAgent.areaMask;
			autoBraking = navMeshAgent.autoBraking;
			autoRepath = navMeshAgent.autoRepath;
			avoidancePriority = navMeshAgent.avoidancePriority;
			baseOffset = navMeshAgent.baseOffset;
			isStopped = navMeshAgent.isStopped;
			nextPosition = navMeshAgent.nextPosition;
			stoppingDistance = navMeshAgent.stoppingDistance;
			updatePosition = navMeshAgent.updatePosition;
			updateRotation = navMeshAgent.updateRotation;
		}
	}
}