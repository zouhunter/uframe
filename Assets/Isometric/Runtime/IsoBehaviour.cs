/****************************************************************************//*
 *   等距视角 -（管理器）                                                      *
 *    -  按显示坐标调整3d对象坐标                                              *
 *    -  将所有渲染对象进行z轴排序                                             *
 *    -  设置排序后的对象坐标.                                                 *
 *                                                                             *
 *******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace UFrame.Isometric
{
    public class IsoBehaviour : MonoBehaviour
    {
        [SerializeField]
        public Camera IsoCamera = null;
        [SerializeField]
        public Camera mock3DCamera = null;
        [SerializeField]
        public Transform m_levelBg = null;
        [SerializeField]
        public Transform m_parallaxBg = null;

        [SerializeField]
        public int m_levelBgZ = 60;
        [SerializeField]
        public int m_parallaxBgZ = 90;
        [SerializeField]
        public int m_levelObjectZ = 30;

        protected Vector3 gravityPara = Vector3.zero;
        [System.NonSerialized]
        protected List<IsoRenderer> isoRenderers = new List<IsoRenderer>();

        protected static IsoBehaviour instance;
        public static IsoBehaviour Current
        {
            get
            {
                if (instance == null)
                {
                    instance = (IsoBehaviour)FindObjectOfType(typeof(IsoBehaviour));
                    if (instance == null)
                        Debug.LogError("IsoBehaviour not exists!");
                }
                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected virtual void Start()
        {
            if (IsoCamera == null)
                Debug.LogError("IsoManager need a isometric camera.", this);

            if (mock3DCamera == null)
            {
                Debug.LogError("IsoManager need a mock camera for 3D enviroment.", this);
                return;
            }
            mock3DCamera.enabled = true;
            IsoCamera.orthographicSize = mock3DCamera.orthographicSize;
            mock3DCamera.clearFlags = CameraClearFlags.Depth;
            mock3DCamera.tag = "MainCamera";
            IsoCamera.tag = "Untagged";

            if (m_levelBg != null)
            {
                // Setup the position for level background
                m_levelBg.transform.position += Vector3.forward * m_levelBgZ;
            }
            else
            {
                Debug.Log("Don't have a level background in this scene.", this);
            }

            if (m_parallaxBg != null)
            {
                // Setup parallaxBg position
                m_parallaxBg.transform.position += Vector3.forward * m_parallaxBgZ;
            }
            else
            {
                Debug.Log("Don't have a parallax background in this scene.", this);
            }

            // Setup all adorment objects
            foreach (Transform child in transform)
            {
                if (child == m_levelBg || child == m_parallaxBg ||
                    child == mock3DCamera.transform || child == IsoCamera.transform)
                {
                    continue;
                }

                IsoLevelAdorment adormentObj = child.GetComponent<IsoLevelAdorment>();
                if (adormentObj == null)
                    continue;

                GameObject adormentLogiObj = ((Transform)GameObject.Instantiate(child)).gameObject;
                IsoRenderer isoRender = adormentLogiObj.AddComponent<IsoRenderer>();
                isoRender.RenderObj = child;
                isoRender.DetectSprite();

                adormentLogiObj.transform.position = IsoUtility.VecIsoToPlane(IsoCamera, mock3DCamera, child.transform.position - transform.position, adormentObj.Height);
                if (!isoRender.Is2D)
                {
                    adormentLogiObj.transform.rotation = IsoUtility.QuatIsoTo3D(mock3DCamera.transform, child.transform.localRotation);
                    adormentLogiObj.transform.localScale = child.transform.localScale;
                }
            }
        }

        protected virtual void LateUpdate()
        {
            if (m_parallaxBg != null)
            {
                UpdateParallax();
            }

            // Make the render object follow the 3D owner
            foreach (IsoRenderer renderer in isoRenderers)
            {
                Transform obj3d = renderer.transform;
                Transform objIso = renderer.RenderObj;

                objIso.localPosition = IsoUtility.Vec3DToIso(mock3DCamera, IsoCamera, obj3d.position);

                objIso.localRotation = IsoUtility.Quat3DToIso(mock3DCamera.transform, obj3d.rotation);
                objIso.localScale = obj3d.lossyScale;
            }

            // Sort all iso renderer object by z pos
            isoRenderers.Sort((x, y) =>
                    {
                        if (x.SortOrder != y.SortOrder)
                        {
                            return x.SortOrder - y.SortOrder;
                        }
                        var zcmp = -1 * x.RenderObj.transform.localPosition.z.CompareTo(y.RenderObj.transform.localPosition.z);
                        return zcmp == 0 ? x.GetInstanceID().CompareTo(y.GetInstanceID()) : zcmp;
                    }
                );

            // Sort all
            float currentZ = m_levelObjectZ;
            for (int i = 0; i < isoRenderers.Count; ++i)
            {
                IsoRenderer isoRenderer = isoRenderers[i];
                if (isoRenderer.Is2D)
                {
                    Vector3 curPos = isoRenderer.RenderObj.transform.localPosition;
                    isoRenderer.RenderObj.transform.localPosition = new Vector3(curPos.x, curPos.y, currentZ);
                    currentZ -= 0.1f;
                }
                else
                {
                    Bounds aabb = isoRenderer.GetRenderBounds();
                    float zTrans = currentZ - aabb.max.z;
                    isoRenderer.RenderObj.transform.localPosition = isoRenderer.RenderObj.transform.localPosition + Vector3.forward * zTrans;
                    currentZ -= aabb.size.z;
                }
            }
        }
        public virtual void AddRenderer(IsoRenderer renderer)
        {
            if (renderer == null || renderer.RenderObj == null)
            {
                Debug.LogError("AddRenderer failed. RenderObj is null.", renderer);
                return;
            }

            if (renderer.RenderObj == null)
            {
                Debug.LogError("AddRenderer failed. No Renderer attached to RenderObj");
                return;
            }

            if (!isoRenderers.Contains(renderer))
            {
                isoRenderers.Add(renderer);
            }
        }

        public virtual void RemoveRenderer(IsoRenderer renderer)
        {
            if (renderer)
            {
                isoRenderers.Remove(renderer);
            }
        }

        protected virtual void UpdateParallax()
        {
            // Drag pos
            Vector3 newParaPos = m_parallaxBg.localPosition;
            newParaPos.x = 0.6f * IsoCamera.transform.localPosition.x;
            newParaPos.y = 0.6f * IsoCamera.transform.localPosition.y;

            // Gravity pos
            Vector3 acceleraTarget = Vector3.zero;
            acceleraTarget.x = -1.5f * Input.acceleration.x;
            acceleraTarget.y = -1.5f * (Input.acceleration.y + 0.5f); // The mid pos of gravity is at 45 degree

            gravityPara = Vector3.Lerp(gravityPara, acceleraTarget, 0.5f);

            // Combine
            m_parallaxBg.localPosition = newParaPos + gravityPara;
        }
    }
}
