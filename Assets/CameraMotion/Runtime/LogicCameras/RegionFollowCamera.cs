/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-06-27                                                                   *
*  功能:                                                                              *
*   - 区域相机跟随脚本（赤色要塞视角）                                                *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.CameraMotion
{
    [AddComponentMenu("UFrame/CameraMotion/RegionFollowCamera")]
    public class RegionFollowCamera : MonoBehaviour
    {
        [SerializeField]
        protected Transform m_target;
        [SerializeField]
        protected float m_speed = 0.5f;
        [SerializeField]
        protected bool m_lerp = false;
        protected Vector3 m_distance;
        protected Vector3 m_nextposition;
        protected float m_timer = 0;
        protected Vector3 m_lastPosition;
        [SerializeField]
        protected Vector3 m_viewRect;
        [SerializeField]
        protected float m_xMin = -3;
        [SerializeField]
        protected float m_xMax = 37;
        [SerializeField]
        protected Camera m_camera;
        protected float m_minZ = int.MinValue;
        [SerializeField]
        protected float m_shakeTime = 0.5f;
        protected float m_shakeTimer;
        protected bool m_shake;
        protected float m_shakeRange = 0.1f;
        protected Vector3 m_shakeOffset;
        [SerializeField]
        protected UnityEngine.Events.UnityEvent m_onShake;

        protected void Start()
        {
            if (!m_camera)
            {
                m_camera = GetComponent<Camera>();
            }
            CalcuteDistance();
            SetTarget(m_target);
            var pos = transform.position;
            pos.x = ClampPosX(pos.x);
            transform.position = pos;
        }

        protected void Update()
        {
            if (!m_target)
            {
                return;
            }

            m_nextposition = m_target.transform.position + m_distance;
            var offset = m_nextposition - transform.position;

            var minX = -m_viewRect.x;
            var maxX = m_viewRect.x;
            var maxY = m_viewRect.y;
            var minY = -m_viewRect.z;

            GetRealRect(ref minX, ref maxX, ref minY, ref maxY);

            if (offset.x < minX)
            {
                m_nextposition.x -= minX;
            }
            else if (offset.x > maxX)
            {
                m_nextposition.x -= maxX;
            }
            else
            {
                m_nextposition.x = transform.position.x;
            }

            if (offset.z < minY)
            {
                m_nextposition.z -= minY;
            }
            else if (offset.z > maxY)
            {
                m_nextposition.z -= maxY;
            }
            else
            {
                m_nextposition.z = transform.position.z;
            }

            m_nextposition.x = ClampPosX(m_nextposition.x);
            ProcessShake(ref m_nextposition);

            if (m_nextposition.z < m_minZ)
            {
                m_nextposition.z = m_minZ;
                transform.position = m_nextposition;
            }

            if (m_lerp)
            {
                var pos = Vector3.Lerp(transform.position, m_nextposition, m_speed);
                pos.y = transform.position.y;
                transform.position = pos;
            }
            else
            {
                m_nextposition.y = transform.position.y;
                transform.position = m_nextposition;
            }
        }

        public void SetTarget(Transform target)
        {
            m_target = target;
        }

        public void Shake()
        {
            m_shake = true;

#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
            m_shakeTimer = Time.time;
            if (m_onShake != null)
            {
                m_onShake.Invoke();
            }
        }

        protected void ProcessShake(ref Vector3 position)
        {
            if (m_shake)
            {
                if (Time.time - m_shakeTimer < m_shakeTime)
                {
                    var randomOffset = UnityEngine.Random.insideUnitCircle * m_shakeRange;
                    position -= m_shakeOffset;
                    m_shakeOffset = new Vector3(randomOffset.x, 0, randomOffset.y);
                    position += m_shakeOffset;
                }
                else
                {
                    m_shake = false;
                }
            }
        }
        protected void CalcuteDistance()
        {
            var centerPos = VecInsertPanel(m_camera.transform.forward, m_camera.transform.position, Vector3.up, 0);
            m_distance = transform.position - centerPos;
        }


        private static Vector3 VecInsertPanel(Vector3 vecDir, Vector3 vecPoint, Vector3 panelNormal, float panelD)
        {
            float dis = Vector3.Dot(panelNormal, vecPoint) - panelD;

            Vector3 insertPoint;
            insertPoint.x = vecDir.x * 1000 + vecPoint.x;
            insertPoint.y = vecDir.y * 1000 + vecPoint.y;
            insertPoint.z = vecDir.z * 1000 + vecPoint.z;

            float td = Vector3.Dot(panelNormal, insertPoint) - panelD;
            float k = dis / (dis - td);

            insertPoint.x -= vecPoint.x;
            insertPoint.y -= vecPoint.y;
            insertPoint.z -= vecPoint.z;

            insertPoint.x *= k;
            insertPoint.y *= k;
            insertPoint.z *= k;

            insertPoint.x += vecPoint.x;
            insertPoint.y += vecPoint.y;
            insertPoint.z += vecPoint.z;

            return insertPoint;
        }

        protected float ClampPosX(float x)
        {
            var viewWidth = m_camera.orthographicSize * 2 * Screen.width / Screen.height;
            var min = m_xMin + viewWidth * 0.5f;
            var max = m_xMax - viewWidth * 0.5f;
            return Mathf.Clamp(x, min, max);
        }

        protected void GetRealRect(ref float leftWidth, ref float rightWidth, ref float backHight, ref float forwardHight)
        {
            var viewHeight = m_camera.orthographicSize * 2;
            var viewWidth = m_camera.orthographicSize * 2 * Screen.width / Screen.height;
            var left = viewWidth * leftWidth * 0.5f;
            var right = viewWidth * rightWidth * 0.5f;
            var forward = viewHeight * forwardHight * 0.5f;
            var back = viewHeight * backHight * 0.5f;
            leftWidth = left;
            rightWidth = right;
            forwardHight = forward;
            backHight = back;
        }

        public void LockMinCameraZ(float minZ)
        {
            m_minZ = minZ;
        }
    }
}