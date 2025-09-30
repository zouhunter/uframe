using UnityEngine;
using UnityEditor;
using static UnityEditorInternal.ReorderableList;

namespace UFrame.BridgeUI.Editors
{
    public class UIInfoListDrawer : ReorderListDrawer
    {
        protected Object m_target;

        public UIInfoListDrawer(string title, Object target = null) : base(title)
        {
            m_target = target;
        }

        public override void InitReorderList(SerializedProperty property)
        {
            base.InitReorderList(property);
            reorderList.headerHeight = 0;
            reorderList.displayAdd = false;
            reorderList.displayRemove = false;
            reorderList.drawElementBackgroundCallback = DrawBackground;
        }

        private void DrawBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (property.arraySize <= index || index < 0) return;

            if (isFocused)
            {
                var oldColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.yellow;
                GUI.Box(rect, "");
                GUI.backgroundColor = oldColor;
            }
        }
        protected override void DrawElementCallBack(Rect rect, int index, bool isActive, bool isFocused)
        {
            base.DrawElementCallBack(rect, index, isActive, isFocused);
            var prop = property.GetArrayElementAtIndex(index);
            rect = BridgeEditorUtility.DrawBoxRect(rect, index.ToString("00"));
            EditorGUI.PropertyField(rect, prop);
        }

        protected override float ElementHeightCallback(int index)
        {
            var prop = property.GetArrayElementAtIndex(index);
            var height = EditorGUI.GetPropertyHeight(prop, null, true) + BridgeUI.Editors.BridgeEditorUtility.padding * 3;
            return height;
        }
    }
}
