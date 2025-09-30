/****************************************************************************//*
 *   等距视角 -（地图区域线框绘制）                                            *
 *    -                                                                        *
 *******************************************************************************/

using UnityEngine;

namespace UFrame.Isometric
{
	public class MapSpaceGizmos : MonoBehaviour
	{
		public float m_width;
		public float m_hight;
		public Camera m_isoCamera = null;

		void OnDrawGizmos()
		{
			if (m_isoCamera != null)
			{
				Gizmos.color = Color.red;
				var center = transform.position + new Vector3(0f, 0f, m_isoCamera.farClipPlane * 0.5f);
				var size = new Vector3(m_width, m_hight, m_isoCamera.farClipPlane);
				Gizmos.DrawWireCube(center, size);
			}
		}
	}
}