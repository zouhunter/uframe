/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-06-27                                                                   *
*  功能:                                                                              *
*   - 子物体作为路径显示                                                              *
*//************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.Debugging
{
	[AddComponentMenu("UFrame/Debug/PathGizmosBehaviour")]
	public class PathGizmosBehaviour : MonoBehaviour
	{
		[SerializeField]
		protected float m_nodeSize = 1.5f;
		[SerializeField]
		protected Color m_pathColor = Color.blue;//为随后绘制的gizmos设置颜色。

		void OnDrawGizmos()
		{
			Gizmos.color = m_pathColor;
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				Gizmos.DrawCube(child.transform.position, Vector3.one * m_nodeSize);
				if(i >= 1)
				{
					Gizmos.DrawLine(child.transform.position, transform.GetChild(i - 1).transform.position);
				}
			}
		}
	}
}