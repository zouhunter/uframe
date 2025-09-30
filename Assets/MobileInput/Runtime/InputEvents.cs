/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-01-06 13:58:38
 * Version: 1.0.0
 * Description: camera input controller
 *_*/
using System;
using System.Collections.Generic;
using UnityEngine;


namespace UFrame.MobileInput
{
    //事件句柄
    public delegate void CameraHitsEvent(RaycastHit[] hits);
    public delegate void CameraHitEvent(RaycastHit hit, int index);
    public delegate void CameraClickEvent(Vector2 point, int count);
    public delegate void CameraDragEvent(TouchPhase step, Vector2 offset, int touchCount);
    public delegate void CameraScaleEvent(TouchPhase step, float offest);
    public delegate void CameraRotateEvent(TouchPhase step, float angle);
}
