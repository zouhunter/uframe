using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.Build.Player;
using System.IO;

public class CodeExportWindow : EditorWindow
{
    public string exportPath = "Export";
    public string sourceNameSpace = "UFrame";
    public string exportNameSpace = "UFrame";
    public bool encrypt = false;
    private string[] moduleDirs;
    private string sourceDir;
    private ReorderableList moduleList;

    [MenuItem("Window/CodeExporter")]
    public static void ExportCode()
    {
        GetWindow<CodeExportWindow>();
    }

    private void OnEnable()
    {
        sourceDir = $"{Application.dataPath}/Framework";
        moduleDirs = System.IO.Directory.GetDirectories(sourceDir);
        moduleList = new ReorderableList(moduleDirs, typeof(string));
        moduleList.displayAdd = moduleList.displayRemove = false;
        moduleList.drawHeaderCallback += OnDrawHeader;
        moduleList.drawElementCallback += GetDrawElementCallback;
    }

    private void GetDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
    {
        var path = moduleDirs[index];
        var moduleName = path.Substring(System.IO.Path.GetDirectoryName(path).Length +1);
        EditorGUI.LabelField(rect,moduleName);

        var exportCodeRect = new Rect(rect.x + rect.width - 120, rect.y, 60, rect.height);
        var exportDllRect = exportCodeRect;
        exportDllRect.x += exportCodeRect.width;
        if (GUI.Button(exportCodeRect, "code", EditorStyles.miniButtonLeft))
        {
            BuildCode(path);
        }
        if (GUI.Button(exportDllRect, "dll", EditorStyles.miniButtonLeft))
        {
            BuildDll(path, moduleName);
        }
    }

    private void OnDrawHeader(Rect rect)
    {
        var exportCodeRect = new Rect(rect.x + rect.width - 120, rect.y, 60, rect.height);
        var exportDllRect = exportCodeRect;
        exportDllRect.x += exportCodeRect.width;
        if (GUI.Button(exportCodeRect,"code-all",EditorStyles.miniButtonLeft))
        {
            BuildCodeAll();
        }
        if (GUI.Button(exportDllRect, "dll-all",EditorStyles.miniButtonLeft))
        {
            BuildDllAll();
        }
        
    }

    private void OnGUI()
    {
        exportPath = EditorGUILayout.TextField("Export", exportPath);
        sourceNameSpace = EditorGUILayout.TextField("NameSpace",sourceNameSpace);
        exportNameSpace = EditorGUILayout.TextField("NewNameSpace", exportNameSpace);
        moduleList.DoLayoutList();
    }
    
    private void BuildDll(string path,string moduleName)
    {
        var csfiles = System.IO.Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
        var output = $"{System.Environment.CurrentDirectory}/{exportPath}/dlls";
        System.IO.Directory.CreateDirectory(output);
        var defineFiles = System.IO.Directory.GetFiles(path, "*.asmdef", SearchOption.AllDirectories);
        foreach (var item in defineFiles)
        {
            var assetPath = System.IO.Path.GetRelativePath(System.Environment.CurrentDirectory, item);
            var defineAsset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(assetPath);
            if (!defineAsset.name.Contains("Editor"))
                moduleName = defineAsset.name;
        }
        var builder = new UnityEditor.Compilation.AssemblyBuilder($"{output}/{moduleName}", csfiles);
        builder.Build();
    }

    private void BuildCode(string path)
    {
        BuildCodeByPath(path);
    }

    private void BuildDllAll()
    {
        var targetFolder = $"{System.Environment.CurrentDirectory}/{exportPath}/dlls";
        CompileDll(targetFolder, EditorUserBuildSettings.activeBuildTarget, false);
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
        var targetFolder = System.Environment.CurrentDirectory + "/" + exportPath;
        foreach (var item in files)
        {
            var relePath = System.IO.Path.GetRelativePath(sourceDir, item);
            var text = System.IO.File.ReadAllText(item);
            text = text.Replace(sourceNameSpace, exportNameSpace);
            text = text.Replace(sourceNameSpace.ToLower(), exportNameSpace.ToLower());
            var targetPath = System.IO.Path.Join(targetFolder, relePath);
            Debug.Log(targetPath);
            var dir = System.IO.Path.GetDirectoryName(targetPath);
            System.IO.Directory.CreateDirectory(dir);
            System.IO.File.WriteAllText(targetPath, text);
            var cancel = EditorUtility.DisplayCancelableProgressBar("export code", "progress", ++counter / countAll);
            if (cancel)
                break;
        }
        EditorUtility.ClearProgressBar();
    }

    public static void CompileDll(string buildDir, BuildTarget target, bool developmentBuild)
    {
        var group = BuildPipeline.GetBuildTargetGroup(target);
        ScriptCompilationSettings scriptCompilationSettings = new ScriptCompilationSettings();
        scriptCompilationSettings.group = group;
        scriptCompilationSettings.target = target;
        scriptCompilationSettings.options = developmentBuild ? ScriptCompilationOptions.DevelopmentBuild : ScriptCompilationOptions.None;
        Directory.CreateDirectory(buildDir);
        ScriptCompilationResult scriptCompilationResult = PlayerBuildInterface.CompilePlayerScripts(scriptCompilationSettings, buildDir);
        UnityEditor.EditorUtility.ClearProgressBar();
        Debug.Log("compile finish!!!");
    }

}
