using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent (typeof(NavMeshAgent))]
public class LeaderMovement : MonoBehaviour
{

	#region Private params

	[SerializeField] private List<GroupMemberMovement> m_groupMembers;

	[SerializeField] private Formation[] m_formations;

	[Header ("Coordinated movement params")]
	[SerializeField] private float m_groupSpeed = 1.0f;
	[SerializeField] private float m_distributionRadius = 2.0f;
	[SerializeField] private float m_positionUpdateInterval = 1.0f;

	private int m_currentFormation = 0;
	private Formation m_currentEnvironmentFormation = null;

	private NavMeshAgent m_navigationAgent = null;

	private Transform m_transform = null;

	#endregion


	#region Properties

	public float GroupSpeed {
		get { return m_groupSpeed; }
	}

	public float DistributionRadius {
		get { return m_distributionRadius; }
	}

	public Vector3 CurrentMovementDirection {
		get { return m_transform.forward; }
	}

	public float PositionUpdateinterval {
		get { return m_positionUpdateInterval; }
	}

	#endregion


	#region Public methods

	/// <summary>
	/// Gets the member position in the formation.
	/// </summary>
	/// <returns>The member position.</returns>
	/// <param name="member">Member.</param>
	public Vector3 getMemberPosition (GroupMemberMovement member)
	{
		int index = m_groupMembers.IndexOf (member);
		bool validMember = index == -1 && index < getCurrentFormation ().Positions.Length;
		return validMember ? new Vector3 (float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity) : getCurrentFormation ().Positions [index].position;
	}


	/// <summary>
	/// Gets the member predicted position, using the group set up
	/// </summary>
	/// <returns>The member predicted position.</returns>
	/// <param name="member">Member.</param>
	public Vector3 getMemberPredictedPosition (GroupMemberMovement member)
	{

		int index = m_groupMembers.IndexOf (member);
		Transform[] formation = getCurrentFormation ().Positions;

		if (index != -1) {

			//Get member position in formation and tranlate it into local space
			Vector3 currentMemberPos = formation [index].position;
			Vector3 localMemberPos = m_transform.InverseTransformPoint (currentMemberPos);

			//Calculate distance for prediction using the current speed and a the time interval used to update the positions
			float predictionDist = m_groupSpeed * m_positionUpdateInterval;
			predictionDist += localMemberPos.magnitude;

			//Calculate a path
			NavMeshHit prediction;
			m_navigationAgent.SamplePathPosition (m_navigationAgent.areaMask, predictionDist, out prediction);


			//Get the predicted position of the group leader and translate it into localspace
			Vector3 localPredictedPos = m_transform.InverseTransformPoint (prediction.position);


			//Add the predicted leader position and the local member position, add them, and translating it into a world space position
			//the result is the World Space predicted position of the group member
			return m_transform.TransformPoint (localPredictedPos + localMemberPos);

		} else
			return new Vector3 (float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

	}

	#endregion


	#region Private methods

	/// <summary>
	/// Center of mass of the formation
	/// </summary>
	/// <returns>The current center of mass.</returns>
	public Vector3 getCurrentCenterOfMass ()
	{
		Vector3 centerOfMass = Vector3.zero;

		for (int i = 0; i < m_groupMembers.Count; ++i) {
			centerOfMass += m_groupMembers [i].transform.position;
		}

		centerOfMass /= m_groupMembers.Count;

		return centerOfMass;
	}



	/// <summary>
	/// Updates the leader speed.
	/// 
	/// In order to update the speed properly we have to distinguish different cases
	/// 
	///  1 - The group has one or less members -> leader moves at full speed
	///  2 - The center of mass is BEHIND the leader (Group members are far from the formation positions) -> leader slows down the speed allowing members to catch up
	///  3 - The center of mass is IN FRONT OF the leader -> leader moves at full speed (members are where they are expected to be)
	/// </summary>
	private void UpdateLeaderSpeed ()
	{
		//We only update the group speed when there is more than one member
		if (m_groupMembers.Count > 1) {

			//To update the speed we use the center of mass of the group and how far we are from that center of mass
			//the longer is the distance to the center of mass the slower the group leader ahs to move, allowing the group members to catch the leader speed

			Vector3 centerOfMass = getCurrentCenterOfMass ();
			Vector3 localCenterOfMass = m_transform.InverseTransformPoint (centerOfMass);

			if (localCenterOfMass.z <= 0.0f) {  //In case that the center of mass is BEHIND the leader, we adjust the leader speed

				float distFromCM = (m_transform.position - centerOfMass).sqrMagnitude;

				float unitRadius = m_distributionRadius;
				float sqrRadius = unitRadius * unitRadius;


				float speedModifier = Mathf.Clamp (((2.0f * sqrRadius - distFromCM) / 2.0f * sqrRadius), 0.2f, 1.0f);


				float speedValue = m_groupSpeed * speedModifier;

				m_navigationAgent.speed = speedValue;

			} else { // in case is in front, the leader moves at full speed
				m_navigationAgent.speed = m_groupSpeed;
			}

		} else { // If the group only has one member, the leader moves at full speed

			m_navigationAgent.speed = m_groupSpeed;
		}
	}

	/// <summary>
	/// Updates the formation, depending on input
	/// </summary>
	private void UpdateFormation ()
	{

		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			m_currentFormation = 0;
		} else if (Input.GetKeyDown (KeyCode.Alpha2)) {
			m_currentFormation = 1;
		}
	}

	private Formation getCurrentFormation ()
	{
		if (m_currentEnvironmentFormation != null)
			return m_currentEnvironmentFormation;
		else
			return m_formations [m_currentFormation];
	}

	#endregion



	#region Monobehaviour calls

	private void Awake ()
	{

		m_navigationAgent = this.GetComponent<NavMeshAgent> ();
		m_transform = this.GetComponent<Transform> ();

	}


	/// <summary>
	/// Used to update the leader speed depending on the group position 
	/// </summary>
	private void Update ()
	{
		UpdateLeaderSpeed ();
		UpdateFormation ();
	}

	/// <summary>
	/// checks if there is an external formaiton inorder to set it up as the current formation to be used
	/// </summary>
	private void OnTriggerEnter (Collider other)
	{
		if (other.CompareTag ("EnvironmentFormation")) {
			Formation externalFormation = other.GetComponent<Formation> ();
			m_currentEnvironmentFormation = externalFormation;
		}
	}

	/// <summary>
	/// forgets the last external formation used
	/// </summary>
	private void OnTriggerExit (Collider other)
	{
		m_currentEnvironmentFormation = null;
	}


	private void OnDrawGizmosSelected ()
	{
		if (m_groupMembers != null && m_groupMembers.Count > 0) {
			Gizmos.color = Color.red;

			Gizmos.DrawWireSphere (getCurrentCenterOfMass (), 0.5f);
		}
	}

	#endregion

}
