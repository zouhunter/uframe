/*-*-* Copyright (c) webxr@zht
 * Author: zouhunter
 * Creation Date: 2024-03-25
 * Version: 1.0.0
 * Description: 变体信息
 *_*/

using System;
using System.Collections.Generic;

using UnityEngine;

namespace UFrame.BehaviourTree
{
    [Serializable]
    public class InfoModify<T>
    {
        public T value;
        public bool enable;
        public InfoModify(T value)
        {
            this.value = value;
            this.enable = true;
        }
    }

    [Serializable]
    public class TreeInfoModify
    {
        public string id;
        public InfoModify<bool> enable;
        public InfoModify<bool> condition_enable;
        public InfoModify<MatchType> condition_matchType;
        [SerializeReference]
        public List<ConditionInfoModify> condition_modifys;
    }

    [Serializable]
    public class ConditionInfoModify
    {
        public InfoModify<bool> subEnable;
        public InfoModify<int> state;
        public InfoModify<MatchType> matchType;
        public List<InfoModify<int>> sub_conditions;
    }
}

