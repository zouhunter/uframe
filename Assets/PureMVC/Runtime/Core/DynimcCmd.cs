/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - 动态创建command                                                                          *
*//************************************************************************************/

using System;

namespace UFrame
{
    public abstract class DynimicCmd<Key>
    {
        public bool oneTime { get; set; }
        public Action<Key> onRemoveDynimic { get; set; }
        public Action<Key, BaseObserver<Key>, ICommandBase> onRegist { get; set; }
        public abstract BaseObserver<Key> Create(Controller<Key> controller, Key observerKey);
    }

    public class DynimicCmd<Key, C> : DynimicCmd<Key> where C : ICommand<Key>, new()
    {
        public override BaseObserver<Key> Create(Controller<Key> controller, Key observerKey)
        {
            var command = new C();
            var observer = new ActionObserver<Key>(command.Execute, false);
            if (!oneTime)
            {
                onRegist?.Invoke(observerKey, observer, command);
                onRemoveDynimic?.Invoke(observerKey);
            }
            return observer;
        }
    }

    public class DynimicCmd<Key, C, T1> : DynimicCmd<Key> where C : ICommand<Key, T1>, new()
    {
        public override BaseObserver<Key> Create(Controller<Key> controller, Key observerKey)
        {
            var command = new C();
            var observer = new ActionObserver<Key, T1>(command.Execute, false);
            if (!oneTime)
            {
                onRegist?.Invoke(observerKey, observer, command);
                onRemoveDynimic?.Invoke(observerKey);
            }
            return observer;
        }
    }

    public class DynimicCmd<Key, C, T1, T2> : DynimicCmd<Key> where C : ICommand<Key, T1, T2>, new()
    {
        public override BaseObserver<Key> Create(Controller<Key> controller, Key observerKey)
        {
            var command = new C();
            var observer = new ActionObserver<Key, T1, T2>(command.Execute, false);
            if (!oneTime)
            {
                onRegist?.Invoke(observerKey, observer, command);
                onRemoveDynimic?.Invoke(observerKey);
            }
            return observer;
        }
    }

    public class DynimicCmd<Key, C, T1, T2, T3> : DynimicCmd<Key> where C : ICommand<Key, T1, T2, T3>, new()
    {
        public override BaseObserver<Key> Create(Controller<Key> controller, Key observerKey)
        {
            var command = new C();
            var observer = new ActionObserver<Key, T1, T2, T3>(command.Execute, false);
            if (!oneTime)
            {
                onRegist?.Invoke(observerKey, observer, command);
                onRemoveDynimic?.Invoke(observerKey);
            }
            return observer;
        }
    }
}
