//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-12 10:22:37
//* 描    述：

//* ************************************************************************************
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.LitUI
{
    public interface IUICtrl
    {
        // UI打开时触发的事件，传递UI信息。
        event Action<UIInfo> onUIOpenEvent;

        // UI关闭时触发的事件，传递UI信息。
        event Action<UIInfo> onUICloseEvent;

        // 注册UI信息到指定的根Transform下。
        void RegistUIInfos(Transform root, params UIInfo[] infos);

        // 打开指定的UI界面，传递UI信息、参数和父Transform。
        UIAsyncOperation Open(UIInfo info, object arg = null, Transform parent = null);

        // 根据UI名称打开指定的UI界面，传递参数和父Transform。
        UIAsyncOperation Open(string name, object arg = null, Transform parent = null);

        // 关闭指定名称的UI界面。
        bool Close(string name);

        // 获取当前激活的UI界面的名称列表，是否包括堆叠的视图。
        void GetActiveViews(List<string> names, bool includeStack = false);

        // 查找指定名称的UI视图。
        UIView FindView(string name);

        // 查找指定层级的所有UI视图。
        List<UIView> FindViews(byte layer,bool showingOnly = false);

        // 隐藏指定名称的UI视图。
        bool Hide(string name);

        // 显示指定名称的UI视图。
        bool UnHide(string name);

        // 隐藏指定层级的所有UI视图，默认是所有层级。
        void HideALL(byte layer = byte.MaxValue);

        // 显示指定层级的所有UI视图，默认是所有层级。
        void UnHideALL(byte layer = byte.MaxValue);

        // 将指定名称的UI视图压入堆栈。
        bool Stack(string name);

        // 将指定名称的UI视图从堆栈中移除。
        bool UnStack(string name);

        // 将指定层级的所有UI视图压入堆栈，默认是所有层级。
        void StackALL(byte layer = byte.MaxValue);

        // 将指定层级的所有UI视图从堆栈中移除，默认是所有层级。
        void UnStackAll(byte layer = byte.MaxValue);

        // 清理UI视图堆栈，是否仅隐藏视图，默认是清理指定层级。
        void CleanStacks(bool hideOnly = true, byte layer = byte.MaxValue);
    }
}
