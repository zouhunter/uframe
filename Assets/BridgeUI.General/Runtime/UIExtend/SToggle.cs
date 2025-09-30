using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class SToggle : Toggle {
    protected override void Awake()
    {
        base.Awake();
        onValueChanged.AddListener(ChangeImage);
    }

    void ChangeImage(bool isOn)
    {
       GetComponent<Image>().enabled = !isOn;
    }
}
