using SaveToolbox.Runtime.BasicSaveableMonoBehaviours;
using UnityEngine.AI;

namespace SaveToolbox.Runtime.CustomComponentSavers
{
	public class NavMeshAgentComponentSaver : AbstractComponentSaver<NavMeshAgent>
	{
		public override object Serialize()
		{
			return new NavMeshAgentSaveData(Target);
		}

		public override void Deserialize(object data)
		{
			if (data is NavMeshAgentSaveData navMeshAgentData)
			{
				Target.acceleration = navMeshAgentData.Acceleration;
				Target.destination = navMeshAgentData.Destination;
				Target.height = navMeshAgentData.Height;
				Target.radius = navMeshAgentData.Radius;
				Target.speed = navMeshAgentData.Speed;
				Target.velocity = navMeshAgentData.Velocity;
				Target.angularSpeed = navMeshAgentData.AngularSpeed;
				Target.areaMask = navMeshAgentData.AreaMask;
				Target.autoBraking = navMeshAgentData.AutoBraking;
				Target.autoRepath = navMeshAgentData.AutoRepath;
				Target.avoidancePriority = navMeshAgentData.AvoidancePriority;
				Target.baseOffset = navMeshAgentData.BaseOffset;
				Target.isStopped = navMeshAgentData.IsStopped;
				Target.nextPosition = navMeshAgentData.NextPosition;
				Target.stoppingDistance = navMeshAgentData.StoppingDistance;
				Target.updatePosition = navMeshAgentData.UpdatePosition;
				Target.updateRotation = navMeshAgentData.UpdateRotation;
			}
		}
	}
}