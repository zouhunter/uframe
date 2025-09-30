using UnityEngine;
using System.Collections.Generic;

namespace UFrame.Tween
{
    [System.Serializable]
    public class TweenJumpPath : TweenBase
    {
        public Transform target;
        public Vector3[] path;
        public bool isWorld = false;
        public float jumpHeight;
        private float from = 0f;
        private float to = 1f;
        private List<Vector3> pathPoints = new List<Vector3>();

        private float mValue;
        public float value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
                if (target != null)
                {
                    if (isWorld)
                    {
                        target.position = GetCRSPoint(mValue);
                    }
                    else
                    {
                        target.localPosition = GetCRSPoint(mValue);
                    }
                }
            }
        }

        public override void EnableTween()
        {
            pathPoints = BuildCRSplinePath(new List<Vector3>(path));
        }

        protected override void OnUpdate(float factor, bool isFinished)
        {
            float t = from + factor * (to - from);
            value = Mathf.Clamp01(t);
        }

        public List<Vector3> BuildCRSplinePath(List<Vector3> pts)
        {
            List<Vector3> path = new List<Vector3>(pts);
            if (pts[0] == pts[pts.Count - 1])
            {
                path.Insert(0, pts[pts.Count - 2]);
                path.Add(pts[1]);
            }
            else
            {
                path.Insert(0, pts[0] + (pts[0] - pts[1]));
                path.Add(pts[pts.Count - 1] + (pts[pts.Count - 1] - pts[pts.Count - 2]));
            }
            return path;
        }

        public Vector3 CRSpline(List<Vector3> pts, float t)
        {
            int numSections = pts.Count - 3;
            int currPt = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections - 1);
            float u = t * (float)numSections - (float)currPt;
            Vector3 a = pts[currPt];
            Vector3 b = pts[currPt + 1];
            Vector3 c = pts[currPt + 2];
            Vector3 d = pts[currPt + 3];

            return 0.5f * (
              (-a + 3f * b - 3f * c + d) * (u * u * u)
              + (2f * a - 5f * b + 4f * c - d) * (u * u)
              + (-a + c) * u
              + 2f * b
              );
        }

        public Vector3 GetCRSPoint(float t)
        {
            var splinePos = CRSpline(pathPoints, t);
            var stepPercent = t * (path.Length - 1) % 1;
            stepPercent = 1 - Mathf.Abs(1 - stepPercent * 2);
            var yOffset = EaseLib.easeOutBack(0, jumpHeight, stepPercent);
            splinePos.y += yOffset;// Mathf.Cos(-Mathf.PI * 0.5f + Mathf.PI * (t * (path.Length - 1) % 1)) * jumpHeight;
            return splinePos;
        }

        public void OnDrawGizmos()
        {
            if (path == null)
            {
                return;
            }
            for (int i = 0; i < path.Length; i++)
            {
                Gizmos.DrawWireSphere(path[i], 1f);
            }
        }
        public TweenJumpPath()
        {
        }
        public TweenJumpPath(Transform target,Vector3[] path,float jumpHeight,float dureation,bool isWorld)
        {
            this.target = target;
            this.path = path;
            this.isWorld = isWorld;
            this.duration = dureation;
            this.jumpHeight = jumpHeight;
            if (path == null || path.Length < 1)
                this.enabled = false;
        }
    }
}
