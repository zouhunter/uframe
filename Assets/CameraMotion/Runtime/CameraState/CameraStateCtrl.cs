/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 相机管理器                                                                      *
*//************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UFrame;

namespace UFrame.CameraMotion
{
    public class CameraStateCtrl : ICameraStateCtrl
    {
        public bool Regsited { get; private set; }
        public Camera mainCamera
        {
            get
            {
                if (activeState != null)
                {
                    return activeState.camera;
                }
                return Camera.main;
            }
        }
        private Dictionary<int, Type> cameraStateTypeDic;
        private Dictionary<int, CameraState> cameraStateDic;
        private Stack<int> activedStack;
        private CameraState activeState;

        public void SetCameraState(int state)
        {
            if (!cameraStateDic.ContainsKey(state))
            {
                if (cameraStateTypeDic.ContainsKey(state))
                {
                    cameraStateDic.Add(state, Activator.CreateInstance(cameraStateTypeDic[state]) as CameraState);
                }
                else
                {
                    Debug.LogError("无相机类型！");
                }
            }

            if (cameraStateDic.ContainsKey(state))
            {
                var newstate = cameraStateDic[state];
                if (newstate != activeState)
                {
                    if (activeState != null)
                        activeState.SetActive(false);

                    activeState = newstate;
                    activeState.SetActive(true);
                    activedStack.Push(state);
                }
            }
        }

        public void OnRegist()
        {
            activedStack = new Stack<int>();
            cameraStateDic = new Dictionary<int, CameraState>();
            cameraStateTypeDic = new Dictionary<int, Type>();
            Regsited = true;
        }

        public void OnUnRegist()
        {
            activedStack.Clear(); activedStack = null;
            cameraStateDic.Clear(); cameraStateDic = null;
            cameraStateTypeDic.Clear();
            Regsited = false;
        }

        public void RegistCameraStates(int arg1, Type arg2)
        {
            if (arg2 == null) return;

            if (!cameraStateTypeDic.ContainsKey(arg1))
            {
                cameraStateTypeDic.Add(arg1, arg2);
            }
        }

        public void RetreatCameraState()
        {
            if (activeState != null)
            {
                activeState.SetActive(false);
            }

            if (activedStack.Count > 0)
            {
                var state = activedStack.Pop();
                activeState = cameraStateDic[state];
                activeState.SetActive(true);
            }
        }

        public T GetCameraState<T>(int state) where T : CameraState
        {
            if (cameraStateDic.ContainsKey(state))
            {
                return cameraStateDic[state] as T;
            }
            return null;
        }
    }
}