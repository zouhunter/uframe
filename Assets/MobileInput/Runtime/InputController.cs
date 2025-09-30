/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-01-06 13:58:38
 * Version: 1.0.0
 * Description: camera input controller
 *_*/
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;


namespace UFrame.MobileInput
{
    /// <summary>
    /// cross scene camera input controller
    /// </summary>
    public class InputController
    {
        public event CameraHitEvent onHitEvent;//射线检测
        public event CameraHitsEvent onHitsEvent;//射线检测
        public event CameraClickEvent onClickEvent;//连续点击事件
        public event CameraDragEvent onDragEvent;//拖拽事件
        public event CameraScaleEvent onScaleEvent;//缩放事件
        public event CameraRotateEvent onRotateEvent;//旋转事件
        private LayerMask _hitMask;
        private Vector2 _touchDownPos;
        private float _rayDistance = 100f;
        private const float _scaleAngleMin = 155;
        private const float _rotateMinAngle = 30f;
        private Camera _camera;
        private float _consecutivetInterval = 0.5f;//连续点击间隔
        private float _lastTouchTime;
        private int _consecutivetTouchCount;
        private float _clickCheckDistance;
        private bool _touchDown;
        private Vector2 _currentTouchPos;
        private int _touchCount;
        private bool _inDrag;
        private InputType _inputType = InputType.All;
        private Dictionary<string, InputType> _maskDic;
        private InputType _nextInputType;
        private bool _inScale;
        private bool _inRotate;
        private int _lastRotateFrame;
        private int _lastScaleFrame;

        public InputType inputType => _inputType;

        private void Awake()
        {
            _hitMask = LayerMask.GetMask("interaction", "ground", "Player");
            _clickCheckDistance = Screen.width / 100;
            _maskDic = new Dictionary<string, InputType>();
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
        /// 注册射线检测事件
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public CameraHitsHandle RegistHitsEvent(CameraHitsEvent callback, int mask = 0)
        {
            var handle = new CameraHitsHandle(callback,
                (x) => { onHitsEvent += x; },
                (x) => { onHitsEvent -= x; }
            );
            handle.layerMask = mask == 0 ? _hitMask : mask;
            return handle;
        }

        /// <summary>
        /// 注册点击事件
        /// </summary>
        /// <param name="callback"></param>
        public CameraClickEventHandle RegistClickEvent(CameraClickEvent callback)
        {
            return new CameraClickEventHandle(callback,
                (x) => { onClickEvent += x; },
                (x) => { onClickEvent -= x; }
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
        /// 旋转事件
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        internal CameraRotateEventHandle RegistRotateEvent(CameraRotateEvent callback)
        {
            return new CameraRotateEventHandle(callback,
                (x) => { onRotateEvent += x; },
                (x) => { onRotateEvent -= x; }
            );
        }

        /// <summary>
        /// 添加相机遮罩
        /// </summary>
        /// <param name="maskId"></param>
        /// <param name="inputType"></param>
        /// <returns></returns>
        public CameraInputMaskHandle SetInputMask(string maskId, InputType inputType)
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
            _nextInputType = InputType.All;
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
            OnClickScreen(GetTouchPosition(0));
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
        private Vector2 GetTouchPosition(int index)
        {
            if (Application.isMobilePlatform)
            {
                if (Input.touchCount > index)
                {
                    return Input.touches[index].position;
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
        private bool GetTouched(int touchCount)
        {
            if (Application.isMobilePlatform)
            {
                _touchCount = Input.touchCount;

                if (Input.touchCount >= touchCount)
                {
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
                    return _touchCount >= touchCount;
                }
                _touchCount = 0;
            }
            return false;
        }

        private Vector2 GetTouchMove(int index)
        {
            if (Application.isMobilePlatform && Input.touchCount > index)
            {
                var touch = Input.touches[index];
                return touch.deltaPosition;
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                }
            }
            return Vector2.zero;
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
                else if (Input.touchCount == 0 && _touchDown)
                {
                    keyUp = true;
                    return false;
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
            var touchPos = GetTouchPosition(0);
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

            if (_inputType.HasFlag(InputType.Touch))
            {
                try
                {
                    //点击事件
                    onClickEvent?.Invoke(pos, _consecutivetTouchCount);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }


            //射线检测
            if (_camera && _inputType.HasFlag(InputType.Hit))
            {
                Ray ray = _camera.ScreenPointToRay(pos);
                var raycasts = Physics.RaycastAll(ray, _rayDistance, _hitMask, QueryTriggerInteraction.Collide);
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
                try
                {
                    onHitsEvent?.Invoke(raycasts);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }


        /// <summary>
        /// 结束拖住啊
        /// </summary>
        private void OnStartDrag()
        {
            if (!_inputType.HasFlag(InputType.Drag))
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
            if (!_inputType.HasFlag(InputType.Drag))
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
            if (!_inputType.HasFlag(InputType.Drag))
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
        /// <summary>
        /// 缩放
        /// </summary>
        /// <param name="offset"></param>
        private void OnScroll(float offset)
        {
            try
            {
                TouchPhase phase = TouchPhase.Moved;
                if (Time.frameCount - _lastScaleFrame > 1)
                    phase = TouchPhase.Began;
                _lastScaleFrame = Time.frameCount;
                onScaleEvent?.Invoke(phase, offset);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
        private void OnScrollEnd()
        {
            try
            {
                onScaleEvent?.Invoke(TouchPhase.Ended, 0);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
        /// <summary>
        /// 旋转
        /// </summary>
        /// <param name="angle"></param>
        private void OnRotate(float angle)
        {
            if (!_inputType.HasFlag(InputType.Rotate))
                return;

            try
            {
                TouchPhase phase = TouchPhase.Moved;
                if (Time.frameCount - _lastRotateFrame > 1)
                    phase = TouchPhase.Began;
                _lastRotateFrame = Time.frameCount;
                onRotateEvent?.Invoke(phase, angle);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
        /// <summary>
        /// 旋转
        /// </summary>
        /// <param name="angle"></param>
        private void OnRotateEnd()
        {
            if (!_inputType.HasFlag(InputType.Rotate))
                return;

            try
            {
                onRotateEvent?.Invoke(TouchPhase.Ended, 0);
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
                    if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                        return true;
                }
            }
            else if (EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断拖拽
        /// </summary>
        /// <param name="scaling"></param>
        /// <param name="rotate"></param>
        /// <returns></returns>
        private bool CheckDrag()
        {
            if (onDragEvent != null)
            {
                if (GetTouched(2))
                {
                    var move0 = GetTouchMove(0);
                    var move1 = GetTouchMove(1);
                    if (Vector2.Angle(move0, move1) > 90)
                        return false;
                    if (move0.magnitude == 0 || move1.magnitude == 0)
                        return false;
                    if (move0.magnitude > 10 * move1.magnitude || move1.magnitude > 10 * move0.magnitude)
                        return false;
                }

                if (GetTouched(1))
                {
                    var currentPos = GetTouchPosition(0);

                    if (!_inDrag)
                    {
                        OnStartDrag();
                        _inDrag = true;
                    }
                    else
                    {
                        var offset = currentPos - _currentTouchPos;
                        OnDragOffset(offset);
                    }
                    _currentTouchPos = currentPos;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查旋转
        /// </summary>
        /// <returns></returns>
        private bool CheckRotate()
        {
            if (onRotateEvent == null)
                return false;

            if (!_inputType.HasFlag(InputType.Rotate))
                return false;

            if (Application.isMobilePlatform)
            {
                if (Input.touchCount == 2)
                {
                    Touch touch1 = Input.GetTouch(0);
                    Touch touch2 = Input.GetTouch(1);

                    if (touch1.phase != TouchPhase.Moved && touch2.phase != TouchPhase.Moved)
                        return false;

                    if (Vector2.Distance(touch1.position, touch2.position) / Screen.width < 0.2f)
                        return false;
                    //移动方向判断
                    if (Vector2.Angle(touch1.deltaPosition, touch2.deltaPosition) < _rotateMinAngle)
                        return false;

                    //1移动方向和旋转方向夹角判断
                    Vector2 line = touch1.position - touch2.position;
                    var angle1 = Vector2.Angle(line, touch1.deltaPosition);
                    if (Mathf.Min(angle1, 180 - angle1) < _rotateMinAngle)
                        return false;

                    //2移动方向和旋转方向夹角判断
                    var angle2 = Vector2.Angle(line, touch2.deltaPosition);
                    if (Mathf.Min(angle2, 180 - angle2) < _rotateMinAngle)
                        return false;

                    // 通过deltaPosition计算前一帧的触摸点位置
                    Vector2 prevTouch1Pos = touch1.position - touch1.deltaPosition;
                    Vector2 prevTouch2Pos = touch2.position - touch2.deltaPosition;

                    // 当前两指的位置
                    Vector2 currentTouch1Pos = touch1.position;
                    Vector2 currentTouch2Pos = touch2.position;

                    // 计算前一帧向量和当前帧向量
                    Vector2 prevVector = prevTouch2Pos - prevTouch1Pos;
                    Vector2 currentVector = currentTouch2Pos - currentTouch1Pos;

                    // 计算旋转角度
                    float angle = Vector2.SignedAngle(prevVector, currentVector);
                    OnRotate(angle);
                    return true;
                }
            }
            else
            {
                var axis = Input.GetAxis("Mouse ScrollWheel");
                if (axis != 0 && Input.GetMouseButton(1))
                {
                    OnRotate(axis * Screen.width);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 缩放检查
        /// </summary>
        /// <returns></returns>
        private bool CheckScroll()
        {
            if (onScaleEvent == null)
                return false;

            if (!_inputType.HasFlag(InputType.Scale))
                return false;

            if (Application.isMobilePlatform)
            {
                if (Input.touchCount == 2)
                {
                    var touch1 = Input.GetTouch(0);
                    var touch2 = Input.GetTouch(1);

                    if (touch1.phase != TouchPhase.Moved && touch2.phase != TouchPhase.Moved)
                        return false;

                    if (Vector2.Distance(touch1.position, touch2.position) / Screen.width < 0.2f)
                        return false;

                    var lastDistance = Vector2.Distance(touch1.position - touch1.deltaPosition, touch2.position - touch2.deltaPosition);
                    var currentDistance = Vector2.Distance(touch1.position, touch2.position);

                    if (Vector2.Angle((touch1.position * 2 - touch1.deltaPosition) * 0.5f, touch2.deltaPosition) > _rotateMinAngle * 2 &&
                       Vector2.Angle((touch2.position * 2 - touch2.deltaPosition) * 0.5f, touch1.deltaPosition) > _rotateMinAngle * 2)
                        return false;

                    //无旋转可忽略角度限制
                    if (onRotateEvent == null || !_inputType.HasFlag(InputType.Rotate) || Vector2.Angle(touch1.deltaPosition, touch2.deltaPosition) > _scaleAngleMin)
                    {
                        var offset = (currentDistance - lastDistance);
                        OnScroll(offset / Screen.width);
                        return true;
                    }
                }
            }
            else
            {
                var axis = Input.GetAxis("Mouse ScrollWheel");
                if (axis != 0)
                {
                    OnScroll(axis / 10);
                    return true;
                }
            }
            return false;
        }

        public void FixedUpdate()
        {
            //防止ui触发释放mask后，同一帧触发点击事件
            _inputType = _nextInputType;
        }
        public void Update()
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

                if (touchUp && _touchDown)
                {
                    //点击
                    var clicked = GetClickedIfTouchUp();
                    if (clicked)
                        OnClickScreen(_touchDownPos);
                    _touchDown = false;
                }
                else if (_touchDown)
                {
                    //判断旋转
                    _inRotate = CheckRotate();
                    //判断缩放
                    _inScale = CheckScroll();
                    //拖拽
                    CheckDrag();
                }
            }

            if (!_inRotate && Time.frameCount - _lastRotateFrame == 1)
            {
                OnRotateEnd();
            }
            if (!_inScale && Time.frameCount - _lastScaleFrame == 1)
            {
                OnScrollEnd();
            }
        }
    }
}
