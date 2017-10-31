using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Formation : MonoBehaviour
{

	#region Private params

	[SerializeField] private Transform[] m_positions;

	#endregion


	#region Properties

	public Transform[] Positions {
		get { return m_positions; }
	}

	#endregion
}
