using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Sprites;
using UnityEngine.Scripting;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Assertions.Must;
using UnityEngine.Assertions.Comparers;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;

namespace UFrame.Editors
{
    public class ImageCreater : DefultElementCreater
    {
        protected override string MenuPath
        {
            get
            {
                return "GameObject/UI/Image";
            }
        }
        protected override Type CommponentType
        {
            get
            {
                return typeof(Image);
            }
        }
        protected override void ChargeWidgetInfo(Component component, WidgetItem info, bool newCreated)
        {
            var image = component as Image;
            if (info.spriteDic.TryGetValue(KeyWord.sprite, out var sprite))
            {
                if (newCreated)
                    WidgetUtility.InitImage(image, sprite, Image.Type.Simple);
                else
                    image.sprite = sprite;
            }
        }

        public override List<Sprite> GetPreviewList(WidgetItem info)
        {
            List<Sprite> list = new List<Sprite>();
            if (info.spriteDic.ContainsKey(KeyWord.sprite))
            {
                var sprite = info.spriteDic[KeyWord.sprite];
                if (sprite != null)
                {
                    list.Add(sprite);
                }
            }
            return list;
        }
        protected override List<string> CreateDefultList()
        {
            return new List<string>() { KeyWord.sprite };
        }
    }
}
