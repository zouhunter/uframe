using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UFrame.EventCenter;
using System;

public class EventTest : MonoBehaviour
{
    private EventController eventCtrl;
    private List<IObserver> observers;
    private void Start()
    {
        eventCtrl = new EventController();
        observers = new List<IObserver>();
    }
    private void OnGUI()
    {
        if (GUILayout.Button("Regist"))
        {
            observers.Add(eventCtrl.Regist("action00", OnAction00));
            observers.Add(eventCtrl.Regist("action0", OnAction0));
            observers.Add(eventCtrl.Regist<string>("action1", OnAction1));
        }
        if (GUILayout.Button("UnRegist"))
        {
            foreach (var item in observers)
            {
                item.Dispose();
            }
            observers.Clear();
        }
        if (GUILayout.Button("UnRegist1"))
        {
            eventCtrl.Remove("action00", OnAction00);
            eventCtrl.Remove("action0", OnAction0);
            eventCtrl.Remove<string>("action1", OnAction1);
            observers.Clear();
        }
        if (GUILayout.Button("Trigger"))
        {
            eventCtrl.Notify("action00");
            eventCtrl.Notify("action0");
            eventCtrl.Notify("action1", "要111");
        }
    }
    private void OnAction00()
    {
        Debug.Log("OnAction00");
    }

    private void OnAction0(string arg1)
    {
        Debug.Log("OnAction0:" +arg1);
    }
    private void OnAction1(string arg1, string t)
    {
        Debug.Log("OnAction1:" + arg1 + ":" + t);
    }
}
