using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UFrame.Geometrys
{
    public class Ring3D : MonoBehaviour
    {
        public float Radius = 20.0f;                //外圈的半径
        public float Thickness = 10.0f;             //厚度，外圈半径减去内圈半径
        public float Depth = 1.0f;                  //厚度
        public float NumberOfSides = 30.0f;         //由多少个面组成
        public float DrawArchDegrees = 90.0f;       //要绘画多长
        public Material archMaterial = null;

        private List<Vector3> vertexList = new List<Vector3>();
        private List<int> triangleList = new List<int>();
        private List<Vector2> uvList = new List<Vector2>();
        private MeshFilter m_meshFilter;
        private MeshRenderer m_meshRender;

        private void OnEnable()
        {
            ReGenerateVertex();
        }

        [ContextMenu("ReGenerate")]
        public void ReGenerateVertex()
        {
            //顶点坐标
            vertexList.Clear();
            float incrementAngle = DrawArchDegrees / NumberOfSides;
            //小于等于是因为n+1条线才能组成n个面
            for (int i = 0; i <= NumberOfSides; i++)
            {
                float angle = 180 - i * incrementAngle;
                float innerX = (Radius - Thickness) * Mathf.Cos(angle * Mathf.Deg2Rad);
                float innerY = (Radius - Thickness) * Mathf.Sin(angle * Mathf.Deg2Rad);
                vertexList.Add(new Vector3(innerX, innerY, 0));
                float outsideX = Radius * Mathf.Cos(angle * Mathf.Deg2Rad);
                float outsideY = Radius * Mathf.Sin(angle * Mathf.Deg2Rad);
                vertexList.Add(new Vector3(outsideX, outsideY, 0));
            }

            //三角形索引
            triangleList.Clear();
            int direction = 1;
            for (int i = 0; i < NumberOfSides * 2; i++)
            {
                int[] triangleIndexs = getTriangleIndexs(i, direction);
                direction *= -1;
                for (int j = 0; j < triangleIndexs.Length; j++)
                {
                    triangleList.Add(triangleIndexs[j]);
                }
            }

            //UV索引
            uvList.Clear();
            for (int i = 0; i <= NumberOfSides; i++)
            {
                float angle = 180 - i * incrementAngle;
                float littleX = (1.0f / NumberOfSides) * i;
                uvList.Add(new Vector2(littleX, 0));
                float bigX = (1.0f / NumberOfSides) * i;
                uvList.Add(new Vector2(bigX, 1));
            }
            Mesh mesh = new Mesh()
            {
                vertices = vertexList.ToArray(),
                uv = uvList.ToArray(),
                triangles = triangleList.ToArray(),
            };

            mesh.RecalculateNormals();
            MustMeshComponent();
            m_meshFilter.mesh = mesh;
            m_meshRender.material = archMaterial;
        }

        protected void MustMeshComponent()
        {
            if(!m_meshFilter)
                m_meshFilter = gameObject.GetComponent<MeshFilter>();
            if (!m_meshRender)
                m_meshRender = gameObject.GetComponent<MeshRenderer>();
            if (!m_meshFilter)
                m_meshFilter = gameObject.AddComponent<MeshFilter>();
            if (!m_meshRender)
                m_meshRender = gameObject.AddComponent<MeshRenderer>();
        }

        protected int[] getTriangleIndexs(int index, int direction)
        {
            int[] triangleIndexs = new int[3] { 0, 1, 2 };
            for (int i = 0; i < triangleIndexs.Length; i++)
            {
                triangleIndexs[i] += index;
            }
            if (direction == -1)
            {
                int temp = triangleIndexs[0];
                triangleIndexs[0] = triangleIndexs[2];
                triangleIndexs[2] = temp;
            }
            return triangleIndexs;
        }
    }
}