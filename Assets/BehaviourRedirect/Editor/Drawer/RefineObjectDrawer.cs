using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Reflection;

namespace ScriptRefine
{
    [CustomEditor(typeof(RefineObj))]
    public class RefineObjectDrawer : Editor
    {
        SerializedProperty scriptProp;
        SerializedProperty refineListProp;
        SerializedProperty ignoreFolderProp;
        SerializedProperty ignoreNameSpaceProp;
        SerializedProperty ignoreTypeProp;
        RefineObj refineObj;
        SerializedProperty refineListProp_w;
        SerializedProperty ignoreFolderProp_w;
        SerializedProperty ignoreNameSpaceProp_w;
        SerializedProperty ignoreTypeProp_w;
        string match;
        const string preferPath = "refineExporptPath";
        string[] options = { "开始配制", "~文件夹", "~命名空间", "~类型" };
        int index = 0;
        private void OnEnable()
        {
            scriptProp = serializedObject.FindProperty("m_Script");
            refineListProp = serializedObject.FindProperty("refineList");
            ignoreFolderProp = serializedObject.FindProperty("ignoreFolder");
            ignoreNameSpaceProp = serializedObject.FindProperty("ignoreNameSpace");
            ignoreTypeProp = serializedObject.FindProperty("ignoreType");
            refineObj = target as RefineObj;
            var _refineObj_w = ScriptableObject.CreateInstance<RefineObj>();
            var ws = new SerializedObject(_refineObj_w);
            refineListProp_w = ws.FindProperty("refineList");
            ignoreFolderProp_w = serializedObject.FindProperty("ignoreFolder");
            ignoreNameSpaceProp_w = serializedObject.FindProperty("ignoreNameSpace");
            ignoreTypeProp_w = serializedObject.FindProperty("ignoreType");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(scriptProp);
            EditorGUILayout.ObjectField(refineObj, typeof(RefineObj), false);
            DrawToolBarsAndOptions();
            DrawMatchField();
            DrawInfosByIndex();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawMatchField()
        {
            EditorGUI.BeginChangeCheck();
            match = EditorGUILayout.TextField(match, EditorStyles.textField);
            if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(match))
            {
                refineListProp_w.ClearArray();
                for (int i = 0; i < refineListProp.arraySize; i++)
                {
                    var prop = refineListProp.GetArrayElementAtIndex(i);
                    if (prop.FindPropertyRelative("type").stringValue.ToLower().Contains(match.ToLower()))
                    {
                        refineListProp_w.InsertArrayElementAtIndex(0);
                        RefineUtility.CopyPropertyValue(refineListProp_w.GetArrayElementAtIndex(0), prop);
                    }
                }
            }
        }

        private void DrawRefineList()
        {
            List<KeyValuePair<string[], UnityAction>> options = new List<KeyValuePair<string[], UnityAction>>()
        {
            new KeyValuePair<string[], UnityAction>(new string[] {"import", "脚本信息读取"},()=> {
                TryLoadFromSelection();
                refineObj.refineList.Sort();
            }),
             new KeyValuePair<string[], UnityAction>(new string[] {"exprot", "生成脚本并导出"},()=> {
                 ExportWorpScripts();
            }),
              new KeyValuePair<string[], UnityAction>(new string[] {"clean", "清空信息列表"},()=> {
               refineListProp.ClearArray();
            }),
        };

            DrawButtonsInnternal(options);

            SerializedProperty refineListProp_current = null;

            if (string.IsNullOrEmpty(match))
            {
                refineListProp_current = refineListProp;
            }
            else
            {
                refineListProp_current = refineListProp_w;
            }

            DrawListProp(refineListProp_current);
        }

        private void DrawListProp(SerializedProperty property)
        {
            for (int i = 0; i < property.arraySize; i++)
            {
                var prop = property.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(prop);
            }
        }

        private void DrawToolBarsAndOptions()
        {
            index = GUILayout.Toolbar(index, options, EditorStyles.toolbarButton);
        }
        private void DrawInfosByIndex()
        {
            if (index == 0)
            {
                DrawRefineList();
            }
            else if (index == 1)
            {
                DrawIgnoreFolder();
            }
            else if (index == 2)
            {
                DrawIgnoreNameSpace();
            }
            else if (index == 3)
            {
                DrawIgnoreTypes();
            }
        }

        private void DrawIgnoreFolder()
        {
            List<KeyValuePair<string[], UnityAction>> options = new List<KeyValuePair<string[], UnityAction>>()
        {
            new KeyValuePair<string[], UnityAction>(new string[] {"select", "添加选中文件夹"},()=> {
                if(Selection.instanceIDs!= null && Selection.instanceIDs.Length > 0)
                {
                    foreach (var item in Selection.instanceIDs)
                    {
                        if(ProjectWindowUtil.IsFolder(item))
                        {
                            var path = AssetDatabase.GetAssetPath(item);
                            if(refineObj.ignoreFolder.Find(x=>path.Contains(x)) == null){
                                refineObj.ignoreFolder.RemoveAll(x=>x.Contains(path));
                                refineObj.ignoreFolder.Add(path);
                            }
                        }
                    }
                    EditorUtility.SetDirty(refineObj);
                }
            }),
              new KeyValuePair<string[], UnityAction>(new string[] {"clean", "清空文件夹列表"},()=> {
                  ignoreFolderProp.ClearArray();
            }),
        };

            DrawButtonsInnternal(options);

            var prop = string.IsNullOrEmpty(match) ? ignoreFolderProp : ignoreFolderProp_w;

            for (int i = 0; i < prop.arraySize; i++)
            {
                var item = prop.GetArrayElementAtIndex(i);
                using (var hor = new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(item.stringValue);
                    if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(20)))
                    {
                        prop.DeleteArrayElementAtIndex(i);
                        return;
                    }
                }
            }
        }

        private void DrawIgnoreNameSpace()
        {
            List<KeyValuePair<string[], UnityAction>> options = new List<KeyValuePair<string[], UnityAction>>()
        {
            new KeyValuePair<string[], UnityAction>(new string[] {"add", "添加一项目"},()=> {
                var newNamespace = "";
                if(Selection.activeObject != null && Selection.activeObject is MonoScript)
                {
                    var nameSpace = (Selection.activeObject as MonoScript).GetClass().Namespace;
                    newNamespace = nameSpace == null ? "": nameSpace.ToString();
                }
                if(!refineObj.ignoreNameSpace.Contains(newNamespace))
                {
                    refineObj.ignoreNameSpace.Add(newNamespace);
                }
                EditorUtility.SetDirty(refineObj);
            }),
              new KeyValuePair<string[], UnityAction>(new string[] {"clean", "清空文件夹列表"},()=> {
                  ignoreNameSpaceProp.ClearArray();
            }),
        };

            DrawButtonsInnternal(options);

            var prop = string.IsNullOrEmpty(match) ? ignoreNameSpaceProp : ignoreNameSpaceProp_w;

            for (int i = 0; i < prop.arraySize; i++)
            {
                var item = prop.GetArrayElementAtIndex(i);
                using (var hor = new EditorGUILayout.HorizontalScope())
                {
                    item.stringValue = EditorGUILayout.TextField(item.stringValue);
                    if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(20)))
                    {
                        prop.DeleteArrayElementAtIndex(i);
                        return;
                    }
                }
            }
        }

        private void DrawIgnoreTypes()
        {
            List<KeyValuePair<string[], UnityAction>> options = new List<KeyValuePair<string[], UnityAction>>()
        {
            new KeyValuePair<string[], UnityAction>(new string[] {"add", "添加一项目"},()=> {
                string newType = "";
                if (Selection.activeObject != null && Selection.activeObject is MonoScript)
                {
                   newType = (Selection.activeObject as MonoScript).GetClass().ToString();
                }
                if(!refineObj.ignoreType.Contains(newType))
                {
                    refineObj.ignoreType.Add(newType);
                }
                EditorUtility.SetDirty(refineObj);
            }),
              new KeyValuePair<string[], UnityAction>(new string[] {"clean", "清空文件夹列表"},()=> {
                  ignoreTypeProp.ClearArray();
            }),
        };

            DrawButtonsInnternal(options);

            var prop = string.IsNullOrEmpty(match) ? ignoreTypeProp : ignoreTypeProp_w;

            for (int i = 0; i < prop.arraySize; i++)
            {
                var item = prop.GetArrayElementAtIndex(i);
                using (var hor = new EditorGUILayout.HorizontalScope())
                {
                    item.stringValue = EditorGUILayout.TextField(item.stringValue);
                    if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(20)))
                    {
                        prop.DeleteArrayElementAtIndex(i);
                        return;
                    }
                }
            }
        }

        private void DrawButtonsInnternal(List<KeyValuePair<string[], UnityAction>> options)
        {
            if (options == null || options.Count == 0) return;

            var btnStyles = EditorStyles.radioButton;
            var layout = GUILayout.Width(EditorGUIUtility.currentViewWidth / options.Count);
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                foreach (var item in options)
                {
                    if (item.Key == null || item.Key.Length != 2) continue;

                    var btnName = item.Key[0];
                    var btnInfo = item.Key[1];
                    var action = item.Value;
                    if (GUILayout.Button(new GUIContent(btnName, btnInfo), btnStyles, layout))
                    {
                        if (action != null) action.Invoke();
                    }
                }
            }
        }

        private void TryLoadFromSelection()
        {
            if (Selection.objects.Length > 0)
            {
                var list = new List<MonoScript>();
                foreach (var item in Selection.objects)
                {
                    if (item is ScriptableObject)
                    {
                        TryLoadFromScriptObject(item as ScriptableObject, list);
                    }
                    else if (item is MonoScript)
                    {
                        TryLoadSingleScript(item as MonoScript, list);
                    }
                    else if (item is GameObject)
                    {
                        TryLoadScriptsFromPrefab(item as GameObject, list);
                    }
                    else if (ProjectWindowUtil.IsFolder(item.GetInstanceID()))
                    {
                        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
                        TryLoadFromFolder(path, list);
                    }
                }
                OnLoadMonoScript(list.ToArray());
                EditorUtility.SetDirty(refineObj);
            }
        }

        /// <summary>
        /// 生成worp脚本
        /// </summary>
        private void ExportWorpScripts()
        {
            var oldPath = PlayerPrefs.GetString(preferPath);
            var folder = EditorUtility.SaveFolderPanel("选择导出路径", oldPath, "");
            if (!string.IsNullOrEmpty(folder))
            {
                PlayerPrefs.SetString(preferPath, folder);
                RefineUtility.ExportScripts(folder, refineObj.refineList);
            }
        }

        /// <summary>
        /// 将scriptObject用到的脚本读取到列表中
        /// </summary>
        private void TryLoadFromScriptObject(ScriptableObject scriptObj, List<MonoScript> scriptList)
        {
            var mono = MonoScript.FromScriptableObject(scriptObj);
            if (RefineUtility.IsMonoBehaiverOrScriptObjectRuntime(mono))
            {
                scriptList.Add(mono);
            }
        }


        /// <summary>
        /// 直接读取一个脚本
        /// </summary>
        private void TryLoadSingleScript(MonoScript script, List<MonoScript> scriptList)
        {
            if (RefineUtility.IsMonoBehaiverOrScriptObjectRuntime(script))
            {
                scriptList.Add(script);
            }
        }

        /// <summary>
        /// 从文件夹读取
        /// </summary>
        private void TryLoadFromFolder(string path, List<MonoScript> scriptList)
        {
            var files = System.IO.Directory.GetFiles(path, "*.*", System.IO.SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var dirPath = file.Replace("\\", "/").Replace(Application.dataPath, "Assets");

                if (refineObj.ignoreFolder.Find(x => file.Contains(x)) != null) continue;

                if (file.EndsWith(".cs"))
                {
                    var mono = AssetDatabase.LoadAssetAtPath<MonoScript>(dirPath);
                    if (mono != null)
                    {
                        TryLoadSingleScript(mono, scriptList);
                    }
                }
                else if (file.EndsWith(".asset"))
                {
                    var scriptObj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(dirPath);
                    if (scriptObj != null)
                    {
                        TryLoadFromScriptObject(scriptObj, scriptList);
                    }
                }
                else if (file.EndsWith(".prefab"))
                {
                    var gameObj = AssetDatabase.LoadAssetAtPath<GameObject>(dirPath);
                    if (gameObj != null)
                    {
                        TryLoadScriptsFromPrefab(gameObj, scriptList);
                    }
                }

            }
        }


        /// <summary>
        /// 从预制体身上加载脚本
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="types"></param>
        private void TryLoadScriptsFromPrefab(GameObject go, List<MonoScript> behaivers)
        {
            if (go == null) return;
            var trans = go.transform;
            var behaiver = trans.GetComponents<MonoBehaviour>();
            if (behaiver != null)
            {
                //var monos = new MonoScript[behaiver.Length];
                var monos = new List<MonoScript>();
                for (int i = 0; i < behaiver.Length; i++)
                {
                    if (behaiver[i] == null)
                    {
                        Debug.Log(trans.name + ":scriptMissing", go);
                    }
                    else
                    {
                        var mono = MonoScript.FromMonoBehaviour(behaiver[i]);
                        if (RefineUtility.IsMonoBehaiverOrScriptObjectRuntime(mono))
                        {
                            monos.Add(mono);
                        }
                    }
                }
                behaivers.AddRange(monos);
            }
            if (trans.childCount == 0)
            {
                return;
            }
            else
            {
                for (int i = 0; i < trans.childCount; i++)
                {
                    var childTrans = trans.GetChild(i);
                    TryLoadScriptsFromPrefab(childTrans.gameObject, behaivers);
                }
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="monos"></param>
        private void OnLoadMonoScript(params MonoScript[] monos)
        {
            foreach (var item in monos)
            {
                if (item == null || item.GetClass() == null) continue;

                var old = refineObj.refineList.Find(x => x.type == item.GetClass().ToString());
                if (old == null)
                {
                    var refineItem = new RefineItem(item);
                    refineObj.refineList.Add(refineItem);
                    LoopInsertItem(refineItem, refineObj.refineList);
                }
                else
                {
                    old.Update(item);
                }
            }
        }


        private bool IsIgnored(Type type)
        {
            var nameSpace = type.Namespace;
            if (refineObj.ignoreType.Contains(type.ToString()))
            {
                return false;
            }
            if (refineObj.ignoreNameSpace.Contains(nameSpace))
            {
                return false;
            }
            return true;
        }

        private void LoopInsertItem(RefineItem item, List<RefineItem> refineList)
        {
            //遍历参数
            foreach (var arg in item.arguments)
            {
                if (!string.IsNullOrEmpty(arg.subType))
                {
                    var type = Assembly.Load(arg.subAssemble).GetType(arg.subType);

                    if (type == null) continue;

                    if (type.IsGenericType)
                    {
                        type = type.GetGenericArguments()[0];
                    }
                    else if (type.IsArray)
                    {
                        type = type.GetElementType();
                    }

                    if (RefineUtility.IsInternalScript(type)) continue;
                    if (IsIgnored(type)) continue;

                    var old = refineObj.refineList.Find(x => x.type == type.ToString());
                    if (old == null)
                    {
                        var refineItem = new RefineItem(type);
                        refineObj.refineList.Add(refineItem);
                        LoopInsertItem(refineItem, refineList);
                    }
                    else
                    {
                        old.Update(type);
                    }
                }
            }

            var currentType = Assembly.Load(item.assemble).GetType(item.type);
            if (currentType == null)
            {
                currentType = Type.GetType(item.type);
            }
            if (currentType == null)
            {
                Debug.Log(item.type + ": load empty");
                return;
            }

            //遍历泛型类
            if (currentType.IsGenericType)
            {
                var gtypes = currentType.GetGenericArguments();
                foreach (var gtype in gtypes)
                {
                    if (RefineUtility.IsInternalScript(gtype)) continue;
                    if (IsIgnored(gtype)) continue;

                    var old = refineObj.refineList.Find(x => x.type == gtype.ToString());
                    if (old == null)
                    {
                        var refineItem = new RefineItem(gtype);
                        refineObj.refineList.Add(refineItem);
                        LoopInsertItem(refineItem, refineObj.refineList);
                    }
                    else
                    {
                        old.Update(gtype);
                    }
                }
            }

            //遍历类父级
            while (currentType != null && currentType.BaseType != null)
            {
                currentType = currentType.BaseType;

                if (RefineUtility.IsInternalScript(currentType)) continue;
                if (IsIgnored(currentType)) continue;

                var old = refineObj.refineList.Find(x => x.type == currentType.ToString());
                if (old == null)
                {
                    var refineItem = new RefineItem(currentType);
                    refineObj.refineList.Add(refineItem);
                    LoopInsertItem(refineItem, refineObj.refineList);
                }
                else
                {
                    old.Update(currentType);
                }
            }
        }
    }
}