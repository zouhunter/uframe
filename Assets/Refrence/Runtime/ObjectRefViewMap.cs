using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.Refrence
{
    public class ObjectRefViewMap : ScriptableObject
    {
        [HideInInspector]
        public List<ObjectRefView> objRefViews = new List<ObjectRefView>();
    }
}