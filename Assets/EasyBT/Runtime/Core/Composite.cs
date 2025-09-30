using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UFrame.EasyBT
{
    [DisallowMultipleComponent]
    public abstract class Composite : ParentNode
    {
        [SerializeField]
        protected MatchType _abortType;
        public MatchType abortType => _abortType;
    }
}
