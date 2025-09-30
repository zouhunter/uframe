/*-*-* Copyright (c) webxr@zht
 * Author: zouhunter
 * Creation Date: 2024-01-06 13:58:38
 * Version: 1.0.0
 * Description: camera input controller
 *_*/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// cross scene camera input controller
/// </summary>
public class CameraInputController : MonoBehaviour
{
    private static CameraInputController _instance;
    public static CameraInputController instance
    {
        get
        {
            if (!_instance)
            {
                _instance = new GameObject("CameraInputController").AddComponent<CameraInputController>();
            }
            GameObject.DontDestroyOnLoad(_instance.gameObject);
            return _instance;
        }
    }

    public event CameraHitEvent onHitEvent;//射线检测
    public event CameraTouchEvent onTouchEvent;//连续点击事件
    public event CameraDragEvent onDragEvent;//拖拽事件
    public event CameraScaleEvent onScaleEvent;//缩放事件
    private LayerMask _hitMask;
    private Vector2 _touchDownPos;
    private float _rayDistance = 100f;
    private Camera _camera;
    private float _consecutivetInterval = 0.5f;//连续点击间隔
    private float _lastTouchTime;
    private int _consecutivetTouchCount;
    private float _clickCheckDistance;
    private bool _touchDown;
    private Vector2 _currentTouchPos;
    private int _touchCount;
    private bool _inDrag;
    private CameraInputType _inputType = CameraInputType.All;
    private Dictionary<string, CameraInputType> _maskDic;
    private CameraInputType _nextInputType;

    private void Awake()
    {
        _hitMask = LayerMask.GetMask("interaction", "ground");
        _clickCheckDistance = Screen.width / 100;
        _maskDic = new Dictionary<string, CameraInputType>();
        _nextInputType = _inputType;
    }

    /// <summary>
    /// 注册射线检测事件
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    public CameraHitHandle RegistHitEvent(CameraHitEvent callback, int mask = 0)
    {
        var handle = new CameraHitHandle(callback,
            (x) => { onHitEvent += x; },
            (x) => { onHitEvent -= x; }
        );
        handle.layerMask = mask == 0 ? _hitMask : mask;
        return handle;
    }

    /// <summary>
    /// 注册点击事件
    /// </summary>
    /// <param name="callback"></param>
    public CameraTouchEventHandle RegistTouchEvent(CameraTouchEvent callback)
    {
        return new CameraTouchEventHandle(callback,
            (x) => { onTouchEvent += x; },
            (x) => { onTouchEvent -= x; }
        );
    }

    /// <summary>
    /// 注册拖拽事件
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    internal CameraDragEventHandle RegistDargEvent(CameraDragEvent callback)
    {
        return new CameraDragEventHandle(callback,
            (x) => { onDragEvent += x; },
            (x) => { onDragEvent -= x; }
        );
    }

    /// <summary>
    /// 注册缩放事件
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    internal CameraScaleEventHandle RegistScaleEvent(CameraScaleEvent callback)
    {
        return new CameraScaleEventHandle(callback, 
            (x) => { onScaleEvent += x; },
            (x) => { onScaleEvent -= x; }
        );
    }

    /// <summary>
    /// 添加相机遮罩
    /// </summary>
    /// <param name="maskId"></param>
    /// <param name="inputType"></param>
    /// <returns></returns>
    public CameraInputMaskHandle SetInputMask(string maskId, CameraInputType inputType)
    {
        if (_maskDic.TryGetValue(maskId, out var type))
        {
            _maskDic[maskId] = type | inputType;
        }
        else
        {
            _maskDic.Add(maskId, inputType);
        }
        RefreshInputType();
        return new CameraInputMaskHandle(maskId, RemoveMask);
    }   

    /// <summary>
    /// 刷新输入类型
    /// </summary>
    private void RefreshInputType()
    {
        _nextInputType = CameraInputType.All;
        foreach (var item in _maskDic)
        {
            _nextInputType ^= item.Value;
        }
    }

    /// <summary>
    /// 主动点击屏幕
    /// </summary>
    /// <param name="pos"></param>
    public void UITouchScreen()
    {
        OnClickScreen(GetTouchPosition());
    }

    /// <summary>
    /// 移除遮罩
    /// </summary>
    /// <param name="maskId"></param>
    private void RemoveMask(string maskId)
    {
        if (_maskDic.ContainsKey(maskId))
        {
            _maskDic.Remove(maskId);
            RefreshInputType();
        }
    }

    /// <summary>
    /// 按下位置
    /// </summary>
    /// <returns></returns>
    private Vector2 GetTouchPosition()
    {
        if (Application.isMobilePlatform)
        {
            if (Input.touchCount >= 1)
            {
                return Input.touches[0].position;
            }
            return Vector2.zero;
        }
        else
        {
            return Input.mousePosition;
        }
    }

    /// <summary>
    /// 判断是否按下
    /// </summary>
    /// <returns></returns>
    private bool GetTouched()
    {
        if (Application.isMobilePlatform)
        {
            if (Input.touchCount >= 1)
            {
                _touchCount = Input.touchCount;
                return true;
            }
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                if (Input.GetMouseButton(1))
                {
                    _touchCount = 2;
                }
                else
                {
                    _touchCount = 1;
                }
                return true;
            }
        }
        _touchCount = 0;
        return false;
    }

    /// <summary>
    /// 判断按下
    /// </summary>
    /// <returns></returns>
    private bool GetTouchDown()
    {
        if (Application.isMobilePlatform)
        {
            if (Input.touchCount == 1)
            {
                if (Input.touches[0].phase == TouchPhase.Began)
                {
                    _touchDownPos = Input.touches[0].position;
                    return true;
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                _touchDownPos = Input.mousePosition;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 判断抬起
    /// </summary>
    /// <returns></returns>
    private bool GetTouchUp(out bool keyUp)
    {
        keyUp = false;
        if (Application.isMobilePlatform)
        {
            if (Input.touchCount == 1)
            {
                keyUp = Input.touches[0].phase == TouchPhase.Ended;
                if (keyUp && Vector2.Distance(_touchDownPos, Input.touches[0].position) < _clickCheckDistance)
                    return true;
            }
        }
        else
        {
            keyUp = Input.GetMouseButtonUp(0);
            if (keyUp && Vector2.Distance(_touchDownPos, Input.mousePosition) < _clickCheckDistance)
                return true;
        }
        return false;
    }


    /// <summary>
    /// 判断点击
    /// </summary>
    /// <returns></returns>
    private bool GetClickedIfTouchUp()
    {
        var touchPos = GetTouchPosition();
        if (Vector2.Distance(_touchDownPos, touchPos) < _clickCheckDistance)
            return true;
        return false;
    }

    /// <summary>
    /// 屏幕点击事件
    /// </summary>
    private void OnClickScreen(Vector2 pos)
    {
        //连续点击事件
        if (Time.time - _lastTouchTime < _consecutivetInterval)
            _consecutivetTouchCount++;
        else
            _consecutivetTouchCount = 1;
        _lastTouchTime = Time.time;

        if (_inputType.HasFlag(CameraInputType.Touch))
        {
            try
            {
                //点击事件
                onTouchEvent?.Invoke(pos, _consecutivetTouchCount);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
     

        //射线检测
        if (_camera && _inputType.HasFlag(CameraInputType.Hit))
        {
            Ray ray = _camera.ScreenPointToRay(pos);
            var raycasts = Physics.RaycastAll(ray, _rayDistance, _hitMask, QueryTriggerInteraction.Ignore);
            Array.Sort(raycasts, (a, b) => a.distance.CompareTo(b.distance));
            for (int i = 0; i < raycasts.Length; i++)
            {
                try
                {
                    onHitEvent?.Invoke(raycasts[i], i);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }


    /// <summary>
    /// 结束拖住啊
    /// </summary>
    private void OnStartDrag()
    {
        if (!_inputType.HasFlag(CameraInputType.Drag))
            return;

        try
        {
            onDragEvent?.Invoke(TouchPhase.Began, Vector2.zero, _touchCount);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }


    /// <summary>
    /// 拖拽事件
    /// </summary>
    /// <param name="offset"></param>
    private void OnDragOffset(Vector2 offset)
    {
        if (!_inputType.HasFlag(CameraInputType.Drag))
            return;

        try
        {
            onDragEvent?.Invoke(TouchPhase.Moved, new Vector2(offset.x / Screen.width, offset.y / Screen.height), _touchCount);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// 结束拖住啊
    /// </summary>
    private void OnEndDrag()
    {
        if (!_inputType.HasFlag(CameraInputType.Drag))
            return;

        try
        {
            onDragEvent?.Invoke(TouchPhase.Ended, Vector2.zero, _touchCount);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void OnScroll(float offset)
    {
        if (!_inputType.HasFlag(CameraInputType.Scale))
            return;

        try
        {
            onScaleEvent?.Invoke(offset);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }
    /// <summary>
    /// 识别移动端是否点击到ui
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 缩放检查
    /// </summary>
    /// <returns></returns>
    private bool CheckScroll()
    {
        if (Application.isMobilePlatform)
        {
            if (Input.touchCount == 2)
            {
                var touch1 = Input.GetTouch(0);
                var touch2 = Input.GetTouch(1);
                var lastDistance = Vector2.Distance(touch1.position - touch1.deltaPosition, touch2.position - touch2.deltaPosition);
                var currentDistance = Vector2.Distance(touch1.position, touch2.position);
                if (Vector2.Angle(touch1.deltaPosition, touch2.deltaPosition) > 135)
                {
                    var offset = (currentDistance - lastDistance) / Screen.width;
                    OnScroll(offset);
                }
            }
        }
        else
        {
            var axis = Input.GetAxis("Mouse ScrollWheel");
            if (axis != 0)
            {
                OnScroll(axis);
                return true;
            }
        }
        return false;
    }

    private void FixedUpdate()
    {
        //防止ui触发释放mask后，同一帧触发点击事件
        _inputType = _nextInputType;
    }

    private void Update()
    {
        if (!_camera)
            _camera = Camera.main;

        if (!_camera || !EventSystem.current)
            return;

        var touchDown = GetTouchDown();
        if (touchDown)
        {
            //按下
            _touchDown = true;
            _currentTouchPos = _touchDownPos;
            _inDrag = false;
        }
        else
        {
            var scaling = CheckScroll();
            //抬起
            var touchUp = GetTouchUp(out var keyUp);
            if (keyUp)
            {
                if (_inDrag)
                {
                    OnEndDrag();
                    _inDrag = false;
                }
            }

            if (touchUp)
            {
                //点击
                var clicked = GetClickedIfTouchUp();
                if (clicked)
                {
                    OnClickScreen(_touchDownPos);
                }
                _touchDown = false;
            }
            else
            {
                //拖拽
                if (_touchDown && onDragEvent != null)
                {
                    if (GetTouched() && !scaling)
                    {
                        if (!_inDrag)
                        {
                            OnStartDrag();
                            _inDrag = true;
                        }
                        var currentPos = GetTouchPosition();
                        var offset = currentPos - _currentTouchPos;
                        OnDragOffset(offset);
                        _currentTouchPos = currentPos;
                    }
                    else if (!scaling)
                    {
                        _touchDown = false;
                    }
                }
            }
        }
    }
}

//事件句柄
public delegate void CameraHitEvent(RaycastHit hit, int index);
public delegate void CameraTouchEvent(Vector2 point, int count);
public delegate void CameraDragEvent(TouchPhase step, Vector2 offset, int touchCount);
public delegate void CameraScaleEvent(float offest);

/// <summary>
/// 输入方式
/// </summary>
public enum CameraInputType
{
    None = 0,
    Hit = 1,
    Touch = 1 << 1,
    Drag = 1 << 2,
    Scale = 1 << 3,
    All = -1,
}

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
/// touch事件句柄
/// </summary>
public class CameraTouchEventHandle : InputHandleBase<CameraTouchEvent>
{
    public CameraTouchEventHandle(CameraTouchEvent callback, Action<CameraTouchEvent> registFunc, Action<CameraTouchEvent> removeFunc) : base(callback, registFunc, removeFunc)
    {
    }

    protected override CameraTouchEvent TriggerEvent => OnTrigger;
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

    private void OnTrigger(float offset)
    {
        if (_uiMask && IsTouchUI())
            return;

        try
        {
            _callback?.Invoke(offset);
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

    public CameraInputMaskHandle(string maskId,Action<string> onDispose)
    {
        this._maskId = maskId;
        this._onDispose = onDispose;
    }

    public void Dispose()
    {
        _onDispose?.Invoke(_maskId);
    }
}
