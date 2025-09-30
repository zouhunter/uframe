//*************************************************************************************
//* 作    者： 邹杭特
//* 创建时间： 2021-08-19 04:29:14
//* 描    述： 代码链接器

//* ************************************************************************************
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using Debug = UnityEngine.Debug;

namespace UFrame.CodeLink
{
    /// <summary>
    /// 类
    /// <summary>
    public class CodeLinkWindow : EditorWindow
    {
        [System.Serializable]
        public class ModuleItem
        {
            public string name;
            public List<string> src = new List<string>();
            public List<string> submodules = new List<string>();
        }

        [System.Serializable]
        public class ModuleMap
        {
            public string name;
            public string path;
            public List<ModuleItem> modules = new List<ModuleItem>();
        }

        [SerializeField]
        protected string m_localFolder;
        protected SerializedProperty m_scriptProp;
        protected List<ModuleItem> m_activeModuleItems = new List<ModuleItem>();
        protected List<ModuleMap> m_moduleMaps = new List<ModuleMap>();
        [SerializeField]
        protected string m_activeModuleName;
        protected string m_srcFolder;
        [SerializeField]
        protected List<string> m_modulePaths = new List<string>();

        protected ReorderableList m_reorderList;
        protected string key_pref = "UFrame.CodeLink.CodeLinkWindow";
        protected Vector2 m_scrollPos;
        protected string m_matchText;

        [MenuItem("Window/UFrame/CodeLink-Editor")]
        private static void OpenWindow()
        {
            GetWindow<CodeLinkWindow>("CodeLinkWindow");
        }

        private void OnEnable()
        {
            var jsonText = EditorPrefs.GetString(key_pref);
            if (!string.IsNullOrEmpty(jsonText))
            {
                EditorJsonUtility.FromJsonOverwrite(jsonText, this);
            }
            m_scriptProp = new SerializedObject(this).FindProperty("m_Script");
            LoadModuleFiles();
            UpdateModuleViewList();
            InitListView();
        }

        private void OnDisable()
        {
            var jsonText = EditorJsonUtility.ToJson(this);
            EditorPrefs.SetString(key_pref, jsonText);
        }

        public void OnGUI()
        {
            if(m_reorderList == null)
            {
                return;
            }

            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                using (var disableProp = new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField(m_scriptProp.objectReferenceValue, typeof(MonoScript), false, GUILayout.Width(160));
                }
                GUILayout.Label("本地路径:", GUILayout.Width(60));
                m_localFolder = GUILayout.TextField(m_localFolder);
                if (GUILayout.Button("选择", GUILayout.Width(40)))
                {
                    var result = EditorUtility.OpenFolderPanel("选择本地文件路径", m_localFolder, "");
                    if (!string.IsNullOrEmpty(result))
                    {
                        m_localFolder = result.Replace("\\", "/").Replace(Application.dataPath, "Assets");
                    }
                }
            }
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                using (var ver = new EditorGUILayout.VerticalScope(GUILayout.Width(160)))
                {
                    for (int i = 0; i < m_moduleMaps.Count; i++)
                    {
                        var module = m_moduleMaps[i];
                        var style = module.name == m_activeModuleName ? EditorStyles.toolbarPopup : EditorStyles.toolbarButton;
                        if (GUILayout.Button(module.name, style, GUILayout.Width(140)))
                        {
                            m_activeModuleName = module.name;
                            UpdateModuleViewList();
                        }
                        var lastRect = GUILayoutUtility.GetLastRect();
                        var deleteRect = new Rect(lastRect.x + lastRect.width, lastRect.y, 20, lastRect.height);
                        if (GUI.Button(deleteRect, "x"))
                        {
                            m_modulePaths.RemoveAt(i);
                            m_moduleMaps.RemoveAt(i);
                            UpdateModuleViewList();
                        }
                        GUILayout.Space(2);
                    }
                    if (GUILayout.Button("添加", EditorStyles.toolbarButton))
                    {
                        var srcPath = EditorUtility.OpenFilePanel("选择配制文件", m_srcFolder, "json");
                        if (!string.IsNullOrEmpty(srcPath))
                        {
                            if (m_modulePaths.Find(x => x == srcPath) == null)
                            {
                                m_modulePaths.Add(srcPath);
                            }
                            LoadModules(srcPath);
                        }
                    }
                }
                using (var vert = new EditorGUILayout.VerticalScope())
                {
                    using (var scro = new EditorGUILayout.ScrollViewScope(m_scrollPos))
                    {
                        m_scrollPos = scro.scrollPosition;
                        m_reorderList.DoLayoutList();
                    }
                    EditorGUILayout.SelectableLabel("路径:" + m_srcFolder, EditorStyles.miniLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                }
            }
        }

        protected void InitListView()
        {
            m_reorderList = new ReorderableList(m_activeModuleItems, typeof(ModuleItem));
            m_reorderList.displayAdd = false;
            m_reorderList.displayRemove = false;
            m_reorderList.drawHeaderCallback = OnDrawHead;
            m_reorderList.elementHeight = EditorGUIUtility.singleLineHeight + 4;
            m_reorderList.drawElementCallback = OnDrawModuleItem;
        }

        private void OnDrawHead(Rect rect)
        {
            var labelRect = new Rect(rect.x, rect.y, 200, rect.height);
            EditorGUI.LabelField(labelRect, string.Format("【{0}】", m_activeModuleName));
            var pathRect = new Rect(rect.x + rect.width - 70, rect.y + 1, 60, rect.height - 2);
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                m_matchText = EditorGUI.TextField(pathRect, m_matchText);
                if (changeScope.changed)
                {
                    UpdateModuleViewList();
                }
            }
        }

        private void OnDrawModuleItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (m_activeModuleItems.Count <= index || index < 0)
                return;

            GUI.Box(new Rect(rect.x, rect.y, rect.width, rect.height - 2f), "");
            var idRect = new Rect(rect.x, rect.y, 30, rect.height);
            var idLabel = (index + 1).ToString("000") + ".";
            var nameRect = new Rect(rect.x + 30, rect.y, rect.width - 160, rect.height - 2);
            var activeModuleItem = m_activeModuleItems[index];
            if (GUI.Button(idRect, idLabel, EditorStyles.miniBoldLabel))
            {
                if (activeModuleItem.src.Count > 0)
                {
                    var folder = System.IO.Path.GetDirectoryName(activeModuleItem.src[0]);
#if UNITY_EDITOR_WIN
                    OpenSelectFolder(folder);
#endif
                }
            }

            EditorGUI.LabelField(nameRect, string.Format("{0}:{1}", activeModuleItem.name, activeModuleItem.src.Count));

            var linkRect = new Rect(rect.x + rect.width - 60, rect.y + 1, 55, rect.height - 2);
            if (GUI.Button(linkRect, "链接", EditorStyles.miniButtonRight))
            {
                var moduleLinked = new HashSet<string>() { string.Format("{0}:{1}", m_activeModuleName, activeModuleItem.name) };
                DoConnectAction(m_srcFolder, m_activeModuleName, activeModuleItem, moduleLinked, (targetPath, originPath) =>
                 {
                     if (System.IO.Directory.Exists(originPath))
                     {
#if UNITY_EDITOR_WIN
                         var cmd = "mklink /J " + string.Format("\"{0}\" \"{1}\"", targetPath.Replace("/", "\\"), originPath.Replace("/", "\\"));
                         RunProgram("cmd.exe", cmd);
#else
                        var cmd = "link " + string.Format("\"{0}\" \"{1}\"", targetPath.Replace("\\","/"), originPath.Replace("\\", "/"));
                        RunProgram("", cmd);
#endif
                     }
                     else if (System.IO.File.Exists(originPath))
                     {
                         CopyFileOrDirectory(originPath, targetPath);
                         Debug.LogWarning("file copyed from:" + originPath + " to:" + targetPath);
                     }
                     else
                     {
                         Debug.LogError("failed connect from:" + originPath + " to:" + targetPath);
                     }
                 });
            }
            var copyRect = new Rect(rect.x + rect.width - 120, rect.y + 1, 55, rect.height - 2);
            if (GUI.Button(copyRect, "拷贝", EditorStyles.miniButton))
            {
                var moduleLinked = new HashSet<string>() { string.Format("{0}:{1}", m_activeModuleName, activeModuleItem.name) };
                DoConnectAction(m_srcFolder, m_activeModuleName, activeModuleItem, moduleLinked, (targetPath, originPath) =>
                {
                    if (System.IO.Directory.Exists(originPath) || System.IO.File.Exists(originPath))
                    {
                        CopyFileOrDirectory(originPath, targetPath);
                    }
                    else
                    {
                        Debug.LogWarning("failed copy from:" + originPath + " to:" + targetPath);
                    }
                });
            }
        }

        //拷贝文件夹或文件
        protected void CopyFileOrDirectory(string source, string target)
        {
            if (System.IO.Directory.Exists(source))
            {
                source = source.Replace("\\", "/");
                target = target.Replace("\\", "/");
                var files = System.IO.Directory.GetFiles(source, "*", System.IO.SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    var sourceFilePath = files[i];
                    var relPath = files[i].Replace("\\", "/").Replace(source, "");
                    var targetFilePath = target + relPath;
                    //Debug.LogFormat("target:{0} relPath:{1} targetFilePath:{2}", target, relPath, targetFilePath);
                    CopyFileOrDirectory(sourceFilePath, targetFilePath);
                }
            }
            else if (System.IO.File.Exists(source))
            {
                var targetDir = System.IO.Path.GetDirectoryName(target);
                if (!System.IO.Directory.Exists(targetDir))
                {
                    System.IO.Directory.CreateDirectory(targetDir);
                }
                try
                {
                    Debug.LogFormat("file copy from:{0} to {1}", source, target);
                    System.IO.File.Copy(source, target, true);
                    var sourceMetaFilePath = source + ".meta";
                    if (System.IO.File.Exists(sourceMetaFilePath))
                    {
                        var targetMetaFile = target + ".meta";
                        System.IO.File.Copy(sourceMetaFilePath, targetMetaFile, true);
                    }
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        protected void DoConnectAction(string sourceFoder,string activeModuleName,ModuleItem activeModule, HashSet<string> moduleLinked, System.Action<string, string> connectAction)
        {
            var targetFolder = System.IO.Path.Combine(m_localFolder, activeModuleName);
            for (int i = 0; i < activeModule.src.Count; i++)
            {
                var relPath = activeModule.src[i];
                var targetPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(targetFolder, relPath));
                var folder = System.IO.Path.GetDirectoryName(targetPath);
                if (!System.IO.Directory.Exists(folder))
                {
                    System.IO.Directory.CreateDirectory(folder);
                }
                var originPath = System.IO.Path.Combine(sourceFoder, relPath);
                connectAction(targetPath, originPath);
            }

            for (int i = 0; i < activeModule.submodules.Count; i++)
            {
                var moduleItemName = activeModule.submodules[i];
                if (moduleLinked.Contains(moduleItemName))
                    continue;

                var subModuleName = "";
                var currentActiveModuleName = activeModuleName;
                var currentSourceFolder = sourceFoder;
                var subModule = m_activeModuleItems.Find(x => x.name == moduleItemName);
                if (subModule == null && moduleItemName.Contains(":"))
                {
                    var pair = moduleItemName.Split(':');
                    var moduleNameSpace = pair[0];
                    var moduleName = pair[1];

                    var moduleMap = m_moduleMaps.Find(x => x.name == moduleNameSpace);
                    if (moduleMap != null)
                    {
                        currentSourceFolder = moduleMap.path;
                        currentActiveModuleName = moduleMap.name;
                        subModule = moduleMap.modules.Find(x => x.name == moduleName);
                    }
                    else
                    {
                        Debug.LogError("failed find moduleMap:" + moduleNameSpace);
                        continue;
                    }
                    subModuleName = moduleItemName;
                }
                else if (subModule != null)
                {
                    subModuleName = string.Format("{0}:{1}", activeModule.name, moduleItemName);
                }

                if (subModule != null)
                {
                    moduleLinked.Add(subModuleName);
                    DoConnectAction(currentSourceFolder, currentActiveModuleName, subModule, moduleLinked, connectAction);
                }
            }
        }

        protected void LoadModuleFiles()
        {
            m_moduleMaps.Clear();
            for (int i = 0; i < m_modulePaths.Count; i++)
            {
                var path = m_modulePaths[i];
                if (!System.IO.File.Exists(path))
                    continue;
                var jsonText = System.IO.File.ReadAllText(path);
                var moduleMap = JsonUtility.FromJson<ModuleMap>(jsonText);
                if (moduleMap != null)
                {
                    moduleMap.path = System.IO.Path.GetDirectoryName(path);
                    m_moduleMaps.Add(moduleMap);
                }
            }
        }

        protected void UpdateModuleViewList()
        {
            m_activeModuleItems.Clear();
            m_srcFolder = "";
            if (m_moduleMaps != null && m_moduleMaps.Count > 0)
            {
                ModuleMap activeMap = null;
                if (!string.IsNullOrEmpty(m_activeModuleName))
                {
                    activeMap = m_moduleMaps.Find(x => x.name == m_activeModuleName);
                }
                if (activeMap == null)
                {
                    activeMap = m_moduleMaps[0];
                }
                if (activeMap != null && activeMap.modules != null)
                {
                    m_srcFolder = activeMap.path;
                    m_activeModuleName = activeMap.name;
                    if (string.IsNullOrEmpty(m_matchText))
                    {
                        m_activeModuleItems.AddRange(activeMap.modules);
                    }
                    else
                    {
                        for (int i = 0; i < activeMap.modules.Count; i++)
                        {
                            var moduleItem = activeMap.modules[i];
                            if (moduleItem.name.ToLower().Contains(m_matchText.ToLower()))
                            {
                                m_activeModuleItems.Add(moduleItem);
                            }
                        }
                    }

                    m_activeModuleItems.Sort((x, y) => string.Compare(x.name, y.name));
                }
            }
        }

        protected void LoadModules(string path)
        {
            if (!System.IO.File.Exists(path))
                return;

            var jsonText = System.IO.File.ReadAllText(path);
            var moduleMap = JsonUtility.FromJson<ModuleMap>(jsonText);
            if (moduleMap != null)
            {
                moduleMap.path = System.IO.Path.GetDirectoryName(path);
                var moduleItem = m_moduleMaps.Find(x => x.name == moduleMap.name);
                if (moduleItem == null)
                {
                    m_moduleMaps.Add(moduleMap);
                }
                else
                {
                    moduleItem.modules = moduleMap.modules;
                }
                m_activeModuleName = moduleMap.name;
                UpdateModuleViewList();
            }
        }

        protected void RunProgram(string programName, string cmd)
        {
            var proc = new System.Diagnostics.Process();
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = programName;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            proc.StandardInput.WriteLine(cmd);
            proc.StandardInput.WriteLine("exit");
            string outStr = proc.StandardOutput.ReadToEnd();
            proc.Close();
            UnityEngine.Debug.Log(outStr);
        }

        protected void OpenSelectFolder(string folder)
        {
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
            psi.Arguments = " /e,/root," + System.IO.Path.GetFullPath(folder);
            var thread = new System.Threading.Thread(() =>
            {
                System.Diagnostics.Process.Start(psi);
            });
            thread.Start();
        }

    }
}