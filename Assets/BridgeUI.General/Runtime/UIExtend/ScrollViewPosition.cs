using UnityEngine;
using UnityEngine.UI;

public class ScrollViewPosition : MonoBehaviour
{
    public bool back = false;
    private ScrollRect scroll;
    private bool isVertical;
    void Awake()
    {
        scroll = GetComponent<ScrollRect>();
        isVertical = scroll.vertical;
    }

    public void SetPosition(int totalSpan, int index)
    {
        if (!scroll) return;
        if (isVertical)
        {
            if (!back)
            {
                scroll.verticalNormalizedPosition = index / (totalSpan + 0.0f);
            }
            else
            {
                scroll.verticalNormalizedPosition = (totalSpan - index + 0.0f) / totalSpan;
            }
        }
        else
        {
            if (!back)
            {
                scroll.horizontalNormalizedPosition = index / (totalSpan + 0.0f);
            }
            else
            {
                scroll.horizontalNormalizedPosition = (totalSpan - index + 0.0f) / totalSpan;
            }
        }
    }
}