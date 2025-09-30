using System.Collections;
/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 设置窗口                                                                        *
*//************************************************************************************/
using System;

using UnityEditor;

using UnityEngine;
using UnityEngine.UIElements;

namespace UFrame.BehaviourTree
{
    public class ScrollViewContainer
    {
        private Rect region;
        private ScrollView scrollView;
        private IMGUIContainer content;
        private VisualElement scrollViewContent;
        private float zoomSize = 1;
        public readonly float minZoomSize = 0.1f;
        public readonly float maxZoomSize = 1.3f;
        private ZoomManipulator zoomMa;
        private int resetOffsetCount;
        public Action onGUI { get; set; }
        public float ZoomSize
        {
            get
            {
                return zoomSize;
            }
            set
            {
                zoomSize = zoomMa.SetZoom(value);
            }
        }
        public Vector2 scrollOffset
        {
            get
            {
                return _scrollOffset;
            }
        }
        private Vector2 _scrollOffset;

        public void Start(VisualElement root, Rect region)
        {
            this.region = region;
            CreateScrollViewContent(region);
            CreateScrollViewContainer(region);
            CreateScrollView(region);
            CreateZoomManipulator(region);
            root.Add(scrollView);
            ResetCenterOffset();
        }

        public void ResetCenterOffset()
        {
            var offset = (zoomSize * region.size / minZoomSize - region.size) * 0.5f;
            offset.y = 0;
            _scrollOffset = offset;
            scrollView.scrollOffset = offset;
        }

        public void UpdateScale(Rect position)
        {
            var percent = _scrollOffset / region.size;

            region = position;
            scrollView.style.marginTop = position.y;
            scrollView.style.marginLeft = position.x;
            scrollView.style.width = position.width;
            scrollView.style.height = position.height;

            content.style.width = position.width / minZoomSize;//内部固定大小（但scale在作用下会实现与ScrollViewContent一样大）
            content.style.height = position.height / minZoomSize;

            scrollViewContent.style.width = position.width * zoomSize / minZoomSize;//缩放容器以动态改变ScrollView的内部尺寸
            scrollViewContent.style.height = position.height * zoomSize / minZoomSize;

            zoomMa.SetContentSize(position.size);
            _scrollOffset = percent * position.size;
        }

        private void CreateScrollView(Rect position)
        {
            scrollView = new ScrollView()
            {
                style =
                {
                     marginTop = position.y,
                     marginLeft = position.x,
                     width = position.width,
                     height = position.height,
                     backgroundColor = Color.clear
                 },
                horizontalScrollerVisibility = ScrollerVisibility.Auto,
                verticalScrollerVisibility = ScrollerVisibility.Auto,
            };
            // 添加滚动事件监听
            scrollView.verticalScroller.valueChanged += OnScrollerValueChanged;
            scrollView.horizontalScroller.valueChanged += OnScrollerValueChanged;
            scrollView.mouseWheelScrollSize = 0;
            scrollView.Add(scrollViewContent);
            ResetCenterOffset();
        }
        // 添加滚动事件处理方法
        private void OnScrollerValueChanged(float newValue)
        {
            _scrollOffset = scrollView.scrollOffset;
            Refesh(); // 刷新视图
        }
        // 在需要清理时记得移除事件监听
        public void Cleanup()
        {
            if (scrollView != null)
            {
                scrollView.verticalScroller.valueChanged -= OnScrollerValueChanged;
                scrollView.horizontalScroller.valueChanged -= OnScrollerValueChanged;
            }
        }
        private void CreateScrollViewContainer(Rect position)
        {
            scrollViewContent = new VisualElement()
            {
                style = {
                    width = position.width / minZoomSize,
                    height = position.height / minZoomSize,
                    backgroundColor = Color.clear,
                    position = Position.Relative
                }
            };
            scrollViewContent.Add(content);
        }
        private void CreateScrollViewContent(Rect position)
        {
            content = new IMGUIContainer(OnGUI)
            {
                style =
                {
                  width =  position.width / minZoomSize,
                  height = position.height / minZoomSize,
                  backgroundColor = Color.clear,
                  position = Position.Absolute
                }
            };
        }
        private void CreateZoomManipulator(Rect position)
        {
            zoomMa = new ZoomManipulator(minZoomSize, maxZoomSize, content);
            zoomMa.SetContentSize(position.size);
            zoomMa.onZoomChanged = OnZoomValueChanged;
            zoomMa.onScrollMove = (arg1) =>
            {
                _scrollOffset = arg1;
                scrollView.scrollOffset = scrollOffset;
            };
            zoomMa.scrollPosGet = () =>
            {
                return scrollView.scrollOffset;
            };
            scrollView.AddManipulator(zoomMa);
        }
        private void OnGUI()
        {
            if (onGUI != null)
            {
                onGUI.Invoke();
            }
            else
            {
                Debug.Log("empty on Gui!");
            }
        }
        private void OnZoomValueChanged(float arg2)
        {
            zoomSize = arg2;

            var width = region.width * zoomSize / minZoomSize;
            var height = region.height * zoomSize / minZoomSize;
            scrollViewContent.style.width = width;//scrollViewContent的大小随着缩放变化
            scrollViewContent.style.height = height;

            //居中显示调整为居左上角显示
            content.style.left = -(region.width / minZoomSize - width) * 0.5f;
            content.style.top = -(region.height / minZoomSize - height) * 0.5f;
        }

        public void ApplyOffset(bool reset)
        {
            if (reset)
                ResetCenterOffset();
            scrollView.scrollOffset = _scrollOffset;
        }

        public void Refesh()
        {
            content.MarkDirtyRepaint();
            scrollViewContent.MarkDirtyRepaint();
            scrollView.MarkDirtyRepaint();
        }
    }

    public class ZoomManipulator : MouseManipulator, IManipulator
    {
        private VisualElement targetElement;
        private float minSize;
        private float maxSize;
        public readonly float zoomStep = 0.05f;

        public System.Action<float> onZoomChanged { get; set; }
        public System.Action<Vector2> onScrollMove { get; set; }
        public System.Func<Vector2> scrollPosGet { get; set; }
        private Vector2 _contentSize;
        public ZoomManipulator(float minSize, float maxSize, VisualElement element)
        {
            this.minSize = minSize;
            this.maxSize = maxSize;
            this.targetElement = element;
            base.activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.LeftMouse,
                modifiers = EventModifiers.Alt
            });
        }

        public void SetContentSize(Vector2 size)
        {
            _contentSize = size;
        }

        public float SetZoom(float zoom)
        {
            var scale = Mathf.Clamp(zoom, minSize, maxSize);
            targetElement.transform.scale = Vector3.one * scale;
            if (onZoomChanged != null)
            {
                onZoomChanged.Invoke(scale);
            }
            return scale;
        }
        protected override void RegisterCallbacksOnTarget()
        {
            base.target.RegisterCallback<WheelEvent>(OnScroll, TrickleDown.NoTrickleDown);
            base.target.RegisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.NoTrickleDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            base.target.UnregisterCallback<WheelEvent>(OnScroll, TrickleDown.NoTrickleDown);
            base.target.UnregisterCallback<MouseMoveEvent>(OnMouseMove, TrickleDown.NoTrickleDown);
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (evt.altKey)
            {
                var offset = -scrollPosGet.Invoke();
                Vector2 delta = evt.mouseDelta;
                offset += delta;
                onScrollMove?.Invoke(-offset);
            }
        }

        private void OnScroll(WheelEvent e)
        {
            var anchorPos = VisualElementExtensions.ChangeCoordinatesTo(target, targetElement.parent.parent, e.localMousePosition);
            var pos = VisualElementExtensions.ChangeCoordinatesTo(target, targetElement, e.localMousePosition);
            float zoomScale = 1f - e.delta.y * zoomStep;
            var offset = -scrollPosGet.Invoke();
            var scale = Mathf.Clamp(this.targetElement.transform.scale.x * zoomScale, minSize, maxSize);
            this.targetElement.transform.scale = scale * Vector2.one;
            onZoomChanged?.Invoke(scale);

            var realPos = pos * targetElement.transform.scale;
            var offset0 = anchorPos - realPos;
            onScrollMove?.Invoke(realPos - anchorPos); e.StopPropagation();
        }
    }
}
