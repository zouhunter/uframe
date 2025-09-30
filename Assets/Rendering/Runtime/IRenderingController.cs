/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 渲染控制器                                                                      *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.Rendering
{
    public interface IRenderingController
    {
        void SetTransparentRatio(float ratio);
        //设置物体材质为透明
        void TransparentTargets(params Renderer[] renderers);
        //回复物体材质
        void RecoverTransparentedTargets(params Renderer[] renderers);
    }
}
