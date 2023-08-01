using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.Build.Player;
using System.IO;
using UnityEditor.Compilation;
using System.Linq;
using System.Collections.Generic;
using ApshaiArts.CodeGuard;
using static UnityEditor.EditorApplication;

namespace UFrame
{
    //[CreateAssetMenu]
    public class CodeExportObject : ScriptableObject
    {
        public string exportPath = "Export";
        [HideInInspector]
        public string sourceNameSpace = "UFrame";
        public string exportNameSpace = "Foundation";
        public bool encryptDll = false;
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
            public bool active = true;
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
            obj.buildInfos.RemoveAll(x => System.Array.Find(moduleDirs, y => x.dirName == System.IO.Path.GetRelativePath(sourceDir, y)) == null);
        }

        private void GetDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var path = moduleDirs[index];
            var moduleName = path.Substring(System.IO.Path.GetDirectoryName(path).Length + 1);
            var labelRect = new Rect(rect.x + 20, rect.y, 300, rect.height);
            EditorGUI.LabelField(labelRect, moduleName + ":");
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
            var defineRect = new Rect(rect.x + 100, rect.y + 2, rect.width - 230, rect.height - 4);
            var info = obj.buildInfos.Find(x => x.dirName == moduleName);
            if (info == null)
            {
                info = new CodeExportObject.BuildInfo();
                info.dirName = moduleName;
                obj.buildInfos.Add(info);
            }
            info.additionalDefines = EditorGUI.TextField(defineRect, info.additionalDefines);
            var activeRect = new Rect(rect.x, rect.y, 20, rect.height);
            info.active = EditorGUI.Toggle(activeRect, info.active);
        }

        private void OnDrawHeader(Rect rect)
        {
            var exportCodeRect = new Rect(rect.x + rect.width - 120, rect.y, 60, rect.height);
            var exportDllRect = exportCodeRect;
            exportDllRect.x += exportCodeRect.width;
            if (GUI.Button(exportCodeRect, "code-all", EditorStyles.miniButtonLeft))
            {
                BuildCodeAll(false);
            }
            if (GUI.Button(exportDllRect, "dll-all", EditorStyles.miniButtonLeft))
            {
                BuildCodeAll(true);
                BuildDllAll(EditorUserBuildSettings.activeBuildTarget);
            }
            EditorGUI.LabelField(rect, "Assemblies:");
        }

        public override void OnInspectorGUI()
        {
            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                base.OnInspectorGUI();
                moduleList.DoLayoutList();
                if (changeScope.changed)
                {
                    EditorUtility.SetDirty(obj);
                    //Debug.LogError(obj + " chaged!");
                }
            }
        }

        private void BuildDll(string path, string moduleName, BuildTarget target)
        {
            var files = new List<string>();
            CollectFiles("*.cs", path, files, true);
            if (files.Count == 0)
            {
                Debug.LogError("empty runtime dir:" + path);
                return;
            }
            string[] csfiles = files.ToArray();
            var output = $"{System.Environment.CurrentDirectory}/{obj.exportPath}/dlls/{target}";
            System.IO.Directory.CreateDirectory(output);
            var defineFiles = System.IO.Directory.GetFiles(path, "*.asmdef", SearchOption.AllDirectories);
            foreach (var item in defineFiles)
            {
                var info = JsonUtility.FromJson<CodeExportObject.AsmInfo>(System.IO.File.ReadAllText(item));
                if (info.includePlatforms != null && System.Array.Find(info.includePlatforms, x => x == "Editor") == null)
                {
                    moduleName = info.name;
                    break;
                }
            }
            var assembilyPath = $"{output}/{moduleName}.dll";
            var builder = new AssemblyBuilder(assembilyPath, csfiles);
            builder.buildTarget = target;
            var buildInfo = obj.buildInfos.Find(x => x.dirName == moduleName);
            if (buildInfo != null && buildInfo.additionalDefines != null && buildInfo.additionalDefines.Length > 0)
            {
                builder.additionalDefines = buildInfo.additionalDefines?.Split(new char[] { '|', ';', ',' });
            }
            StartBuild(builder);
        }

        private void BuildCode(string path)
        {
            BuildCodeByPath(path,false);
        }

        private void CollectFiles(string flags,string dir, List<string> files, bool ignoreEditor)
        {
            if (ignoreEditor)
            {
                var defineFiles = Directory.GetFiles(dir, "*.asmdef", SearchOption.TopDirectoryOnly);
                foreach (var item in defineFiles)
                {
                    if (item.EndsWith("Editor"))
                        return;

                    var info = JsonUtility.FromJson<CodeExportObject.AsmInfo>(System.IO.File.ReadAllText(item));
                    if (info.includePlatforms != null && System.Array.Find(info.includePlatforms, x => x == "Editor") != null)
                    {
                        return;
                    }
                }
            }


            var topFiles = System.IO.Directory.GetFiles(dir, flags, SearchOption.TopDirectoryOnly);
            files.AddRange(topFiles);

            var dirs = System.IO.Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly);
            foreach (var subDir in dirs)
            {
                CollectFiles(flags,subDir, files, ignoreEditor);
            }
        }

        private void BuildDllAll(BuildTarget target)
        {
            var sourceFolder = $"{System.Environment.CurrentDirectory }/{obj.exportPath}";
            var files = new List<string>();
            CollectFiles("*.cs",sourceFolder, files, true);
            if(files.Count == 0)
            {
                Debug.LogError("empty cs!");
                return;
            }
            string[] csfiles = files.ToArray();
            var output = $"{System.Environment.CurrentDirectory}/{obj.exportPath}/dlls/{target}";
            Directory.CreateDirectory(output);
            var assembilyPath = $"{output}/{obj.exportNameSpace}.dll";
            var builder = new AssemblyBuilder(assembilyPath, csfiles);
            builder.buildTarget = target;
            var defines = new List<string>();
            foreach (var item in obj.buildInfos)
            {
                if(item.active && !string.IsNullOrEmpty(item.additionalDefines))
                {
                    var itemInfos = item.additionalDefines?.Split(new char[] { '|', ';', ',' });
                    if (itemInfos != null && itemInfos.Length > 0)
                    {
                        defines.AddRange(itemInfos);
                    }
                }
            }
            if(defines.Count > 0)
            {
                Debug.LogError(defines.Count);
                builder.additionalDefines = defines.ToArray();
            }
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
                EditorApplication.delayCall += () =>
                {
                    if (obj.encryptDll)
                    {
                        var outputDir = $"{System.Environment.CurrentDirectory }/{obj.exportPath}/encrypted-dlls/{builder.buildTarget}";
                        DoCodeGuard(builder.assemblyPath, outputDir);
                        ShowDir(outputDir);
                    }
                    else
                    {
                        ShowDir(Path.GetDirectoryName(builder.assemblyPath));
                    }
                };
            };
            builder.Build();
        }

        private void ShowDir(string dir)
        {
            Application.OpenURL(new System.Uri(dir).AbsoluteUri);
        }

        private void BuildCodeAll(bool clean)
        {
            if (clean)
            {
                var output = $"{System.Environment.CurrentDirectory}/{obj.exportPath}";
                var oldDirs = System.IO.Directory.GetFileSystemEntries(output);
                var targetName = EditorUserBuildSettings.activeBuildTarget.ToString();
                foreach (var item in oldDirs)
                {
                    if (System.IO.Directory.Exists(item))
                    {
                        if (item.EndsWith(targetName) || item.EndsWith("dlls"))
                            continue;
                        System.IO.Directory.Delete(item, true);
                    }
                    else
                    {
                        System.IO.File.Delete(item);
                    }
                }
            }
            BuildCodeByPath(sourceDir,true);
        }

        private void BuildCodeByPath(string path,bool top)
        {
            List<string> files = new List<string>();
            if (top)
            {
                var dirs = System.IO.Directory.GetDirectories(path);
                foreach (var item in dirs)
                {
                    var dirName = System.IO.Path.GetRelativePath(sourceDir, item);
                    var info = obj.buildInfos.Find(x => x.dirName == dirName);
                    if (info == null || info.active)
                    {
                        CollectFiles("*", item, files, false);
                    }
                }
            }
            else
            {
                CollectFiles("*", path, files, false);
            }
            var counter = 0;
            float countAll = files.Count;
            var targetFolder = System.Environment.CurrentDirectory + "/" + obj.exportPath;
            foreach (var item in files)
            {
                var relePath = System.IO.Path.GetRelativePath(sourceDir, item);
                if (relePath.EndsWith(".meta"))
                    continue;
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
            ShowDir(targetFolder);
        }


        public void DoCodeGuard(string dllPath, string outputPath)
        {
            if (!System.IO.File.Exists(dllPath))
            {
                Debug.LogError("dll not exits:" + dllPath);
                return;
            }
            float progress = 0.0f;
            EditorUtility.DisplayProgressBar("CodeGuard", "Obfuscating and protecting code...", progress);
            CodeGuardSetup setup = CodeGuard.CodeGuardSetupSettings();
            setup.obfuscateProperties = false;
            setup.obfuscatePrivateMembers = false;
            setup.obfuscateTypeFields = true;
            setup.obfuscatePrivateFieldsAndProperties = true;
            setup.obfuscateMethodParameters = false;
            setup.proxyUnityMethods = true;
            setup.proxyExcludedMethods = true;
            setup.proxyCustomMethods = false;
            setup.obfuscateCustomMethods = false;
            setup.skipFieldsWithSerializeFieldAttribute = true;
            setup.skipUnityTypesPublicFields = true;
            setup.skipUnityTypesPublicStaticFields = true;
            setup.symbolRenamingModeLatin = true;
            setup.symbolRenamingModeUnreadableLite = false;
            setup.obfuscateTypeFieldsAggressively = false;
            setup.typeSelectionMode = 0;
            setup.AddAssembly(dllPath);
            System.IO.Directory.CreateDirectory(outputPath);
            setup.outputDirectory = outputPath;
            setup.AddAssemblySearchDirectory($"Library/ScriptAssemblies");
            progress = 0.25f;
            EditorUtility.DisplayProgressBar("CodeGuard", "Obfuscating and protecting code...", progress);
            setup.Run();
            EditorUtility.ClearProgressBar();
        }

    }
}