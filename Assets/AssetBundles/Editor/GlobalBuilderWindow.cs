using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace UFrame.AssetBundles
{
    public class ProjectsBuilderWindow : EditorWindow
    {
        [MenuItem(ABBUtility.Menu_ProjectsBuildWindow)]
        static void BuildProjectsAssetBundles()
        {
            EditorWindow.GetWindow<ProjectsBuilderWindow>("全局AssetBundle", true);
        }

        public string assetBundleName;
        public string localPath = "";
        public string targetPath = "";
        public BuildAssetBundleOptions buildOption = BuildAssetBundleOptions.None;
        public BuildTarget buildTarget = BuildTarget.StandaloneWindows;

        private const string Perfer_buildPath = "globalbuildPath";

        void OnEnable()
        {
            if (PlayerPrefs.HasKey(Perfer_buildPath))
            {
                localPath = PlayerPrefs.GetString(Perfer_buildPath);
            }
        }
        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            localPath = EditorGUILayout.TextField("ExportTo", localPath);
            if (GUILayout.Button("选择路径"))
            {
                var path = EditorUtility.SaveFolderPanel("选择保存路径", localPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    localPath = System.IO.Path.GetRelativePath(System.Environment.CurrentDirectory,path);
                    PlayerPrefs.SetString(Perfer_buildPath, localPath);
                    this.Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                targetPath = EditorGUILayout.TextField("CopyTo", targetPath);
                if (GUILayout.Button("选择路径"))
                {
                    var path = EditorUtility.SaveFolderPanel("选择本地路径", targetPath, "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        targetPath = path;
                        PlayerPrefs.SetString(Perfer_buildPath, targetPath);
                        this.Repaint();
                    }
                }
                if (GUILayout.Button("Copy"))
                {
                    FileUtil.DeleteFileOrDirectory(targetPath);
                    FileUtil.CopyFileOrDirectory(localPath, targetPath);
                }
            }
            buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("BuildTarget", buildTarget);


            #region 全局打包
            buildOption = (BuildAssetBundleOptions)EditorGUILayout.EnumFlagsField("Options", buildOption);
            if (GUILayout.Button("GlobleBulid"))
            {
                ABBUtility.BuildProjectsAssetBundle(localPath, buildOption, buildTarget);
            }
            #endregion
        }
    }
}
