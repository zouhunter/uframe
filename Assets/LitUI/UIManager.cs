// using UnityEngine;

// using UFrame.LitUI;

// public class UIManager : Adapter<UIManager, UIController>
// {
//     public static Canvas RootCanvas { get; private set; }

//     protected override UIController CreateAgent()
//     {
//         var ctrl = new UIController();
//         ctrl.Initialize(new UIAddressLoader());
//         return ctrl;
//     }
//     public static UIAsyncOperation Open(string viewId, object arg = null, Transform parent = null)
//     {
//         return Instance.Open(viewId, arg, parent);
//     }
//     internal static void Hide(string viewId)
//     {
//         Instance.Hide(viewId);
//     }
//     internal static void Close(string viewId)
//     {
//         Instance.Close(viewId);
//     }

//     internal static void SetRootCanvas(Canvas canvas)
//     {
//         RootCanvas = canvas;
//         RootCanvas.overrideSorting = true;
//         RootCanvas.sortingLayerName = SortingLayers.UI;
//     }

//     public static Vector2 WorldToUIAnchorPos(Vector3 worldPos)
//     {
//         var camera = RootCanvas.worldCamera;
//         if (!camera)
//             camera = Camera.main;
//         var viewPos = camera.WorldToViewportPoint(worldPos);
//         var parentRect = UIManager.RootCanvas.GetComponent<RectTransform>();
//         var uiPos = new Vector2(viewPos.x * parentRect.sizeDelta.x, viewPos.y * parentRect.sizeDelta.y);
//         return uiPos;
//     }
// }
