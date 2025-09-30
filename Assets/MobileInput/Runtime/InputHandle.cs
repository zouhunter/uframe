using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UFrame.MobileInput
{
    /// <summary>
    /// 输入句柄
    /// </summary>
    public interface IInputHandle : IDisposable
    {
        void Pause(bool pause);
        void SetUIMask(bool uiMask);
    }

    /// <summary>
    /// 输入处理基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class InputHandleBase<T> : IInputHandle where T : MulticastDelegate
    {
        protected Action<T> _registFunc;
        protected Action<T> _removeFunc;
        protected T _callback;
        protected bool _uiMask;
        protected abstract T TriggerEvent { get; }
        public InputHandleBase(T callback, Action<T> registFunc, Action<T> removeFunc)
        {
            _callback = callback;
            _registFunc = registFunc;
            _removeFunc = removeFunc;
            _registFunc?.Invoke(TriggerEvent);
        }
        public virtual void Pause(bool pause)
        {
            _removeFunc?.Invoke(TriggerEvent);
            if (!pause)
            {
                _registFunc?.Invoke(TriggerEvent);
            }
        }
        public virtual void Dispose()
        {
            _removeFunc?.Invoke(TriggerEvent);
            _callback = null;
        }
        public void SetUIMask(bool uiMask)
        {
            this._uiMask = uiMask;
        }

        public bool IsTouchUI()
        {
            if (Application.isMobilePlatform)
            {
                if (Input.touchCount > 0)
                {
                    var touch = Input.touches[0];
                    if (touch.phase == TouchPhase.Began)
                    {
                        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                            return true;
                    }
                }
            }
            else if (EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }
            return false;
        }

    }

    /// <summary>
    /// 射线检测事件句柄
    /// </summary>
    public class CameraHitHandle : InputHandleBase<CameraHitEvent>
    {
        public LayerMask layerMask { get; set; }
        public CameraHitHandle(CameraHitEvent callback, Action<CameraHitEvent> registFunc, Action<CameraHitEvent> removeFunc) : base(callback, registFunc, removeFunc)
        {
        }
        protected override CameraHitEvent TriggerEvent => OnTrigger;
        private void OnTrigger(RaycastHit hit, int index)
        {
            if (_uiMask && IsTouchUI())
                return;

            var mask = 1 << hit.collider.gameObject.layer;
            if ((mask & layerMask) == 0)
                return;

            try
            {
                _callback?.Invoke(hit, index);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    /// <summary>
    /// 射线检测事件句柄
    /// </summary>
    public class CameraHitsHandle : InputHandleBase<CameraHitsEvent>
    {
        public LayerMask layerMask { get; set; }
        public CameraHitsHandle(CameraHitsEvent callback, Action<CameraHitsEvent> registFunc, Action<CameraHitsEvent> removeFunc) : base(callback, registFunc, removeFunc)
        {
        }
        protected override CameraHitsEvent TriggerEvent => OnTrigger;
        private void OnTrigger(RaycastHit[] hits)
        {
            if (_uiMask && IsTouchUI())
                return;

            List<RaycastHit> resultHits = new List<RaycastHit>();
            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                var mask = 1 << hit.collider.gameObject.layer;
                if ((mask & layerMask) != 0)
                {
                    resultHits.Add(hit);
                }
            }
            try
            {
                _callback?.Invoke(resultHits.ToArray());
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
    }


    /// <summary>
    /// touch事件句柄
    /// </summary>
    public class CameraClickEventHandle : InputHandleBase<CameraClickEvent>
    {
        public CameraClickEventHandle(CameraClickEvent callback, Action<CameraClickEvent> registFunc, Action<CameraClickEvent> removeFunc) : base(callback, registFunc, removeFunc)
        {
        }

        protected override CameraClickEvent TriggerEvent => OnTrigger;
        private void OnTrigger(Vector2 pos, int count)
        {
            if (_uiMask && IsTouchUI())
                return;

            try
            {
                _callback?.Invoke(pos, count);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    /// <summary>
    /// drag句柄
    /// </summary>
    public class CameraDragEventHandle : InputHandleBase<CameraDragEvent>
    {
        protected override CameraDragEvent TriggerEvent => OnTrigger;

        public CameraDragEventHandle(CameraDragEvent callback, Action<CameraDragEvent> registFunc, Action<CameraDragEvent> removeFunc) : base(callback, registFunc, removeFunc)
        {
        }
        private void OnTrigger(TouchPhase step, Vector2 pos, int dragCount)
        {
            if (_uiMask && IsTouchUI())
                return;

            try
            {
                _callback?.Invoke(step, pos, dragCount);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    /// <summary>
    /// 缩放句柄
    /// </summary>
    public class CameraScaleEventHandle : InputHandleBase<CameraScaleEvent>
    {
        protected override CameraScaleEvent TriggerEvent => OnTrigger;

        public CameraScaleEventHandle(CameraScaleEvent callback, Action<CameraScaleEvent> registFunc, Action<CameraScaleEvent> removeFunc) : base(callback, registFunc, removeFunc)
        {
        }

        private void OnTrigger(TouchPhase step, float offset)
        {
            if (_uiMask && IsTouchUI())
                return;

            try
            {
                _callback?.Invoke(step, offset);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    /// <summary>
    /// 遮罩句柄
    /// </summary>
    public class CameraInputMaskHandle : IDisposable
    {
        private string _maskId;
        private Action<string> _onDispose;

        public CameraInputMaskHandle(string maskId, Action<string> onDispose)
        {
            this._maskId = maskId;
            this._onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose?.Invoke(_maskId);
        }
    }

    /// <summary>
    /// 旋转句柄
    /// </summary>
    public class CameraRotateEventHandle : InputHandleBase<CameraRotateEvent>
    {
        protected override CameraRotateEvent TriggerEvent => OnTrigger;

        public CameraRotateEventHandle(CameraRotateEvent callback, Action<CameraRotateEvent> registFunc, Action<CameraRotateEvent> removeFunc) : base(callback, registFunc, removeFunc)
        {
        }

        private void OnTrigger(TouchPhase step, float angle)
        {
            if (_uiMask && IsTouchUI())
                return;

            try
            {
                _callback?.Invoke(step, angle);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
