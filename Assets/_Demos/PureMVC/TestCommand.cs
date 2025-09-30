using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Command<T> : UFrame.ICommand<string, T>
{
    public virtual bool Strict => true;
    public abstract void Execute(string observerKey, T data);
}

public abstract class Command<T1,T2> : UFrame.ICommand<string, T1, T2>
{
    public virtual bool Strict => true;
    public abstract void Execute(string observerKey, T1 data1,T2 data2);
}

public class TestCommand : Command<string>
{
    public override void Execute(string observerKey, string data)
    {
        Debug.Log("command:" + observerKey + "," + data);
    }
}



public class TestCommand2 : Command<string,object>
{
    public TestCommand2()
    {
        Debug.Log("new test command2!");
    }
    public override void Execute(string observerKey, string data,object data2)
    {
        Debug.Log("command:" + observerKey + "," + data+ "," + data2);
    }
}
