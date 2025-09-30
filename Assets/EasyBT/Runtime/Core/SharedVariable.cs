using System.Collections.Generic;

using UnityEngine;

namespace UFrame.EasyBT
{
    public abstract class SharedVariable
    {
        public abstract object GetValue();
        public abstract void SetValue(object value);
    }

    [System.Serializable]
    public class SharedVariable<T> : SharedVariable
    {
        public T Value;
        public override object GetValue()
        {
            return Value;
        }
        public override void SetValue(object value)
        {
            if (value is T)
                Value = (T)value;
            else if(value == null)
                Value = default(T);
        }
        public static explicit operator T(SharedVariable<T> variable) { return variable == null ? variable.Value : default(T); }
        public static explicit operator SharedVariable<T>(T value) { return new SharedVariable<T>() { Value = value }; }
    }

    [System.Serializable]
    public class SharedString : SharedVariable<string> { }
    [System.Serializable]
    public class SharedBool : SharedVariable<bool> { }

    [System.Serializable]
    public class SharedInt : SharedVariable<int> { }
    [System.Serializable]
    public class SharedFloat : SharedVariable<float> { }
    [System.Serializable]
    public class SharedVector2 : SharedVariable<Vector2> { }
    [System.Serializable]
    public class SharedVector3 : SharedVariable<Vector3> { }

    [System.Serializable]
    public class SharedQuaternion : SharedVariable<Quaternion> { }
    [System.Serializable]
    public class SharedColor : SharedVariable<Color> { }
    [System.Serializable]
    public class SharedRect : SharedVariable<Rect> { }
    [System.Serializable]
    public class SharedGameObject : SharedVariable<GameObject> { }
    [System.Serializable]
    public class SharedTransform : SharedVariable<Transform> { }
}
