using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UFrame;

public abstract class Mediator<T> : UFrame.IMediator<string, T>
{
    public abstract ICollection<string> Acceptors { get; }
    public virtual bool Strict => true;
    public abstract void OnNotify(string key, T t);
}

public abstract class Mediator<T1,T2> : UFrame.IMediator<string, T1,T2>
{
    public abstract ICollection<string> Acceptors { get; }
    public virtual bool Strict => true;
    public abstract void OnNotify(string key, T1 t,T2 t2);
}

public class TestMediator : Mediator<object>
{
    public override ICollection<string> Acceptors => new string[] { "view_update" };
    public override void OnNotify(string key,object data)
    {
        Debug.Log($"{this},OnNotify:{key} data:{data}");
    }
}
