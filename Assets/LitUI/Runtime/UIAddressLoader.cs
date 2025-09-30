////*************************************************************************************
////* 作    者： zouhunter
////* 创建时间： 2023-05-12 10:06:46
////* 描    述： 

////* ************************************************************************************
//using System.Collections.Generic;

//using TMPro;

//using UnityEngine;
//using UnityEngine.TextCore.Text;
//using UnityEngine.UI;

//namespace UFrame.LitUI
//{
//    public class UIAddressLoader : IUILoader
//    {
//        public enum ModifyType
//        {
//            BackMask = 1,//返回
//            TransparentBlankMask = 1 << 1,//透明返回
//            SingleCanvas = 1<<2,//单独canvas
//            Navibar = 1<<3,//带导航条
//        }
//        public async void Load(UIAsyncOperation operation)
//        {
//            UIView view = null;
//            ////var prefab = await AddressableMgr.Instance.LoadAssetAsync<GameObject>(operation.info.name);
//            //if (prefab != null)
//            //{
//            //    view = Object.Instantiate(prefab.gameObject).GetComponent<UIView>();
//            //    view.name = operation.info.name;
//            //    Modify(operation.info.modify, operation,  view);
//            //}
//            operation.Finish(view);
//        }

//        /// <summary>
//        /// 动态UI调整
//        /// </summary>
//        /// <param name="modify"></param>
//        /// <param name="ui"></param>
//        private void Modify(int modify, UIAsyncOperation operation, UIView ui)
//        {
//            bool includeBlankMask = (modify & (int)ModifyType.BackMask) == (int)ModifyType.BackMask;
//            bool includeTransparentBlankMask = (modify & (int)ModifyType.TransparentBlankMask) == (int)ModifyType.TransparentBlankMask;
//            bool singleCanvas = (modify & (int)ModifyType.SingleCanvas) == (int)ModifyType.SingleCanvas;
//            bool withNavibar = (modify & (int)ModifyType.Navibar) == (int)ModifyType.Navibar;
//            if (includeBlankMask || includeTransparentBlankMask)//black close mask
//            {
//                CreateBackMask(includeTransparentBlankMask, ui);
//            }
//            if (singleCanvas)
//            {
//                //ui.GetOrAddComponent<Canvas>();
//                //ui.GetOrAddComponent<GraphicRaycaster>();
//            }
//            if (withNavibar)
//            {
//                //var returnHandle = UIManager.Instance.Open(ViewId.ReturnView, ui);
                
//                //operation.RegistActive((view,active) => {
//                //    returnHandle.view?.SetActive(active);
//                //});
//            }
//#if UNITY_EDITOR
//            //FontModifyInEditor(ui.gameObject);
//#endif
//        }

//        /// <summary>
//        /// 创建点击背景自动返回功能
//        /// </summary>
//        /// <param name="transparent"></param>
//        /// <param name="ui"></param>
//        private void CreateBackMask(bool transparent, UIView ui)
//        {
//            var mask = new GameObject("BackMask").AddComponent<RectTransform>();
//            mask.SetParent(ui.transform, false);
//            mask.SetAsFirstSibling();
//            mask.anchoredPosition = new Vector2(0, 0);
//            mask.anchorMin = Vector2.one * 0.5f;
//            mask.anchorMax = Vector2.one * 0.5f;
//            mask.sizeDelta = new Vector2(3000, 3000);
//            var img = mask.gameObject.AddComponent<Image>();
//            var btn = mask.gameObject.AddComponent<Button>();
//            btn.targetGraphic = img;
//            img.color = transparent ? Color.clear : new Color(0.2f, 0.188f, 0.1725f, 0.7f);
//            btn.onClick.AddListener(ui.Close);
//        }

//#if UNITY_EDITOR
//        /// <summary>
//        /// editor下防止动态字体发生变化，导致提交经常冲突
//        /// </summary>
//        private Dictionary<TMP_FontAsset, TMP_FontAsset> m_fontInstanceMap = new Dictionary<TMP_FontAsset, TMP_FontAsset>();
//        private void FontModifyInEditor(GameObject ui)
//        {
//           var texts = ui.GetComponentsInChildren<TMP_Text>(true);
//            foreach (var item in texts)
//            {
//                if (item.font)
//                {
//                    if(!m_fontInstanceMap.TryGetValue(item.font,out var instance))
//                    {
//                        instance = m_fontInstanceMap[item.font] = UnityEngine.Object.Instantiate(item.font);
//                    }
//                    item.font = instance;
//                }
//            }
//        }
//#endif

//    }
//}
