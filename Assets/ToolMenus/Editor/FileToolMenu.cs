using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

namespace UFrame.Editors
{
    public class FileToolMenu
    {
        [MenuItem("Assets/UFrame/CodeProcess/ChineseCheck")]
        static void ChineseCheckInfo()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            ErgodicDirectoryOrFile(path, CheckChineseFileName);
        }

        [MenuItem("Assets/UFrame/CodeProcess/ChineseCheck", validate = true)]
        static bool ChineseCheckInfoValid()
        {
            return CheckSelectScriptFileExist();
        }

        [MenuItem("Assets/UFrame/CodeProcess/CodeFormatCheck")]
        static void CodeFormatCheck()
        {
            ErgodicSelectScript(CheckFormatIsUtf8);
        }
        [MenuItem("Assets/UFrame/CodeProcess/CodeFormatCheck", validate = true)]
        static bool CodeFormatCheckValid()
        {
            return CheckSelectScriptFileExist();
        }

        [MenuItem("Assets/UFrame/CodeProcess/CodeFormat2Utf8")]
        static void CodeFormat2Utf8()
        {
            ErgodicSelectScript(MakeSureFileUtf8);
            AssetDatabase.Refresh();
        }
       
        [MenuItem("Assets/UFrame/CodeProcess/CodeFormat2Utf8", validate =true)]
        static bool CodeFormat2Utf8Valid()
        {
            return CheckSelectScriptFileExist();
        }

        [MenuItem("Assets/UFrame/CodeProcess/CodeToUnix")]
        static void CodeToUnix()
        {
            ErgodicSelectScript(MakeSureUnix);
            AssetDatabase.Refresh();
        }
        [MenuItem("Assets/UFrame/CodeProcess/CodeToUnix", validate = true)]
        static bool CodeToUnixValid()
        {
            return CheckSelectScriptFileExist();
        }

        [MenuItem("Assets/UFrame/Create/ScriptObject")]
        static void CreateScriptObject()
        {
            ProjectWindowUtil.CreateAsset(ScriptableObject.CreateInstance<ScriptableObject>(), "new_scriptobject.asset");
        }

        private static void MakeSureUnix(string filePath)
        {
            var texts = System.IO.File.ReadAllText(filePath);
            var newTexts = texts.Replace("\r\n", "\n");
            if(texts != newTexts)
            {
                Debug.Log("file process ok:" + filePath);
                System.IO.File.WriteAllText(filePath,newTexts);
            }
        }

        static void ErgodicDirectoryOrFile(string dirPath, System.Action<string, bool> onCheck)
        {
            onCheck(dirPath, false);
            var fsinfos = Directory.GetFiles(dirPath);
            for (int i = 0; i < fsinfos.Length; i++)
            {
                onCheck(fsinfos[i], true);
            }
            var subDirectorys = Directory.GetDirectories(dirPath);
            for (int i = 0; i < subDirectorys.Length; i++)
            {
                ErgodicDirectoryOrFile(subDirectorys[i], onCheck);
            }
        }

        static bool CheckSelectScriptFileExist()
        {
            var listFiles = new List<string>();
            ErgodicSelectScript((item) => { listFiles.Add(item); });
            return listFiles.Count > 0;
        }

        static void ErgodicSelectScript(System.Action<string> onCheckFile)
        {
            if (Selection.activeObject is DefaultAsset)
            {
                var folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (folderPath == "Assets")
                    return;

                var files = System.IO.Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    onCheckFile(files[i]);
                }
            }
            else if (Selection.activeObject is MonoScript)
            {
                onCheckFile(AssetDatabase.GetAssetPath(Selection.activeObject));
            }
        }
        protected static void CheckChineseFileName(string filePath, bool isFile)
        {
            if (Regex.IsMatch(filePath, @"[\u4e00-\u9fa5]"))
            {
                Debug.LogError("[汉字警告],此路径:" + filePath);
            }
        }
        protected static void MakeSureFileUtf8(string filePath)
        {
            if (File.Exists(filePath))
            {
                if (FileTool.TryGetFileEncoding(filePath, out var encoding))
                {
                    if(!encoding.Equals(System.Text.Encoding.UTF8))
                    {
                        var fileText = System.IO.File.ReadAllText(filePath, encoding).Replace("\r\n","\n");
                        System.IO.File.WriteAllText(filePath, fileText, Encoding.UTF8);
                        Debug.LogWarning("convent file from " + encoding + " to utf8:" + filePath);
                    }
                }
                else
                {
                    Debug.LogError("unknown file format:" + filePath);
                }
            }
        }

        protected static void CheckFormatIsUtf8(string filePath)
        {
            if (FileTool.TryGetFileEncoding(filePath, out var encoding))
            {
                if(!encoding.Equals(System.Text.Encoding.UTF8))
                {
                    var fullPath = System.IO.Path.GetFullPath(filePath);
                    Debug.LogError("file format " +encoding+ " not utf8:" + fullPath);
                }
            }
            else
            {
                Debug.LogError("unknown file format:" + filePath);
            }
        }
    }
}