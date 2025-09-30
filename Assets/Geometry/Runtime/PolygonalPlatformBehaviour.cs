/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  功能:                                                                              *
*   -                                                                           *
*//************************************************************************************/
using System.Collections.Generic;

using UnityEngine;

namespace UFrame
{
    public class PolygonalPlatformBehaviour : MonoBehaviour
    {
        public float outBound = 0.707f;
        public float redio = 0.707f;
        public int pointNum = 4;
        public float startAngle = -45;
        public Material material;

        [SerializeField]
        private Vector3[] vertices;
        private Vector3 gizmosSize = new Vector3(0.1f, 0.1f, 0.1f);

        private void Awake()
        {
            GenerateMesh();
        }

        private void OnDrawGizmos()
        {
            if (vertices != null && vertices.Length > 0)
            {
                foreach (var vertical in vertices)
                {
                    Gizmos.DrawCube(vertical, gizmosSize);
                }
            }
        }

        [ContextMenu("Generate")]
        public void GenerateMesh()
        {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            if (!meshFilter)
                meshFilter = gameObject.AddComponent<MeshFilter>();
            Mesh mesh = meshFilter.mesh = new Mesh();
            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            if (!renderer)
                renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.material = material;

            //顶点数
            //创建顶点和UV
            vertices = new Vector3[pointNum];
            Vector2[] uv = new Vector2[pointNum];

            //顶点
            var startPos = Quaternion.Euler(0, startAngle, 0) * Vector3.forward * redio;
            var angleOffset = 360 / pointNum;
            for (int i = 0; i < pointNum; i++)
            {
                vertices[i] = Quaternion.Euler(0, angleOffset * i, 0) * startPos;
            }
            //uv
            for (int i = 0; i < vertices.Length; i++)
            {
                var pos = vertices[i];
                var uvx = (pos.x + outBound) / (outBound * 2);
                var uvy = (pos.z + outBound) / (outBound * 2);
                uv[i] = new Vector2(uvx, uvy);
                Debug.LogError(uv[i]);
            }
            Vector2[] vertices2D = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertice = vertices[i];
                vertices2D[i] = new Vector2(vertice.x, vertice.z);
            }
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.uv2 = uv;
            mesh.triangles = Triangulate(vertices2D); //三角面
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }
        public int[] Triangulate(Vector2[] m_points)
        {
            List<int> indices = new List<int>();

            int n = m_points.Length;
            if (n < 3)
                return indices.ToArray();

            int[] V = new int[n];
            if (Area(m_points) > 0)
            {
                for (int v = 0; v < n; v++)
                    V[v] = v;
            }
            else
            {
                for (int v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }

            int nv = n;
            int count = 2 * nv;
            for (int m = 0, v = nv - 1; nv > 2;)
            {
                if ((count--) <= 0)
                    return indices.ToArray();

                int u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                int w = v + 1;
                if (nv <= w)
                    w = 0;

                if (Snip(m_points,u, v, w, nv, V))
                {
                    int a, b, c, s, t;
                    a = V[u];
                    b = V[v];
                    c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    m++;
                    for (s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }

            indices.Reverse();
            return indices.ToArray();
        }
        private float Area(Vector2[] m_points)
        {
            int n = m_points.Length;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector2 pval = m_points[p];
                Vector2 qval = m_points[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }
            return (A * 0.5f);
        }

        private bool Snip(Vector2[] m_points,int u, int v, int w, int n, int[] V)
        {
            int p;
            Vector2 A = m_points[V[u]];
            Vector2 B = m_points[V[v]];
            Vector2 C = m_points[V[w]];
            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;
            for (p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w))
                    continue;
                Vector2 P = m_points[V[p]];
                if (InsideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }
        private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;

            ax = C.x - B.x; ay = C.y - B.y;
            bx = A.x - C.x; by = A.y - C.y;
            cx = B.x - A.x; cy = B.y - A.y;
            apx = P.x - A.x; apy = P.y - A.y;
            bpx = P.x - B.x; bpy = P.y - B.y;
            cpx = P.x - C.x; cpy = P.y - C.y;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }
    }
}