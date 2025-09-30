/*-*-* Copyright (c) uframe@zht
 * Author: zouhunter
 * Creation Date: 2024-03-14
 * Version: 1.0.0
 * Description: 反射设置字段值
 *_*/

using System.Collections.Generic;
using System.Reflection;

namespace UFrame.BehaviourTree.Actions
{
    [NodePath("反射/设置字段值")]
    public class SetFieldValue : ActionNode
    {
        public Ref<object> target;
        public Ref<string> fieldName;
        public Ref<object> result;
        public BindingFlags bindingFlags;

        protected override IEnumerable<IRef> GetRefVars()
        {
            return new IRef[] { target, fieldName, result };
        }

        protected override byte OnUpdate()
        {
            if (target.Value == null || string.IsNullOrEmpty(fieldName))
            {
                return Status.Failure;
            }
            var field = target.Value.GetType().GetField(fieldName, bindingFlags);
            if (field != null)
            {
                field.SetValue(target.Value, result);
                return Status.Success;
            }
            return Status.Failure;
        }
    }
}
