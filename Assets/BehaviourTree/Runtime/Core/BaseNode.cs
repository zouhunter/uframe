/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 节点基类
 *_*/
using System;
using System.Collections.Generic;

using UnityEngine;

namespace UFrame.BehaviourTree
{
    public abstract class BaseNode
    {
        private IOwner _owner;
        [HideInInspector]
        public string name;
        public IOwner Owner => _owner;
        public virtual int Priority => 0;

        public static implicit operator bool(BaseNode instance) => instance != null;

        public virtual void SetOwner(IOwner owner)
        {
            _owner = owner;
            BindingRefVars(GetRefVars());
            OnReset();
        }

        protected virtual IEnumerable<IRef> GetRefVars()
        {
            var refs = RefUtil.GetTypeRefs(GetType());
            var refValues = new List<IRef>();
            foreach (var f in refs)
            {
                if (f != null && f.GetValue(this) is IRef refValue && refValue != null)
                {
                    refValues.Add(refValue);
                }
            }
            return refValues;
        }

        public virtual void DoEnd(TreeInfo info)
        {
            OnEnd(info);
        }

        public byte Execute(ITreeInfo info)
        {
            info.tickCount = Owner.TickCount;
            if (info is TreeInfo tInfo && tInfo.condition != null && tInfo.condition.enable)
            {
                if (!TreeInfoUtil.CheckConditions(Owner.TickCount, tInfo))
                {
#if UNITY_EDITOR
                    if (Owner.LogInfo)
                        Debug.Log("condition failed:" + tInfo.node.name);
#endif
                    if (info.status == Status.Running)
                    {
                        OnEnd(info);
                    }

                    info.status = Status.Failure;
                    return info.status;
                }
                else
                {
#if UNITY_EDITOR
                    if (Owner.LogInfo)
                        Debug.Log("condition success:" + tInfo.node.name);
#endif
                }
            }
            if (info.status != Status.Running)
            {
                OnStart(info);
                info.status = Status.Running;
            }
            info.status = OnUpdate(info);
            if (info.status != Status.Running)
            {
                OnEnd(info);
            }
            return info.status;
        }

        public void Clean()
        {
            OnClear();
        }
        protected void BindingRefVars(IEnumerable<IRef> refVars)
        {
            if (refVars != null)
            {
                foreach (var refVar in refVars)
                {
                    refVar?.Binding(Owner);
                }
            }
        }
        protected virtual void OnStart() { }
        protected virtual void OnReset() { }
        protected virtual void OnEnd() { }
        protected virtual void OnClear() { }
        protected virtual void OnStart(ITreeInfo info)
        {
            OnStart();
        }
        protected virtual void OnEnd(ITreeInfo info)
        {
            OnEnd();
        }
        protected virtual byte OnUpdate(ITreeInfo info)
        {
            return OnUpdate();
        }
        protected virtual byte OnUpdate() => Status.Inactive;
    }
}
