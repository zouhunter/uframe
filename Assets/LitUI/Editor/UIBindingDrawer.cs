//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-12 10:26:14
//* 描    述：

//* ************************************************************************************
using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Object = UnityEngine.Object;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace UFrame.LitUI
{
    [CustomPropertyDrawer(typeof(UIBinding), true)]
    public class UIBindingDrawer : PropertyDrawer
    {
        protected SerializedProperty m_bindingProp;
        protected SerializedProperty m_refsProp;
        private ReorderableList m_refsList;
        private static Dictionary<int, Object> m_instanceIds;
        private static GUIContent m_iconContent;
        private static GUIContent m_rootIconContent;
        private static int m_activeId;
        private bool m_inedit;
        private const string m_autoAddComponentKey = "UIBindingDrawer.AutoAddComponent";
        static UIBindingDrawer()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnDrawHierachyItem;
            m_iconContent = EditorGUIUtility.IconContent("d_AreaEffector2D Icon", "using");
            m_rootIconContent = EditorGUIUtility.IconContent("d_CharacterJoint Icon", "using");
        }

        public UIBindingDrawer()
        {
            m_instanceIds = new Dictionary<int, Object>();
        }

        [MenuItem("GameObject/UI/New UIView")]
        public static void CreateNewUIView()
        {
            var sb = new StringBuilder();
            var headString = UICodeGen.CreateHead("", System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
               "UIView子类，");
            sb.AppendLine(headString);
            sb.AppendLine("using System;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.UI;");
            sb.AppendLine("using UFrame.LitUI;");
            sb.AppendLine();
            sb.Append("public class CLASS_NAME:"); sb.Append(UISetting.Instance.uiViewBase);
            sb.AppendLine("{");
            sb.AppendLine("#region " + UISetting.Instance.uiViewReginFlag);
            sb.AppendLine("#endregion " + UISetting.Instance.uiViewReginFlag);
            sb.AppendLine("}");
            var rootClassName = Selection.activeGameObject.name;
            var root = Selection.activeGameObject.transform.parent;
            UIView rootView = null;
            while (root != null)
            {
                if (root.GetComponent<UIView>())
                {
                    rootClassName = root.name;
                    rootView = root.GetComponent<UIView>();
                }
                root = root.parent;
            }
            if (rootClassName == Selection.activeGameObject.name)
            {
                CreateScriptFromTemplate(Selection.activeGameObject.name, sb.ToString());
            }
            else
            {
                CreateSubViewScriptFromTemplate(rootView, Selection.activeGameObject.name, sb.ToString());
            }
        }
        public static void CreateScriptFromTemplate(string name, string context)
        {
            var exportFolder = Application.dataPath + $"/{UISetting.Instance.codePath}/{name}";
            var filePath = $"{exportFolder}/{name}";
            System.IO.Directory.CreateDirectory(exportFolder);
            var file = EditorUtility.SaveFilePanel("Create View To", exportFolder, name, "cs");
            if (!string.IsNullOrEmpty(file))
            {
                if (System.IO.File.Exists(file) && !EditorUtility.DisplayDialog("file exists", file, "overwrite"))
                    return;
                EditorPrefs.SetString(m_autoAddComponentKey, Path.GetRelativePath(System.Environment.CurrentDirectory, file));
                System.IO.File.WriteAllText(file, context.Replace("CLASS_NAME", name));
                AssetDatabase.Refresh();
            }
            else
            {
                if (System.IO.Directory.GetFileSystemEntries(exportFolder).Length == 0)
                {
                    System.IO.Directory.Delete(exportFolder);
                    var metaFile = exportFolder + ".meta";
                    if (System.IO.File.Exists(metaFile))
                        System.IO.File.Delete(metaFile);
                }
            }
        }
        public static void CreateSubViewScriptFromTemplate(UIView rootView, string name, string context)
        {
            var finalClassName = rootView.name.Replace("View", "") + name + "SubView";
            var exportFolder = Application.dataPath + $"/{UISetting.Instance.codePath}/{rootView.name}";
            var filePath = $"{exportFolder}/{finalClassName}";
            System.IO.Directory.CreateDirectory(exportFolder);
            var file = EditorUtility.SaveFilePanel("Create View To", exportFolder, finalClassName, "cs");
            if (!string.IsNullOrEmpty(file))
            {
                if (System.IO.File.Exists(file) && !EditorUtility.DisplayDialog("file exists", file, "overwrite"))
                    return;
                EditorPrefs.SetString(m_autoAddComponentKey, Path.GetRelativePath(System.Environment.CurrentDirectory, file));
                System.IO.File.WriteAllText(file, context.Replace("CLASS_NAME", finalClassName));
                AssetDatabase.Refresh();
            }
            else
            {
                if (System.IO.Directory.GetFileSystemEntries(exportFolder).Length == 0)
                {
                    System.IO.Directory.Delete(exportFolder);
                    var metaFile = exportFolder + ".meta";
                    if (System.IO.File.Exists(metaFile))
                        System.IO.File.Delete(metaFile);
                }
            }
        }
        private static void OnDrawHierachyItem(int instanceID, Rect selectionRect)
        {
            if (instanceID == m_activeId)
            {
                var rect = new Rect(selectionRect.x + selectionRect.width - 10, selectionRect.y + 5, 10, selectionRect.height * 0.5f);
                GUI.DrawTexture(rect, m_rootIconContent.image);
            }

            if (m_instanceIds == null)
            {
                EditorApplication.hierarchyWindowItemOnGUI -= OnDrawHierachyItem;
                return;
            }


            if (m_instanceIds.TryGetValue(instanceID, out var target) && target)
            {
                var rect = new Rect(selectionRect.x + selectionRect.width - 10, selectionRect.y + 5, 10, 10);
                var objectContent = EditorGUIUtility.ObjectContent(target, target.GetType());
                if (objectContent == null || objectContent.image == null)
                    objectContent = m_iconContent;
                GUI.DrawTexture(rect, objectContent.image);
            }
        }

        private void FindProperty(SerializedProperty property)
        {
            if (m_refsList == null)
            {
                var body = property.serializedObject;
                m_activeId = ((Component)body.targetObject).gameObject.GetInstanceID();
                m_bindingProp = body.FindProperty(fieldInfo.Name);
                m_refsProp = m_bindingProp.FindPropertyRelative("refs");
                CollectProps();
                ApplyPropToCode(true);
                m_refsList = new ReorderableList(property.serializedObject, m_refsProp);
                m_refsList.drawHeaderCallback = OnDrawHead;
                m_refsList.drawElementCallback = OnDrawElements;
                m_refsList.onSelectCallback += OnSelectLine;
            }
        }

        private void CollectProps()
        {
            var targetObj = m_refsProp.serializedObject.targetObject;
            var fields = targetObj.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField);
            var objectFields = fields.Where(x => typeof(UnityEngine.Object).IsAssignableFrom(x.FieldType));
            foreach (var item in objectFields)
            {
                if (!item.IsPublic)
                {
                    var attrs = item.CustomAttributes.Where(x => x.AttributeType == typeof(UnityEngine.SerializeField));
                    if (attrs.Count() == 0)
                        continue;
                }
                var obj = item.GetValue(targetObj);
                var objName = item.Name;
                if (item.Name.StartsWith("m_"))
                {
                    objName = objName.Substring(2);
                }
                AddElementFromObject(objName, obj != null ? (Object)obj : default(Object), false);
            }
        }

        private void OnDrawHead(Rect rect)
        {
            var refRect = new Rect(rect.x, rect.y, 120, rect.height);
            EditorGUI.LabelField(refRect, "[References]");
            if (GUI.Button(refRect, "", EditorStyles.label))
            {
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < m_refsProp.arraySize; i++)
                {
                    sb.AppendLine(PropToCode(m_refsProp.GetArrayElementAtIndex(i)));
                }
                var code = sb.ToString();
                GUIUtility.systemCopyBuffer = code;
            }
            if (refRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                }
                else if (Event.current.type == EventType.DragPerform)
                {
                    foreach (var obj in DragAndDrop.objectReferences)
                    {
                        AddElementFromObject(obj.name, obj, true);
                    }
                }
            }

            if (m_inedit)
            {
                var collectRect = new Rect(rect.x + rect.width - 190, rect.y, 60, rect.height);
                if (GUI.Button(collectRect, "collect", EditorStyles.miniButtonLeft))
                {
                    AutoCollect();
                }
                var clearRect = new Rect(rect.x + rect.width - 120, rect.y, 60, rect.height);
                if (GUI.Button(clearRect, "clear", EditorStyles.miniButtonLeft))
                {
                    m_refsProp.ClearArray();
                }
            }

            var applyRect = new Rect(rect.x + rect.width - 60, rect.y, 60, rect.height);
            if (m_inedit)
            {
                if (GUI.Button(applyRect, "apply", EditorStyles.miniButtonRight))
                {
                    m_inedit = false;
                    ApplyPropToCode(true);
                    AutoWriteCode();
                }
            }
            else
            {
                if (GUI.Button(applyRect, "edit", EditorStyles.miniButtonRight))
                {
                    m_inedit = true;
                    CollectProps();
                }
            }

            if (!m_inedit)
            {
                var allRight = true;
                for (int i = 0; i < m_refsProp.arraySize; i++)
                {
                    var subProp = m_refsProp.GetArrayElementAtIndex(i);
                    var ok = subProp.FindPropertyRelative("obj").objectReferenceValue != null;
                    if (!ok)
                        allRight = false;
                }
                var color = GUI.color;
                GUI.color = allRight ? color : Color.red;
                var infoRect = new Rect(rect.x + 80, rect.y, 100, rect.height);
                EditorGUI.LabelField(infoRect, $"(count = {m_refsProp.arraySize})");
                GUI.color = color;
            }

        }
        /// <summary>
        /// 自动写绑定代码
        /// </summary>
        private void AutoWriteCode()
        {
            if (!(m_bindingProp.serializedObject.targetObject is UIView view))
                return;
            if (!(typeof(UIView).IsAssignableFrom(view.GetType())))
                return;
            if (view.GetType() == typeof(UIView))
                return;
            if (typeof(BindingView).IsAssignableFrom(view.GetType()))
                return;
            var file = MonoScript.FromMonoBehaviour(view);
            if (file == null)
                return;
            var filePath = AssetDatabase.GetAssetPath(file);
            var fileText = System.IO.File.ReadAllText(filePath);
            if (!fileText.Contains(UISetting.Instance.uiViewReginFlag))
                return;
            var targetObj = m_refsProp.serializedObject.targetObject;
            var sb = new StringBuilder();
            for (int i = 0; i < m_refsProp.arraySize; i++)
            {
                sb.AppendLine("\t[SerializeField]");
                sb.Append("\t"); sb.Append(PropToCode2(m_refsProp.GetArrayElementAtIndex(i), false));
            }
            fileText = ReplaceAutoBindingContent(fileText, sb.ToString(), UISetting.Instance.uiViewReginFlag);
            File.WriteAllText(filePath, fileText, Encoding.UTF8);
            AssetDatabase.Refresh();
        }
        static string ReplaceAutoBindingContent(string input, string replacement, string flag)
        {
            // 正则表达式匹配#region AUTO_BINDING 和 #endregion AUTO_BINDING 之间的内容
            string pattern = @"#region " + flag + @"[\s\S]*?#endregion " + flag;

            // 使用正则替换匹配的内容
            return Regex.Replace(input, pattern, match =>
            {
                // 返回#region 和 #endregion 的内容和替换内容
                return $"#region {flag}\n{replacement}#endregion {flag}";
            });
        }

        private void AutoCollect()
        {
            var obj = Selection.activeGameObject;
            if (obj)
            {
                AutoCollectDeepth(obj.transform);
                m_refsProp.serializedObject.ApplyModifiedProperties();
            }
        }

        private void AutoCollectDeepth(Transform parent)
        {
            var autoTypeDict = UISetting.Instance.GetAutoCollectTypes();
            foreach (Transform item in parent)
            {
                if (item == parent)
                    continue;

                var subView = item.GetComponent<UIView>();
                if (subView)
                {
                    AddPropertyToRef(subView);
                    continue;
                }

                AutoCollectDeepth(item);

                if (!Regex.IsMatch(item.name, @"^[\w]+$"))
                    continue;
              
                foreach (var pair in autoTypeDict)
                {
                    if (item.name.EndsWith(pair.Key) || item.name.StartsWith(pair.Key))
                    {
                        if (pair.Value == typeof(GameObject))
                        {
                            AddPropertyToRef(item.gameObject);
                        }
                        else
                        {
                            var component = item.GetComponent(pair.Value);
                            if (component)
                            {
                                AddPropertyToRef(component);
                            }
                        }
                    }
                }
            }
        }

        private void AddPropertyToRef(UnityEngine.Object target)
        {
            bool needAdd = true;
            for (int i = 0; i < m_refsProp.arraySize; i++)
            {
                var prop = m_refsProp.GetArrayElementAtIndex(i);
                var obj = prop.FindPropertyRelative("obj");
                var name = prop.FindPropertyRelative("name");
                if (obj.objectReferenceValue == target)
                {
                    name.stringValue = target.name;
                    needAdd = false;
                }
            }
            if (needAdd)
            {
                m_refsProp.InsertArrayElementAtIndex(m_refsProp.arraySize);
                var prop = m_refsProp.GetArrayElementAtIndex(m_refsProp.arraySize - 1);
                var obj = prop.FindPropertyRelative("obj");
                var name = prop.FindPropertyRelative("name");
                obj.objectReferenceValue = target;
                name.stringValue = target.name;
            }
        }

        private void ApplyPropToCode(bool log)
        {
            var targetObj = m_refsProp.serializedObject.targetObject;
            var sb = new StringBuilder();
            for (int i = 0; i < m_refsProp.arraySize; i++)
            {
                var prop = m_refsProp.GetArrayElementAtIndex(i);
                var fieldName = prop.FindPropertyRelative("name").stringValue;
                var obj = prop.FindPropertyRelative("obj").objectReferenceValue;
                var field = targetObj.GetType().GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField);
                if (field == null)
                    field = targetObj.GetType().GetField("m_" + fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField);
                if (field != null && obj != null && field.FieldType.IsAssignableFrom(obj.GetType()))
                {
                    field.SetValue(targetObj, obj);
                    EditorUtility.SetDirty(targetObj);
                }
                else
                {
                    sb.AppendLine(PropToCode2(m_refsProp.GetArrayElementAtIndex(i)));
                }
                if (obj is GameObject)
                {
                    m_instanceIds.TryAdd(obj.GetInstanceID(), obj);
                }
                else if (obj is Component)
                {
                    m_instanceIds.TryAdd(((Component)obj).gameObject.GetInstanceID(), obj);
                }
            }
            var code = sb.ToString();
            if (log && !string.IsNullOrEmpty(code))
            {
                GUIUtility.systemCopyBuffer = code;
                Debug.Log("copy to clipboard:" + code);
            }
        }

        private void AddElementFromObject(string objectName, Object obj, bool selectComponent)
        {
            m_refsProp.serializedObject.Update();
            SerializedProperty element = null;
            for (int i = 0; i < m_refsProp.arraySize; i++)
            {
                var prop = m_refsProp.GetArrayElementAtIndex(i);
                if (prop.FindPropertyRelative("name").stringValue == objectName)
                {
                    element = prop;
                    break;
                }
            }
            if (element == null)
            {
                m_refsProp.InsertArrayElementAtIndex(Mathf.Max(0, m_refsProp.arraySize - 1));
                element = m_refsProp.GetArrayElementAtIndex(m_refsProp.arraySize - 1);
            }
            var nameProp = element.FindPropertyRelative("name");
            var objProp = element.FindPropertyRelative("obj");
            nameProp.stringValue = objectName;
            if (obj)
            {
                if (selectComponent && obj is GameObject)
                    obj = SelectBestObject(obj as GameObject);
                objProp.objectReferenceValue = obj;
            }
            m_refsProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        public static Object SelectBestObject(GameObject obj)
        {
            Object behaviour = obj;
            var behaviours = obj.GetComponents<Behaviour>();
            if (behaviours.Length > 0)
            {
                bool existSelectable = false;
                bool existLayout = false;
                foreach (var item in behaviours)
                {
                    if (behaviour == null)
                    {
                        behaviour = item;
                        continue;
                    }

                    var itemNameSpace = item.GetType().Namespace;
                    if (string.IsNullOrEmpty(itemNameSpace) || !itemNameSpace.StartsWith("UnityEngine"))
                    {
                        behaviour = item;
                        break;
                    }

                    if (item is UnityEngine.UI.Selectable || item is UnityEngine.EventSystems.IDragHandler)
                    {
                        behaviour = item;
                        existSelectable = true;
                        continue;
                    }

                    if (item is ILayoutGroup && !existSelectable)
                    {
                        behaviour = item;
                        existLayout = true;
                        continue;
                    }

                    if (item is UnityEngine.UI.Graphic && !existSelectable && !existLayout)
                    {
                        behaviour = item;
                        continue;
                    }
                }
            }
            return behaviour;
        }

        private void OnSelectLine(ReorderableList list)
        {
            var prop = m_refsProp.GetArrayElementAtIndex(list.index);
            var code = PropToCode2(prop);
            GUIUtility.systemCopyBuffer = code;
            Debug.Log(code);
        }

        private string PropToCode(SerializedProperty prop)
        {
            var nameProp = prop.FindPropertyRelative("name");
            var objProp = prop.FindPropertyRelative("obj");
            var typeName = "GameObject";
            if (objProp.objectReferenceValue != null)
                typeName = objProp.objectReferenceValue.GetType().Name;
            return $"protected {typeName} m_{nameProp.stringValue} => GetRef<{typeName}>(\"{nameProp.stringValue}\");";
        }


        private string PropToCode2(SerializedProperty prop, bool writeAttr = true)
        {
            var nameProp = prop.FindPropertyRelative("name");
            var objProp = prop.FindPropertyRelative("obj");
            var typeName = "GameObject";
            if (objProp.objectReferenceValue != null)
                typeName = objProp.objectReferenceValue.GetType().FullName;
            var sb = new System.Text.StringBuilder();
            if (writeAttr) sb.AppendLine("[SerializeField]");
            sb.AppendLine($"protected {typeName} m_{nameProp.stringValue};");
            return sb.ToString();
        }
        private void OnDrawElements(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index < m_refsProp.arraySize)
            {
                var elementProp = m_refsProp.GetArrayElementAtIndex(index);
                var nameProp = elementProp.FindPropertyRelative("name");
                var objProp = elementProp.FindPropertyRelative("obj");
                var nameRect = new Rect(rect.x + 15, rect.y + 5, rect.width * 0.25f, rect.height - 10);
                nameProp.stringValue = EditorGUI.TextField(nameRect, nameProp.stringValue, EditorStyles.miniTextField);
                var objRect = new Rect(rect.x + 20 + rect.width * 0.25f, rect.y + 5, rect.width * 0.35f, rect.height - 10);
                objProp.objectReferenceValue = EditorGUI.ObjectField(objRect, objProp.objectReferenceValue, typeof(UnityEngine.Object), true);
                var nameTitleRect = new Rect(rect.x, rect.y + 5, 20, rect.height - 10);
                EditorGUI.LabelField(nameTitleRect, $"{index + 1}:");

                if (objProp.objectReferenceValue is GameObject || objProp.objectReferenceValue is Component)
                {
                    var changeTypeRect = new Rect(rect.x + rect.width * 0.7f, rect.y + 1, rect.width * 0.4f, rect.height);
                    if (GUI.Button(changeTypeRect, objProp.objectReferenceValue.GetType().Name, EditorStyles.linkLabel))
                    {
                        GenericMenu menu = new GenericMenu();
                        var types = GetTypes(objProp.objectReferenceValue);
                        foreach (var type in types)
                        {
                            var selected = objProp.objectReferenceValue.GetType() == type;
                            menu.AddItem(new GUIContent(type.FullName), selected, (typeNow) =>
                            {
                                ChangeObjectRef(objProp, (Type)typeNow);
                            }, type);
                        }
                        menu.ShowAsContext();
                    }
                }
            }
        }

        private Type[] GetTypes(Object obj)
        {
            var list = new System.Collections.Generic.List<Type>();
            list.Add(typeof(GameObject));
            Component[] comps = null;
            if (obj is GameObject)
            {
                var go = obj as GameObject;
                comps = go.GetComponents<Component>();
            }
            else
            {
                var go = obj as Component;
                comps = go.GetComponents<Component>();
            }
            list.AddRange(comps.Select(x => x.GetType()).ToArray());
            return list.ToArray();
        }

        private void ChangeObjectRef(SerializedProperty prop, Type type)
        {
            if (prop.objectReferenceValue.GetType() == type)
                return;
            if (prop.objectReferenceValue is GameObject)
            {
                var comp = (prop.objectReferenceValue as GameObject).GetComponent(type);
                prop.objectReferenceValue = comp;
            }
            else
            {
                var comObj = (prop.objectReferenceValue as Component);
                if (type == typeof(GameObject))
                {
                    prop.objectReferenceValue = comObj.gameObject;
                }
                else
                {
                    var comp = comObj.GetComponent(type);
                    prop.objectReferenceValue = comp;
                }
            }
            prop.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            FindProperty(property);
            if (!m_inedit)
                return EditorGUIUtility.singleLineHeight;
            return m_refsList.GetHeight();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.serializedObject.Update();
            if (m_inedit)
            {
                m_refsList.DoList(position);
            }
            else
            {
                GUI.Box(position, "");
                OnDrawHead(position);
            }
            property.serializedObject.ApplyModifiedProperties();
        }

        [InitializeOnLoadMethod]
        private static void CheckAutoAddComponent()
        {
            if (EditorPrefs.HasKey(m_autoAddComponentKey) && Selection.activeGameObject)
            {
                var file = EditorPrefs.GetString(m_autoAddComponentKey);
                if (File.Exists(file))
                {
                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(file);
                    if (script)
                    {
                        Selection.activeGameObject.AddComponent(script.GetClass());
                    }
                }
                EditorPrefs.DeleteKey(m_autoAddComponentKey);
            }
        }
    }
}
