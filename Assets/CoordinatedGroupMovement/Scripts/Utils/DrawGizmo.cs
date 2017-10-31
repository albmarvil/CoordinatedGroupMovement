using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawGizmo : MonoBehaviour
{

	public enum GizmoType
	{
		CUBE,
		SPHERE
	}

	public bool m_OnDrawGizmosSelected;

	public Color m_color = Color.white;

	public GizmoType m_gizmo;

	public bool m_useWireMesh = false;

	public Vector3 m_size = Vector3.one;


	private void Draw ()
	{
		Gizmos.color = m_color;
		switch (m_gizmo) {
		case GizmoType.CUBE:
			if (m_useWireMesh)
				Gizmos.DrawWireCube (transform.position, m_size);
			else
				Gizmos.DrawCube (transform.position, m_size);
			break;
		case GizmoType.SPHERE:
			if (m_useWireMesh)
				Gizmos.DrawWireSphere (transform.position, m_size.x);
			else
				Gizmos.DrawSphere (transform.position, m_size.x);
			break;
		}
	}

	private void OnDrawGizmos ()
	{
		if (!m_OnDrawGizmosSelected)
			Draw ();
	}


	private void OnDrawGizmosSelected ()
	{
		if (m_OnDrawGizmosSelected)
			Draw ();
	}

}
