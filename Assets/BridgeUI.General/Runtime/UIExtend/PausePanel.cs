using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class PausePanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string temp;
    public Text infoText;
    public Image infoImage;
    public Color pausecolor;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Cursor.visible)
        {
			temp = infoText.text;
            infoText.text = "<color=yellow>"+temp+"</color>";
            infoImage.color = pausecolor;
            //Facade.SendNotification<bool>(CommonEvents.PAUSEPLAY,true);
            Time.timeScale = 0;
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (Cursor.visible)
        {
            infoText.text = temp;
            infoImage.color = Color.white;
            //Facade.SendNotification<bool>(CommonEvents.PAUSEPLAY, false);
            Time.timeScale = 1;
        }
    }

}
