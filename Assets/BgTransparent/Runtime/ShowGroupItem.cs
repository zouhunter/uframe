using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.BgTransparent
{
    [System.Serializable]
    public class ShowGroupItem
    {
        public string key;
        public Material worpmat;
        public GameObject[] worp;
        public RendererOperation renderOperate;
    }
}