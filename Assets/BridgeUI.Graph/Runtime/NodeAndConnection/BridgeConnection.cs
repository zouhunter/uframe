using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Sprites;
using UnityEngine.Scripting;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Assertions.Must;
using UnityEngine.Assertions.Comparers;
using System.Collections;
using System.Collections.Generic;
using UFrame.NodeGraph;
using UFrame.NodeGraph.DataModel;
using UFrame.BridgeUI;
using System;
namespace UFrame.BridgeUI.Graph
{
    [CustomConnectionAttribute("bridge")]
    public class BridgeConnection : Connection
    {
        public bool blocking;//堵塞
        public ShowMode show = new ShowMode();
    }
}