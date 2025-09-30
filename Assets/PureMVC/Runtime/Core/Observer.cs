/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 观查者                                                                          *
*//************************************************************************************/
using System;

namespace UFrame
{
    public abstract class BaseObserver<Key>
    {
        public object Context { get; set; }
        public abstract void NotifyObserver(Key observerKey);
        public abstract void NotifyObserver<T1>(Key observerKey, T1 body1);
        public abstract void NotifyObserver<T1, T2>(Key observerKey, T1 body1, T2 body2);
        public abstract void NotifyObserver<T1, T2, T3>(Key observerKey, T1 body1, T2 body2, T3 body3);
        public abstract bool Valid { get; }
    }

    public abstract class BaseEventObserver<Key> : BaseObserver<Key>
    {
        public bool strict;
        public override bool Valid => Context != null;
    }

    public class DynimicCommandObserver<Key> : BaseObserver<Key>
    {
        private Func<Key, BaseObserver<Key>> getObserver { get; set; }
        public override bool Valid => getObserver != null;
        public DynimicCommandObserver(Func<Key, BaseObserver<Key>> getCmdFunc, object context)
        {
            getObserver = getCmdFunc;
            Context = context;
        }

        private BaseObserver<Key> GetObserver(Key observerKey)
        {
            return getObserver?.Invoke(observerKey);
        }

        public override void NotifyObserver(Key observerKey)
        {
            var observer = GetObserver(observerKey);
            observer?.NotifyObserver(observerKey);
        }

        public override void NotifyObserver<T1>(Key observerKey, T1 body1)
        {
            var observer = GetObserver(observerKey);
            observer?.NotifyObserver(observerKey, body1);
        }

        public override void NotifyObserver<T1, T2>(Key observerKey, T1 body1, T2 body2)
        {
            var observer = GetObserver(observerKey);
            observer?.NotifyObserver(observerKey, body1, body2);
        }

        public override void NotifyObserver<T1, T2, T3>(Key observerKey, T1 body1, T2 body2, T3 body3)
        {
            var observer = GetObserver(observerKey);
            observer?.NotifyObserver(observerKey, body1, body2, body3);
        }
    }

    #region ActionObserver
    public class ActionObserver<Key> : BaseEventObserver<Key>
    {
        private Action<Key> callback { get; set; }

        public ActionObserver(Action<Key> notifyMethod, bool strict)
        {
            callback = notifyMethod;
            Context = notifyMethod;
            this.strict = strict;
        }

        public override void NotifyObserver(Key observerKey)
        {
            callback?.Invoke(observerKey);
        }

        public override void NotifyObserver<T1>(Key observerKey, T1 body1)
        {
            if (!strict)
                NotifyObserver(observerKey);
        }

        public override void NotifyObserver<T1, T2>(Key observerKey, T1 body1, T2 body2)
        {
            if (!strict)
                NotifyObserver(observerKey);
        }

        public override void NotifyObserver<T1, T2, T3>(Key observerKey, T1 body1, T2 body2, T3 body3)
        {
            if (!strict)
                NotifyObserver(observerKey);
        }
    }

    public class ActionObserver<Key, T> : BaseEventObserver<Key>
    {
        private Action<Key, T> callback { get; set; }


        public ActionObserver(Action<Key, T> callback, bool strict)
        {
            this.callback = callback;
            Context = callback;
            this.strict = strict;
        }

        public override void NotifyObserver(Key observerKey)
        {
            if (strict)
                return;

            callback?.Invoke(observerKey, default(T));
        }

        public override void NotifyObserver<P1>(Key observerKey, P1 body1)
        {
            if (callback is Action<Key, P1> action)
            {
                action.Invoke(observerKey, body1);
            }
            else if (body1 is T t)
            {
                callback?.Invoke(observerKey, t);
            }
            else if (Convert.ChangeType(body1, typeof(T)) is T tValue)
            {
                callback?.Invoke(observerKey, tValue);
            }
        }

        public override void NotifyObserver<P1, P2>(Key observerKey, P1 body1, P2 body2)
        {
            if (!strict)
                NotifyObserver(observerKey, body1);
        }

        public override void NotifyObserver<P1, P2, P3>(Key observerKey, P1 body1, P2 body2, P3 body3)
        {
            if (!strict)
                NotifyObserver(observerKey, body1);
        }
    }

    public class ActionObserver<Key, T1, T2> : BaseEventObserver<Key>
    {
        private Action<Key, T1, T2> callback { get; set; }


        public ActionObserver(Action<Key, T1, T2> callback, bool strict)
        {
            this.callback = callback;
            Context = callback;

            this.strict = strict;
        }
        public override void NotifyObserver(Key observerKey)
        {
            if (strict)
                return;

            callback?.Invoke(observerKey, default(T1), default(T2));
        }

        public override void NotifyObserver<P1>(Key observerKey, P1 body1)
        {
            if (strict)
                return;

            if (body1 is T1 t1)
            {
                callback?.Invoke(observerKey, t1, default(T2));
            }
            else if (Convert.ChangeType(body1, typeof(T1)) is T1 t1Value)
            {
                callback?.Invoke(observerKey, t1Value, default(T2));
            }
        }

        public override void NotifyObserver<P1, P2>(Key observerKey, P1 body1, P2 body2)
        {
            if (callback is Action<Key, P1, P2>)
            {
                (callback as Action<Key, P1, P2>).Invoke(observerKey, body1, body2);
            }
            else if (body1 is T1 t1 && body2 is T2 t2)
            {
                callback?.Invoke(observerKey, t1, t2);
            }
            else if (Convert.ChangeType(body1, typeof(T1)) is T1 t1Value && Convert.ChangeType(body2, typeof(T2)) is T2 t2Value)
            {
                callback?.Invoke(observerKey, t1Value, t2Value);
            }
        }

        public override void NotifyObserver<P1, P2, P3>(Key observerKey, P1 body1, P2 body2, P3 body3)
        {
            if (!strict)
                NotifyObserver(observerKey, body1, body2);
        }
    }

    public class ActionObserver<Key, T1, T2, T3> : BaseEventObserver<Key>
    {
        private Action<Key, T1, T2, T3> callback { get; set; }


        public ActionObserver(Action<Key, T1, T2, T3> callback, bool strict)
        {
            this.callback = callback;
            Context = callback;

            this.strict = strict;
        }
        public override void NotifyObserver(Key observerKey)
        {
            if (strict)
                return;

            callback?.Invoke(observerKey, default(T1), default(T2), default(T3));
        }

        public override void NotifyObserver<P1>(Key observerKey, P1 body1)
        {
            if (strict)
                return;

            if (body1 is T1 t1)
            {
                callback?.Invoke(observerKey, t1, default(T2), default(T3));
            }
            else if (Convert.ChangeType(body1, typeof(T1)) is T1 tValue)
            {
                callback?.Invoke(observerKey, tValue, default(T2), default(T3));
            }
        }

        public override void NotifyObserver<P1, P2>(Key observerKey, P1 body1, P2 body2)
        {
            if (strict)
                return;

            if (body1 is T1 t1 && body2 is T2 t2)
            {
                callback?.Invoke(observerKey, t1, t2, default(T3));
            }
            else if (Convert.ChangeType(body1, typeof(T1)) is T1 t1Value && Convert.ChangeType(body2, typeof(T2)) is T2 t2Value)
            {
                callback?.Invoke(observerKey, t1Value, t2Value, default(T3));
            }
        }

        public override void NotifyObserver<P1, P2, P3>(Key observerKey, P1 body1, P2 body2, P3 body3)
        {
            if (callback is Action<Key, P1, P2, P3>)
            {
                (callback as Action<Key, P1, P2, P3>).Invoke(observerKey, body1, body2, body3);
            }
            else if (body1 is T1 t1 && body2 is T2 t2 && body3 is T3 t3)
            {
                callback?.Invoke(observerKey, t1, t2, t3);
            }
            else if (Convert.ChangeType(body1, typeof(T1)) is T1 t1Value && Convert.ChangeType(body2, typeof(T2)) is T2 t2Value && Convert.ChangeType(body3, typeof(T3)) is T3 t3Value)
            {
                callback?.Invoke(observerKey, t1Value, t2Value, t3Value);
            }
        }
    }

    #endregion ActionObserver

    #region EventObserver
    public class EventObserver<Key> : BaseEventObserver<Key>
    {
        private Action callback { get; set; }


        public EventObserver(Action callback, bool strict)
        {
            this.callback = callback;
            Context = callback;

            this.strict = strict;
        }

        public override void NotifyObserver(Key observerKey)
        {
            callback?.Invoke();
        }

        public override void NotifyObserver<T1>(Key observerKey, T1 body1)
        {
            if (!strict)
                NotifyObserver(observerKey);
        }

        public override void NotifyObserver<T1, T2>(Key observerKey, T1 body1, T2 body2)
        {
            if (!strict)
                NotifyObserver(observerKey);
        }

        public override void NotifyObserver<T1, T2, T3>(Key observerKey, T1 body1, T2 body2, T3 body3)
        {
            if (!strict)
                NotifyObserver(observerKey);
        }
    }
    public class EventObserver<Key, T1> : BaseEventObserver<Key>
    {
        private Action<T1> callback { get; set; }


        public EventObserver(Action<T1> callback, bool strict)
        {
            this.callback = callback;
            Context = callback;

            this.strict = strict;
        }

        public override void NotifyObserver(Key observerKey)
        {
            if (strict)
                return;

            callback?.Invoke(default(T1));
        }

        public override void NotifyObserver<P1>(Key observerKey, P1 body1)
        {
            if (callback is Action<P1>)
            {
                (callback as Action<P1>).Invoke(body1);
            }
            else if (body1 is T1 t1)
            {
                callback?.Invoke(t1);
            }
            else if (Convert.ChangeType(body1, typeof(T1)) is T1 t1Value)
            {
                callback?.Invoke(t1Value);
            }
        }

        public override void NotifyObserver<P1, P2>(Key observerKey, P1 body1, P2 body2)
        {
            if (!strict)
                NotifyObserver(observerKey, body1);
        }

        public override void NotifyObserver<P1, P2, P3>(Key observerKey, P1 body1, P2 body2, P3 body3)
        {
            if (!strict)
                NotifyObserver(observerKey, body1);
        }
    }
    public class EventObserver<Key, T1, T2> : BaseEventObserver<Key>
    {
        private Action<T1, T2> callback { get; set; }


        public EventObserver(Action<T1, T2> callback, bool strict)
        {
            this.callback = callback;
            Context = callback;

            this.strict = strict;
        }
        public override void NotifyObserver(Key observerKey)
        {
            if (strict)
                return;

            callback?.Invoke(default(T1), default(T2));
        }

        public override void NotifyObserver<P1>(Key observerKey, P1 body1)
        {
            if (strict)
                return;

            if (body1 is T1 t1)
            {
                callback?.Invoke(t1, default(T2));
            }
            else if (Convert.ChangeType(body1, typeof(T1)) is T1 t1Value)
            {
                callback?.Invoke(t1Value, default(T2));
            }
        }

        public override void NotifyObserver<P1, P2>(Key observerKey, P1 body1, P2 body2)
        {
            if (callback is Action<P1, P2> action)
            {
                action.Invoke(body1, body2);
            }
            else if (body1 is T1 t1 && body2 is T2 t2)
            {
                callback?.Invoke(t1, t2);
            }
            else if (Convert.ChangeType(body1, typeof(T1)) is T1 t1Value && Convert.ChangeType(body2, typeof(T2)) is T2 t2Value)
            {
                callback?.Invoke(t1Value, t2Value);
            }
        }

        public override void NotifyObserver<P1, P2, P3>(Key observerKey, P1 body1, P2 body2, P3 body3)
        {
            if (!strict)
                NotifyObserver(observerKey, body1, body2);
        }
    }
    public class EventObserver<Key, T1, T2, T3> : BaseEventObserver<Key>
    {
        private Action<T1, T2, T3> callback { get; set; }


        public EventObserver(Action<T1, T2, T3> callback, bool strict)
        {
            this.callback = callback;
            Context = callback;

            this.strict = strict;
        }
        public override void NotifyObserver(Key observerKey)
        {
            if (strict)
                return;

            callback?.Invoke(default(T1), default(T2), default(T3));
        }

        public override void NotifyObserver<P1>(Key observerKey, P1 body1)
        {
            if (strict)
                return;

            if (body1 is T1 t1)
            {
                callback?.Invoke(t1, default(T2), default(T3));
            }
            else if (Convert.ChangeType(body1, typeof(T1)) is T1 t1Value)
            {
                callback?.Invoke(t1Value, default(T2), default(T3));
            }
        }

        public override void NotifyObserver<P1, P2>(Key observerKey, P1 body1, P2 body2)
        {
            if (strict)
                return;

            if (body1 is T1 t1 && body2 is T2 t2)
            {
                callback?.Invoke(t1, t2, default(T3));
            }
            else if (Convert.ChangeType(body1, typeof(T1)) is T1 t1Value && Convert.ChangeType(body2, typeof(T2)) is T2 t2Value)
            {
                callback?.Invoke(t1Value, t2Value, default(T3));
            }
        }

        public override void NotifyObserver<P1, P2, P3>(Key observerKey, P1 body1, P2 body2, P3 body3)
        {
            if (callback is Action<P1, P2, P3> action)
            {
                action?.Invoke(body1, body2, body3);
            }
            else if (body1 is T1 t1 && body2 is T2 t2 && body3 is T3 t3)
            {
                callback?.Invoke(t1, t2, t3);
            }
            else if (Convert.ChangeType(body1, typeof(T1)) is T1 t1Value && Convert.ChangeType(body2, typeof(T2)) is T2 t2Value && Convert.ChangeType(body3, typeof(T3)) is T3 t3Value)
            {
                callback?.Invoke(t1Value, t2Value, t3Value);
            }
        }
    }
    #endregion EventObserver
}
