/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 资源包信息                                                                      *
*//************************************************************************************/

namespace UFrame.BridgeUI
{
    [System.Serializable]
    public class BundleUIInfo : UIInfoBase
    {
        public string guid;
        public bool good;
        public string bundleName;
        public override string IDName { get { return bundleName + panelName; } }
    }
}