using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using UnityEditor;
using UFrame;
using UnityEditorInternal;

namespace UFrame.Refrence
{
    [CustomEditor(typeof(ObjectRefViewMap), true)]
    public class ObjectRefViewMapEditor : Editor
    {
        [MenuItem("Assets/UFrame/Create/ObjectRefViewMap")]
        public static void CreateObjRefViewMap()
        {
            var obj = ScriptableObject.CreateInstance<ObjectRefViewMap>();
            ProjectWindowUtil.CreateAsset(obj, "new_refview_map.asset");
        }

        protected ObjectRefViewMap targetViewMap;
        protected ObjectRefEditor m_refEditor;
        //protected ObjectRefView m_selectedView;
        protected int m_selectIndex = 0;

        private void OnEnable()
        {
            targetViewMap = target as ObjectRefViewMap;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var boxRect = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight * 2);
            EditorGUI.HelpBox(boxRect, "Drag Area!", MessageType.Info);
            boxRect = GUILayoutUtility.GetLastRect();
            DragRect(boxRect);

            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                bool changed = false;
                using (var vertical = new EditorGUILayout.VerticalScope(GUILayout.Width(100)))
                {
                    EditorGUI.DrawRect(vertical.rect, Color.clear);
                    using (var changeScope = new EditorGUI.ChangeCheckScope())
                    {
                        for (int i = 0; i < targetViewMap.objRefViews.Count; i++)
                        {
                            var toggleRect = GUILayoutUtility.GetRect(vertical.rect.width, EditorGUIUtility.singleLineHeight);
                            bool selected = m_selectIndex == i;
                            selected = EditorGUI.Toggle(toggleRect, selected, selected ? EditorStyles.toolbarPopup : EditorStyles.toolbarButton);
                            if (selected)
                                m_selectIndex = i;

                            var item = targetViewMap.objRefViews[i];
                            if (item) EditorGUI.LabelField(toggleRect, item.description);
                            var delRect = toggleRect.MoveX(-20).ReSizeW(20);
                            if ((selected && GUI.Button(delRect, "x")) || !item)
                            {
                                var targetRef = targetViewMap.objRefViews[i];
                                var assetPath = AssetDatabase.GetAssetPath(target);
                                if (!string.IsNullOrEmpty(assetPath) && AssetDatabase.GetAssetPath(targetRef) == AssetDatabase.GetAssetPath(target))
                                {
                                    ScriptableObject.DestroyImmediate(targetRef, true);
                                    AssetDatabase.ImportAsset(assetPath);
                                }
                                targetViewMap.objRefViews.RemoveAt(i);
                                if (m_selectIndex >= targetViewMap.objRefViews.Count)
                                {
                                    m_selectIndex = 0;
                                }
                                changed = true;
                                break;
                            }
                        }

                        var addRect = GUILayoutUtility.GetRect(vertical.rect.width, EditorGUIUtility.singleLineHeight);
                        if (GUI.Button(addRect, "<添加分类>", EditorStyles.toolbarButton))
                        {
                            AddNewRefView(true);
                        }

                        if (changeScope.changed && m_selectIndex >= 0 && m_selectIndex < targetViewMap.objRefViews.Count)
                        {
                            changed = true;
                        }
                    }
                }

                if ((!m_refEditor || changed))
                {
                    if ((targetViewMap.objRefViews.Count > m_selectIndex))
                    {
                        OnViewChanged(targetViewMap.objRefViews[m_selectIndex]);
                    }
                    else
                    {
                        OnViewChanged(null);
                    }
                }
                using (var vertical = new EditorGUILayout.VerticalScope())
                {
                    if (m_refEditor && m_refEditor.target)
                    {
                        m_refEditor.DrawListView();
                    }
                    else
                    {
                        if (GUILayout.Button("新建"))
                        {
                            if (Selection.activeTransform)
                            {
                                RecordDeepth(Selection.activeTransform, "");
                                EditorUtility.SetDirty(target);
                                AssetDatabase.Refresh();
                            }
                            else
                            {
                                var objRef = AddNewRefView(false);
                                OnViewChanged(objRef);
                            }
                        }
                    }
                }
            }
        }

        protected void RecordDeepth(Transform root, string parentName)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (PrefabUtility.IsPartOfAnyPrefab(child))
                {
                    var prefab = PrefabUtility.GetCorrespondingObjectFromSource<GameObject>(child.gameObject);
                    ObjectRefView refView = targetViewMap.objRefViews.Where(x => x.description == parentName).FirstOrDefault();
                    if (!refView)
                    {
                        refView = AddNewRefView(true);
                        refView.description = parentName;
                        refView.name = parentName;
                        OnViewChanged(refView);
                    }

                    if (refView.objRefs.Where(x => x.target == prefab).Count() <= 0)
                    {
                        var obj = new ObjectRef();
                        obj.name = child.name;
                        obj.target = prefab;
                        refView.objRefs.Add(obj);
                        EditorUtility.SetDirty(refView);
                    }
                }
                else
                {
                    RecordDeepth(child, parentName + child.name);
                }
            }
        }

        protected ObjectRefView AddNewRefView(bool inner)
        {
            var objRef = ScriptableObject.CreateInstance<ObjectRefView>();
            objRef.name = "new_object_ref_view";
            targetViewMap.objRefViews.Add(objRef);
            if (inner)
            {
                AssetDatabase.AddObjectToAsset(objRef, target);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
            }
            else
            {
                ProjectWindowUtil.CreateAsset(objRef, objRef.name + ".asset");
            }
            EditorUtility.SetDirty(target);
            AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);
            return objRef;
        }

        protected virtual void DragRect(Rect rect)
        {
            if (rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    bool haveRefItem = false;

                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        if (!(DragAndDrop.objectReferences[i] is ObjectRefView))
                            continue;
                        haveRefItem = true;
                        break;
                    }
                    DragAndDrop.visualMode = haveRefItem ? DragAndDropVisualMode.Move : DragAndDropVisualMode.Rejected;
                }
                else if (Event.current.type == EventType.DragPerform)
                {
                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        if (!(DragAndDrop.objectReferences[i] is ObjectRefView))
                            continue;

                        var obj = DragAndDrop.objectReferences[i] as ObjectRefView;
                        var refView = target as ObjectRefViewMap;
                        if (refView && refView.objRefViews.Find(x => x == obj) == null)
                        {
                            refView.objRefViews.Add(obj);
                        }
                    }
                    DragAndDrop.AcceptDrag();
                    EditorUtility.SetDirty(target);
                }
            }
        }

        protected void OnViewChanged(ObjectRefView refView)
        {
            if (refView != null)
            {
                m_refEditor = Editor.CreateEditor(refView) as ObjectRefEditor;
            }
            else
            {
                m_refEditor = null;
            }
        }
    }
}