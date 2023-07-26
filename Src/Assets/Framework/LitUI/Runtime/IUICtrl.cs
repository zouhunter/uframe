//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-12 10:22:37
//* 描    述：控制器借口

//* ************************************************************************************
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.LitUI
{
    public interface IUICtrl
    {
        event Action<UIInfo> onUIOpenEvent;
        event Action<UIInfo> onUICloseEvent;
        void SetUIRoot(Transform root);
        void RegistUIInfos(params UIInfo[] infos);
        UIAsyncOperation Open(UIInfo info, object arg = null, Transform parent = null);
        UIAsyncOperation Open(string name, object arg = null, Transform parent = null);
        bool Close(string name);
        void GetActiveViews(List<string> names, bool includeStack = false);
        UIView FindView(string name);
        UIView[] FindViews(byte layer);
        bool Hide(string name);
        bool UnHide(string name);
        void HideALL(byte layer = byte.MaxValue);
        void UnHideALL(byte layer = byte.MaxValue);
        bool Stack(string name);
        bool UnStack(string name);
        void StackALL(byte layer = byte.MaxValue);
        void UnStackAll(byte layer = byte.MaxValue);
        void CleanStacks(bool hideOnly = true, byte layer = byte.MaxValue);
    }
}
