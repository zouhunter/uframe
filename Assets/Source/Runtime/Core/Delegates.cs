/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 资源加载控制器回调                                                              *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Source
{
    public delegate void LoadSourceCallBack<T>(T source, object context) where T:Object;
}
