/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 界面创建器                                                                      *
*//************************************************************************************/

using UnityEngine;
using UnityEngine.Events;

namespace UFrame.BridgeUI
{

    public interface IPanelCreater
    {
        void CreatePanel(UIInfoBase itemInfo,UnityAction<GameObject> onCreate);
        void CansaleCreatePanel(string panel);
        void ReleasePanel(string panel);
    }
}