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
            public string[] references;
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
                BuildDlls(targetFolder, moduleName, EditorUserBuildSettings.activeBuildTarget);
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

        private string[] SortAsmFiles(CodeExportObject.AsmInfo[] asmFiles)
        {
            List<string> asmNames = new List<string>();
            foreach (var item in asmFiles)
            {
                if(item.references == null || item.references.Length == 0)
                {
                    asmNames.Insert(0, item.name);
                }
                else
                {
                    var index = asmNames.Count;
                    foreach (var guidStr in item.references)
                    {
                        var guid = guidStr.Substring(5);
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        var subInfo = JsonUtility.FromJson<CodeExportObject.AsmInfo>(System.IO.File.ReadAllText(path));
                        for (int i = 0; i < asmNames.Count; i++)
                        {
                            if(subInfo.name == asmNames[i])
                            {
                                index = i + 1;
                            }
                        }
                    }
                    if(index == asmNames.Count)
                    {
                        asmNames.Add(item.name);
                    }
                    else
                    {
                        asmNames.Insert(index,item.name);
                    }
                }
            }
            return asmNames.ToArray();
        }

        private void BuildDlls(string path, string moduleName, BuildTarget target)
        {
            var filesMap = new Dictionary<string, string[]>();
            var output = $"{System.Environment.CurrentDirectory}/{obj.exportPath}/dlls/{target}";
            System.IO.Directory.CreateDirectory(output);
            var defineFiles = System.IO.Directory.GetFiles(path, "*.asmdef", SearchOption.AllDirectories);
            List<CodeExportObject.AsmInfo> asmInfos = new List<CodeExportObject.AsmInfo>();
            foreach (var item in defineFiles)
            {
                var info = JsonUtility.FromJson<CodeExportObject.AsmInfo>(System.IO.File.ReadAllText(item));
                var files = new List<string>();
                CollectAssemblyScripts(true, System.IO.Path.GetDirectoryName(item), files);
                if (files.Count > 0)
                {
                    filesMap[info.name] = files.ToArray();
                    asmInfos.Add(info);
                }
            }
            var names = SortAsmFiles(asmInfos.ToArray());
            var buildInfo = obj.buildInfos.Find(x => x.dirName == moduleName);
            foreach (var dllName in names)
            {
                var assembilyPath = $"{output}/{dllName}.dll";
                var builder = new AssemblyBuilder(assembilyPath, filesMap[dllName]);
                builder.buildTarget = target;
                if (buildInfo != null && buildInfo.additionalDefines != null && buildInfo.additionalDefines.Length > 0)
                {
                    builder.additionalDefines = buildInfo.additionalDefines?.Split(new char[] { '|', ';', ',' });
                }
                StartBuild(builder);
            }
        }

        private void BuildCode(string path)
        {
            BuildCodeByPath(path, false);
        }


        private void CollectAssemblyScripts(bool top, string dir, List<string> files)
        {
            if (!top)
            {
                var defineFiles = Directory.GetFiles(dir, "*.asmdef", SearchOption.TopDirectoryOnly);
                if (defineFiles.Length > 0)
                    return;
            }


            var topFiles = System.IO.Directory.GetFiles(dir, "*.cs", SearchOption.TopDirectoryOnly);
            files.AddRange(topFiles);

            var dirs = System.IO.Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly);
            foreach (var subDir in dirs)
            {
                CollectAssemblyScripts(false, subDir, files);
            }
        }
        private void CollectFileIgnoreMeta(string dir, List<string> files)
        {
            var topFiles = System.IO.Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly);
            foreach (var item in topFiles)
            {
                if (item.EndsWith(".meta"))
                    continue;
                files.Add(item);
            }

            var dirs = System.IO.Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly);
            foreach (var subDir in dirs)
            {
                CollectFileIgnoreMeta(subDir, files);
            }
        }

        private bool CheckIsEditorAssembly(string asmPath)
        {
            var editorAssembly = false;
            if (System.IO.Path.GetDirectoryName(asmPath).EndsWith("Editor"))
                editorAssembly = true;

            var info = JsonUtility.FromJson<CodeExportObject.AsmInfo>(System.IO.File.ReadAllText(asmPath));
            if (info.includePlatforms != null && System.Array.Find(info.includePlatforms, x => x == "Editor") != null)
                editorAssembly = true;
            return editorAssembly;
        }

        private void CollectCSharpFiles(string flags, string dir, List<string> files, bool editor)
        {
            var defineFiles = Directory.GetFiles(dir, "*.asmdef", SearchOption.TopDirectoryOnly);
            foreach (var item in defineFiles)
            {
                var editorAssembly = CheckIsEditorAssembly(item);
                if (editor == editorAssembly)
                {
                    var topFiles = System.IO.Directory.GetFiles(dir, flags, SearchOption.TopDirectoryOnly);
                    files.AddRange(topFiles);
                }
            }
            var dirs = System.IO.Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly);
            foreach (var subDir in dirs)
            {
                CollectCSharpFiles(flags, subDir, files, editor);
            }
        }

        private void BuildDllAll(BuildTarget target)
        {
            var sourceFolder = $"{System.Environment.CurrentDirectory }/{obj.exportPath}";
            var runtimeFiles = new List<string>();
            var editorFiles = new List<string>();
            CollectCSharpFiles("*.cs", sourceFolder, runtimeFiles, false);
            CollectCSharpFiles("*.cs", sourceFolder, editorFiles, true);

            var output = $"{System.Environment.CurrentDirectory}/{obj.exportPath}/dlls/{target}";
            Directory.CreateDirectory(output);

            if (runtimeFiles.Count > 0)
            {
                BuildAllDll(output, target, obj.exportNameSpace, runtimeFiles.ToArray());
            }

            if (editorFiles.Count > 0)
            {
                BuildAllDll(output, target, obj.exportNameSpace + ".Editors", editorFiles.ToArray());
            }
        }

        private void BuildAllDll(string output, BuildTarget target, string dllName, string[] csFiles)
        {
            var assembilyPath = $"{output}/{dllName}.dll";
            var builder = new AssemblyBuilder(assembilyPath, csFiles);
            builder.buildTarget = target;
            var defines = new List<string>();
            foreach (var item in obj.buildInfos)
            {
                if (item.active && !string.IsNullOrEmpty(item.additionalDefines))
                {
                    var itemInfos = item.additionalDefines?.Split(new char[] { '|', ';', ',' });
                    if (itemInfos != null && itemInfos.Length > 0)
                    {
                        defines.AddRange(itemInfos);
                    }
                }
            }
            if (defines.Count > 0)
            {
                Debug.LogError(defines.Count);
                builder.additionalDefines = defines.ToArray();
            }
            StartBuild(builder);
        }

        private void StartBuild(AssemblyBuilder builder)
        {
            var additionalReferences = new List<string>();
            var dlls = System.IO.Directory.GetFiles(System.IO.Path.GetFullPath("Library/ScriptAssemblies"), "*.dll");
            additionalReferences.AddRange(dlls);
            var exePath = System.IO.Path.GetDirectoryName(System.Environment.GetCommandLineArgs()[0]);
            dlls = System.IO.Directory.GetFiles($"{exePath}/Data/Managed", "*.dll");
            additionalReferences.AddRange(dlls);
            var output = $"{System.Environment.CurrentDirectory}/{obj.exportPath}/dlls/{builder.buildTarget}";
            dlls = Directory.GetFiles(output, "*.dll");
            additionalReferences.AddRange(dlls);
            foreach (var item in additionalReferences)
            {
                Debug.LogWarning(item);
            }
            builder.additionalReferences = additionalReferences.ToArray();
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
            while (builder.status != AssemblyBuilderStatus.Finished)
                System.Threading.Thread.Sleep(10);
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
            BuildCodeByPath(sourceDir, true);
        }

        private void BuildCodeByPath(string path, bool top)
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
                        CollectFileIgnoreMeta(item, files);
                    }
                }
            }
            else
            {
                CollectFileIgnoreMeta(path, files);
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
            var exePath = System.IO.Path.GetDirectoryName(System.Environment.GetCommandLineArgs()[0]);
            setup.AddAssemblySearchDirectory($"{exePath}/Data/Managed");
            setup.AddAssemblySearchDirectory($"{exePath}/Data/Managed/UnityEngine");
            progress = 0.25f;
            EditorUtility.DisplayProgressBar("CodeGuard", "Obfuscating and protecting code...", progress);
            //setup.Run();
            EditorUtility.ClearProgressBar();
        }

    }
}