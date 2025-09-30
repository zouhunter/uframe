using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UFrame.BehaviourTree
{
    [Serializable]
    public class UILinePoint
    {
        public Vector2 position;
        public Vector2 curvePointMin, curvePointMax;
        public float thickness = 1;

        public UILinePoint(Vector2 position)
        {
            this.position = position;
        }

        public UILinePoint(Vector2 position, Vector2 curvePointMin, Vector2 curvePointMax)
        {
            this.position = position;
            this.curvePointMin = curvePointMin;
            this.curvePointMax = curvePointMax;
        }
    }

    [Serializable]
    public class UILineGroup
    {
        public Color color;
        public List<UILinePoint> points;
        public UILineGroup() { }
        public UILineGroup(Color color, List<UILinePoint> points)
        {
            this.color = color;
            this.points = points;
        }
    }

    [RequireComponent(typeof(CanvasRenderer))]
    public class UILineRenderer : MaskableGraphic
    {
        public List<UILineGroup> pointsList = new();
        [HideInInspector, SerializeField] private Vector2[] points;
        public float thickness;
        [Range(1, 100)] public int iterations;


        protected override void OnPopulateMesh(VertexHelper vh)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vh.Clear();
            Render(ref vh, ref vertex);
        }

        void Render(ref VertexHelper vh, ref UIVertex vertex)
        {
            int t = 0;
            foreach (var group in pointsList)
            {
                var color = group.color;
                var pointList = group.points;
                points = Line.Create(pointList, iterations);
                for (int i = 1; i < points.Length; i++)
                {
                    CreateVertex(pointList, ref vh, ref vertex, i - 1, true);
                    vertex.color = color;
                    CreateVertex(pointList, ref vh, ref vertex, i, false);
                    vertex.color = color;

                    vh.AddTriangle(t, t + 1, t + 2);
                    vh.AddTriangle(t, t + 2, t + 3);

                    t += 4;
                }
            }

        }

        void CreateVertex(List<UILinePoint> pointList, ref VertexHelper vh, ref UIVertex vertex, int i, bool positive)
        {
            int currentIndex = i;
            int currentPointIndex = i / iterations;
            int lastIndex = i - 1;
            int nextIndex = i + 1;

            Vector2 t0 = (points[currentIndex != points.Length - 1 ? nextIndex : currentIndex] - points[currentIndex]).normalized;
            Vector2 t1 = (points[lastIndex != -1 ? lastIndex : currentIndex] - points[currentIndex]).normalized;

            Vector2 normalized = Vector2.Perpendicular(t0 - t1).normalized;
            Vector2 c =
                normalized *
                (
                    1 +
                    Mathf.SmoothStep(pointList[currentPointIndex].thickness, 0, (float)currentIndex / iterations - currentPointIndex) +
                    Mathf.SmoothStep(0, pointList[currentPointIndex < pointList.Count - 1 ? currentPointIndex + 1 : currentPointIndex].thickness, (float)currentIndex / iterations - currentPointIndex) +
                    (currentIndex == 0 || currentIndex == points.Length - 1
                        ? 0
                        : Mathf.SmoothStep(0, pointList[currentPointIndex].thickness, (180 - Vector2.Angle(t0, t1)) / 180))) * thickness;

            vertex.position = points[currentIndex] + (positive ? -c : c);
            vh.AddVert(vertex);
            vertex.position = points[currentIndex] + (positive ? c : -c);
            vh.AddVert(vertex);
        }
    }

    public static class Line
    {
        private static Vector2[] points;

        public static Vector2[] Create(List<UILinePoint> pointList, int iterations)
        {
            points = new Vector2[(pointList.Count - 1) * iterations + 1];
            int t = 0;
            for (int p = 0; p < pointList.Count - 1; p++)
            {
                for (float i = 0; i < iterations; i++)
                {
                    float j = i / iterations;
                    if (t < points.Length) points[t] = CalculateCubicBezierPoint(j,
                        pointList[p].position,
                        pointList[p].position + pointList[p].curvePointMax,
                        pointList[p + 1].position + pointList[p + 1].curvePointMin,
                        pointList[p + 1].position);
                    t++;
                }
            }
            points[^1] = pointList[^1].position;
            return GetPoints();
        }

        public static Vector2[] GetPoints()
        {
            return points;
        }

        private static Vector2 CalculateCubicBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float a = 1 - t;
            float tt = t * t;
            float aa = a * a;
            float aaa = aa * a;
            float ttt = tt * t;

            Vector2 p = aaa * p0;
            p += 3 * aa * t * p1;
            p += 3 * a * tt * p2;
            p += ttt * p3;

            return p;
        }
    }
}
