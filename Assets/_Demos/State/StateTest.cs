using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UFrame.EventStates;

public class StateTest : MonoBehaviour
{
    public EventStateCtrl<int> stateCtrl = new EventStateCtrl<int>();
    // Start is called before the first frame update
    void Start()
    {
        stateCtrl.SetEnterEvent(1, OnEnterState);
        stateCtrl.SetEnterEvent(2, OnEnterState);

        stateCtrl.SetExitEvent(1, OnExisState);
        stateCtrl.SetExitEvent(2, OnExisState);

    }

    private void OnEnterState(int state)
    {
        Debug.Log("OnEnterState:" + state);
    }
    private void OnExisState(int state)
    {
        Debug.Log("OnExisState:" + state);
    }

    // Update is called once per frame
    void Update()
    {
        stateCtrl.UpdateState(Random.Range(1, 3));
    }
}
