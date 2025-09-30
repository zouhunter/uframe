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
using UnityEditor;
using System;

namespace UFrame.Editors
{
    public class InputFieldCreater : DefultElementCreater
    {
        protected override string MenuPath
        {
            get
            {
                return "GameObject/UI/Input Field";
            }
        }
        protected override Type CommponentType
        {
            get
            {
                return typeof(InputField);
            }
        }

        protected override void ChargeWidgetInfo(Component component, WidgetItem info, bool newCreated)
        {
            var inputfield = component as InputField;
            if (info.spriteDic.TryGetValue(KeyWord.background, out var sprite))
            {
                var image = inputfield.targetGraphic as Image;
                if (newCreated)
                {
                    WidgetUtility.InitImage(image, sprite);
                }
                else
                {
                    image.sprite = sprite;
                }
            }
        }

        public override List<Sprite> GetPreviewList(WidgetItem info)
        {
            var list = new List<Sprite>();
            if (info.spriteDic.ContainsKey(KeyWord.background))
            {
                list.Add(info.spriteDic[KeyWord.background]);
            }
            return list;
        }

        protected override List<string> CreateDefultList()
        {
            return new List<string>() { KeyWord.background };
        }
    }
}
