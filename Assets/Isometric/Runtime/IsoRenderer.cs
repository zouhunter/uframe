/****************************************************************************//*
 *   等距视角 -（渲染层）                                                      *
 *    - 控制目标颜色                                                           *
 *    - 收集边界                                                               *
 *                                                                             *
 *******************************************************************************/

using UnityEngine;

namespace UFrame.Isometric
{
    public class IsoRenderer : MonoBehaviour
    {
        [SerializeField]
        protected Transform m_renderPrefab;
        [SerializeField]
        protected bool m_is2D;
        [SerializeField]
        protected int m_sortOrder = 0;
        [SerializeField]
        protected bool m_realPos = true;

        [System.NonSerialized]
        public Color CurColor = Color.white;

        protected Renderer[] renderers;

        protected Transform m_renderObj;
        public ref Transform RenderObj => ref m_renderObj;
        public bool Is2D { get { return m_is2D; } }
        public ref int SortOrder => ref m_sortOrder;
        public ref bool RealPos => ref m_realPos;
        public Transform RenderPrefab => m_renderPrefab;

        protected IsoBehaviour m_isoBehaviour;

        protected virtual void Start()
        {
            m_isoBehaviour = IsoBehaviour.Current;

            if (RenderObj == null)
            {
                if (m_renderPrefab != null)
                {
                    RenderObj = (Transform)Instantiate(m_renderPrefab);
                    if (IsoView)
                    {
                        RenderObj.parent = m_isoBehaviour.transform;
                    }
                }
                else
                {
                    Debug.LogError("Didn't assigned any render object for IsoRendererCtrler!");
                }
            }

            if (IsoView)
            {
                m_isoBehaviour.AddRenderer(this);
            }
            else if (RenderObj)
            {
                RenderObj.transform.SetParent(transform);
                RenderObj.transform.localPosition = Vector3.zero;
                RenderObj.transform.localRotation = Quaternion.identity;
            }

            renderers = RenderObj.GetComponentsInChildren<Renderer>();
            DetectSprite();
        }

        public bool IsoView
        {
            get
            {
                return (!RealPos || Is2D) && m_isoBehaviour;
            }
        }

        protected virtual void Update()
        {
            if (RenderObj == null)
                Destroy(gameObject);

            foreach (Renderer renderer in renderers)
            {
                if (!(renderer is SkinnedMeshRenderer))
                    continue;

                if (!renderer.material.HasProperty("_Color"))
                    return;

                if (renderer.material.color == CurColor)
                    return;

                renderer.material.color = CurColor;
            }
        }

        public static IsoRenderer Create(Transform renderObj)
        {
            GameObject go = new GameObject(renderObj.name);
            IsoRenderer renderer = go.AddComponent<IsoRenderer>();
            renderer.RenderObj = renderObj;
            return renderer;
        }

        public void DetectSprite()
        {
            //if (!Is2D)
            //    Is2D = RenderObj.GetComponentInChildren<tk2dBaseSprite>() != null;
        }

        public Bounds GetRenderBounds()
        {
            if (renderers == null || renderers.Length == 0)
                return new Bounds();

            Bounds renderBound = new Bounds(RenderObj.transform.position, Vector3.zero);
            foreach (Renderer curRenderer in renderers)
            {
                if (curRenderer is TrailRenderer || /*curRenderer is ParticleRenderer ||*/ curRenderer is ParticleSystemRenderer)
                    continue;

                renderBound.Encapsulate(curRenderer.bounds);
            }

            // We need local bounds
            renderBound.center = RenderObj.parent.InverseTransformPoint(renderBound.center);
            if (renderBound.size.z < 0.1f)
                renderBound.size = new Vector3(renderBound.size.x, renderBound.size.y, 0.1f);

            return renderBound;
        }

        protected virtual void OnEnable()
        {
            EnableRender(true);
        }

        protected virtual void OnDisable()
        {
            EnableRender(false);
        }

        protected virtual void EnableRender(bool enable)
        {
            if (renderers == null)
                return;

            foreach (var renderer in renderers)
            {
                if (renderer != null)
                    renderer.enabled = enable;
            }
        }

        protected virtual void OnDestroy()
        {
            if (RenderObj != null)
                Destroy(RenderObj.gameObject);

            if (IsoView)
                m_isoBehaviour.RemoveRenderer(this);
        }
    }
}