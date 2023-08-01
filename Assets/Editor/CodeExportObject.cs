using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.Build.Player;
using System.IO;
using UnityEditor.Compilation;
using System.Linq;
using System.Collections.Generic;

namespace UFrame
{
    //[CreateAssetMenu]
    public class CodeExportObject : ScriptableObject
    {
        public string exportPath = "Export";
        public string sourceNameSpace = "UFrame";
        public string exportNameSpace = "Foundation";
        public bool encrypt = false;
        [HideInInspector]
        public List<BuildInfo> buildInfos = new List<BuildInfo>();

        [System.Serializable]
        public class AsmInfo
        {
            public string name;
            public string[] includePlatforms;
        }

        [System.Serializable]
        public class BuildInfo
        {
            public string dirName;
            public string additionalDefines;
        }
    }

    [CustomEditor(typeof(CodeExportObject))]
    [DisallowMultipleComponent]
    public class CodeExportObjectEditor : Editor
    {
        private ReorderableList moduleList;
        private string[] moduleDirs;
        private string sourceDir;
        private CodeExportObject obj;

        private void OnEnable()
        {
            obj = target as CodeExportObject;
            sourceDir = $"{Application.dataPath}/Framework";
            moduleDirs = System.IO.Directory.GetDirectories(sourceDir);
            moduleList = new ReorderableList(moduleDirs, typeof(string));
            moduleList.displayAdd = moduleList.displayRemove = false;
            moduleList.drawHeaderCallback += OnDrawHeader;
            moduleList.drawElementCallback += GetDrawElementCallback;
            obj.buildInfos.RemoveAll(x=>System.Array.Find(moduleDirs,y=>x.dirName==y) == null);
        }

        private void GetDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var path = moduleDirs[index];
            var moduleName = path.Substring(System.IO.Path.GetDirectoryName(path).Length + 1);
            EditorGUI.LabelField(rect, moduleName + ":");

            var exportCodeRect = new Rect(rect.x + rect.width - 120, rect.y, 60, rect.height);
            var exportDllRect = exportCodeRect;
            exportDllRect.x += exportCodeRect.width;
            if (GUI.Button(exportCodeRect, "code", EditorStyles.miniButtonLeft))
            {
                BuildCode(path);
            }
            if (GUI.Button(exportDllRect, "dll", EditorStyles.miniButtonLeft))
            {
                BuildCode(path);
                var targetFolder = $"{System.Environment.CurrentDirectory }/{obj.exportPath}/{moduleName}";
                BuildDll(targetFolder, moduleName, EditorUserBuildSettings.activeBuildTarget);
            }
            var defineRect = new Rect(rect.x + 100,rect.y + 2,rect.width - 230,rect.height-4);
            var info = obj.buildInfos.Find(x => x.dirName == moduleName);
            if(info == null)
            {
                info = new CodeExportObject.BuildInfo();
                info.dirName = moduleName;
                obj.buildInfos.Add(info);
            }
            info.additionalDefines = EditorGUI.TextField(defineRect, info.additionalDefines);
        }

        private void OnDrawHeader(Rect rect)
        {
            var exportCodeRect = new Rect(rect.x + rect.width - 120, rect.y, 60, rect.height);
            var exportDllRect = exportCodeRect;
            exportDllRect.x += exportCodeRect.width;
            if (GUI.Button(exportCodeRect, "code-all", EditorStyles.miniButtonLeft))
            {
                BuildCodeAll();
            }
            if (GUI.Button(exportDllRect, "dll-all", EditorStyles.miniButtonLeft))
            {
                BuildCodeAll();
                BuildDllAll(EditorUserBuildSettings.activeBuildTarget);
            }
            EditorGUI.LabelField(rect,"Assemblies:");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            moduleList.DoLayoutList();
        }

        private void BuildDll(string path, string moduleName, BuildTarget target)
        {
            string[] csfiles = System.IO.Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
            var output = $"{System.Environment.CurrentDirectory}/{obj.exportPath}/dlls/{target}";
            System.IO.Directory.CreateDirectory(output);
            var defineFiles = System.IO.Directory.GetFiles(path, "*.asmdef", SearchOption.AllDirectories);
            foreach (var item in defineFiles)
            {
                var info = JsonUtility.FromJson<CodeExportObject.AsmInfo>(System.IO.File.ReadAllText(item));
                if (System.Array.Find(info.includePlatforms, x => x == "Editor") == null)
                {
                    moduleName = info.name;
                    csfiles = System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(item), "*.cs", SearchOption.AllDirectories);
                }
            }
            var assembilyPath = $"{output}/{moduleName}.dll";
            var builder = new AssemblyBuilder(assembilyPath, csfiles);
            builder.buildTarget = target;
            var buildInfo = obj.buildInfos.Find(x => x.dirName == moduleName);
            if(buildInfo != null)
            {
                builder.additionalDefines = buildInfo.additionalDefines.Split(new char[] { '|', ';', ',' });
            }
            StartBuild(builder);
        }

        private void BuildCode(string path)
        {
            BuildCodeByPath(path);
        }

        private void CollectCSharpScripts(string dir,List<string> files)
        {
            var defineFiles = Directory.GetFiles(dir, "*.asmdef",SearchOption.TopDirectoryOnly);
            foreach (var item in defineFiles)
            {
                var info = JsonUtility.FromJson<CodeExportObject.AsmInfo>(System.IO.File.ReadAllText(item));
                if (info.includePlatforms != null && System.Array.Find(info.includePlatforms, x => x == "Editor") != null)
                {
                    return;
                }
            }

            var topFiles = System.IO.Directory.GetFiles(dir, "*.cs", SearchOption.TopDirectoryOnly);
            files.AddRange(topFiles);

            var dirs = System.IO.Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly);
            foreach (var subDir in dirs)
            {
                CollectCSharpScripts(subDir, files);
            }
        }

        private void BuildDllAll(BuildTarget target)
        {
            var sourceFolder = $"{System.Environment.CurrentDirectory }/{obj.exportPath}";
            var files = new List<string>();
            CollectCSharpScripts(sourceFolder, files);
            string[] csfiles = files.ToArray();
            var output = $"{System.Environment.CurrentDirectory}/{obj.exportPath}/dlls/{target}";
            Directory.CreateDirectory(output);
            var assembilyPath = $"{output}/{obj.exportNameSpace}.dll";
            var builder = new AssemblyBuilder(assembilyPath, csfiles);
            builder.buildTarget = target;
            var defines = new List<string>();
            foreach (var item in obj.buildInfos)
            {
                var itemInfos = item.additionalDefines?.Split(new char[] { '|', ';', ',' });
                if(itemInfos != null && itemInfos.Length > 0)
                    defines.AddRange(itemInfos);
            }
            builder.additionalDefines = defines.ToArray();
            StartBuild(builder);
        }

        private void StartBuild(AssemblyBuilder builder)
        {
            // Called on main thread
            builder.buildStarted += delegate (string assemblyPath)
            {
                Debug.LogFormat("Assembly build started for {0}", assemblyPath);
            };
            // Called on main thread
            builder.buildFinished += delegate (string assemblyPath, CompilerMessage[] compilerMessages)
            {
                foreach (var item in compilerMessages)
                {
                    switch (item.type)
                    {
                        case CompilerMessageType.Error:
                            Debug.LogError(item.message);
                            break;
                        case CompilerMessageType.Warning:
                            Debug.LogWarning(item.message);
                            break;
                        case CompilerMessageType.Info:
                            Debug.Log(item.message);
                            break;
                        default:
                            break;
                    }
                }
                var warningCount = compilerMessages.Count(m => m.type == CompilerMessageType.Warning);
                var errorCount = compilerMessages.Count(m => m.type == CompilerMessageType.Error);
                Debug.LogFormat("Assembly build finished for {0}", assemblyPath);
                Debug.LogFormat("Warnings: {0} - Errors: {1}", warningCount, errorCount);
                Application.OpenURL(new System.Uri(System.IO.Path.GetDirectoryName(builder.assemblyPath)).AbsoluteUri);
            };
            builder.Build();
        }

        private void BuildCodeAll()
        {
            BuildCodeByPath(sourceDir);
        }

        private void BuildCodeByPath(string path)
        {
            var files = System.IO.Directory.GetFiles(path, "*", System.IO.SearchOption.AllDirectories);
            var counter = 0;
            float countAll = files.Length;
            var targetFolder = System.Environment.CurrentDirectory + "/" + obj.exportPath;
            foreach (var item in files)
            {
                var relePath = System.IO.Path.GetRelativePath(sourceDir, item);
                var text = System.IO.File.ReadAllText(item);
                text = text.Replace(obj.sourceNameSpace, obj.exportNameSpace);
                text = text.Replace(obj.sourceNameSpace.ToLower(), obj.exportNameSpace.ToLower());
                var targetPath = System.IO.Path.Join(targetFolder, relePath);
                var dir = System.IO.Path.GetDirectoryName(targetPath);
                System.IO.Directory.CreateDirectory(dir);
                System.IO.File.WriteAllText(targetPath, text);
                var cancel = EditorUtility.DisplayCancelableProgressBar("export code", "progress", ++counter / countAll);
                if (cancel)
                    break;
            }
            EditorUtility.ClearProgressBar();
        }
    }
}