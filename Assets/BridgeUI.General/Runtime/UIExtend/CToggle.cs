using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CToggle : Toggle {
    private Toggle toggle;
    protected override void Awake()
    {
        base.Awake();
        toggle = GetComponent<CToggle>();
        onValueChanged.AddListener(ChangeImage);
    }

    private void ChangeImage(bool arg0)
    {
        toggle.targetGraphic.GetComponent<Image>().enabled = !arg0;
        toggle.graphic.GetComponent<Image>().enabled = arg0;
    }
}
