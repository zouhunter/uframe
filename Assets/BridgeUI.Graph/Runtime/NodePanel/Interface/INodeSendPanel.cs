using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

namespace UINodePanel
{
    public interface INodeSendPanel
    {
        event UnityAction<string,UnityAction, object> OpenEvent;
    }
}