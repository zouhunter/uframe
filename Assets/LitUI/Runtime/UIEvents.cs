//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-12 13:52:36
//* 描    述：

//* ************************************************************************************
using System;
using UnityEngine;

namespace UFrame.LitUI
{

    public delegate void UILoadEvent(UIAsyncOperation operation);
    public delegate void UICloseEvent(UIView view);
    public delegate void UIActiveEvent(UIView view,bool show);
}
