using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof(NavMeshAgent))]
public class GroupMemberMovement : MonoBehaviour
{


	#region Private params

	[SerializeField] private LeaderMovement m_leader = null;
	[SerializeField] private bool m_setAsCameraTarget = true;

	private NavMeshAgent m_navigationAgent = null;
	private Transform m_transform = null;

	private float m_timeAcum = 0.0f;

	#endregion



	#region Properties

	#endregion



	#region Private methods

	/// <summary>
	/// Updates the member navigation. This icludes POSITION and SPEED
	/// 
	/// To update the position we use the navigation agent setting the destination to the Predicted Position within the group
	/// 
	/// To update the speed we ahve to take into account how far is the member from the predicted position and the current formation position, and its relative position to those points
	/// </summary>
	private void UpdateMemberNavigation ()
	{
		//Get data from leader

		//The movement target is the predicted position on the formation
		Vector3 movementTarget = m_leader.getMemberPredictedPosition (this);
		//This is the current formation position
		Vector3 formationPos = m_leader.getMemberPosition (this);
		//the moving direction of the group is considered as the "forward" of the leader
		Vector3 groupMovingDir = m_leader.CurrentMovementDirection;



		//Update the member navigation target
		m_navigationAgent.SetDestination (movementTarget);





		//Calculate and update navigation speed
		Vector3 targetToPos = movementTarget - m_transform.position;




		float angle = Vector3.Angle (groupMovingDir, targetToPos);

		float speedModifier = 1.0f;

		float distFromTarget = targetToPos.sqrMagnitude;

		float distFromFormation = (m_transform.position - formationPos).sqrMagnitude;


		float distributionRaidus = m_leader.DistributionRadius;
		float sqrRadius = distributionRaidus * distributionRaidus;


		if (angle > 90.0f) { //If the angle between the movementDir and the TargetPos vector is more than 90, we are behind the target pos

			if (distFromFormation >= sqrRadius) { 
				//If we are too far from our formation position we speed up
				speedModifier = Mathf.Clamp ((2.0f * sqrRadius + distFromFormation) / (4.0f * sqrRadius), 1.0f, 2.0f);
			} else if (distFromFormation < sqrRadius) { 
				//otherwise the member moves at normal speed
				speedModifier = 1.0f;
			}

			//If the angle is less, we have to wait for the group
		} else if (angle < 90.0f) {
			if (distFromFormation >= sqrRadius) { 
				//In this case we are far from our formation position, we have to slow down
				speedModifier = Mathf.Clamp ((2.0f * sqrRadius + distFromFormation) / (4.0f * sqrRadius), 1.0f, 2.0f);

			} else if (distFromFormation < sqrRadius) {
				//In this case we have to compare our position wiht the formation position
				Vector3 localFormPos = m_transform.InverseTransformPoint (formationPos);

				if (localFormPos.z < 0.0f) {
					speedModifier = Mathf.Clamp (((2.0f * sqrRadius - distFromFormation) / 2.0f * sqrRadius), 0.2f, 1.0f);
				} else {
					speedModifier = 1.0f;
				}


			}

		} else if (angle == 90.0f) {
			speedModifier = 1.0f;
		}

		//The member speed its always the Group speed multiplied by the modifier calculated previously

		m_navigationAgent.speed = m_leader.GroupSpeed * speedModifier;

	}

	#endregion




	#region Monobehaviour calls

	private void Awake ()
	{
		m_navigationAgent = this.GetComponent<NavMeshAgent> ();
		m_transform = this.GetComponent<Transform> ();
	}

	private void Start ()
	{
		if (m_setAsCameraTarget && CameraController.Singleton != null)
			CameraController.Singleton.m_Targets.Add (m_transform);

		m_timeAcum = 0.0f;
		UpdateMemberNavigation ();
	}

	//the member updates the speed using the update interval defined by the leader
	private void Update ()
	{
		m_timeAcum += Time.deltaTime;

		if (m_timeAcum >= m_leader.PositionUpdateinterval) {
			m_timeAcum = 0.0f;
			UpdateMemberNavigation ();
		}
	}

	#endregion

}
