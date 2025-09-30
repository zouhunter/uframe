using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class TestPuremvc : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AppFacade.RegisterMediator("m1",new TestMediator());
        AppFacade.RegisterMediator("m2", new TestMediator());
        AppFacade.RegistCommand("cmd1", new TestCommand());
        AppFacade.RegistCommand<TestCommand2,string,object>("cmd2",true);
        AppFacade.RegisterProxy("data1", new Vector2(1, 2));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.M))
        {
            AppFacade.SendNotification("view_update", 123);
        }

        if (Input.GetKey(KeyCode.C))
        {
            AppFacade.SendNotification("cmd1", 123);
            AppFacade.SendNotification("cmd1", "234");
        }
        if (Input.GetKey(KeyCode.D))
        {
            AppFacade.SendNotification("cmd2", "234", 123);
        }

        if (Input.GetKey(KeyCode.P))
        {
            var data = AppFacade.RetrieveData<Vector2>("data1");
            Debug.Log($"data.x:{data.x}");
        }
    }
}
