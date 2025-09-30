#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
namespace UFrame.HiveBundle
{
    [InitializeOnLoad]
    public static class HiveBundleGUI
    {
        static GUIContent s_packIcon;
        static string[] _groups;
        static string[] _exclude = new string[] { ".cs", ".js", ".boo", ".exe", ".dll", ".meta", ".preset", ".asmdef" };
        private static Dictionary<AssetBundleGroup, AssetBundleInfo> groups = new Dictionary<AssetBundleGroup, AssetBundleInfo>();
        private static Dictionary<string, string> _cacheName = new Dictionary<string, string>();
        static HiveBundleGUI()
        {
            s_packIcon = EditorGUIUtility.IconContent("d_CacheServerConnected@2x", "address");
            Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
        }

        [UnityEngine.TestTools.ExcludeFromCoverage]
        static void OnPostHeaderGUI(Editor editor)
        {
            if (!AssetBundleSetting.Instance.inspectorGui)
                return;

            EditorGUI.EndDisabledGroup();
            if (editor.targets == null || editor.targets.Length == 0)
                return;
            if (_groups == null)
                _groups = AssetBundleSetting.Instance.groups.Select(x=>x.name).ToArray();

            if (editor.targets.Length == 1)
            {
                var target = editor.targets[0];
                if (target != Selection.activeObject)
                    return;

                var path = AssetDatabase.GetAssetPath(editor.targets[0]);
                var ext = Path.GetExtension(path).ToLower();
                if (!string.IsNullOrEmpty(path) && System.Array.IndexOf(_exclude, ext) < 0)
                {
                    groups.Clear();
                    bool editable = true;
                    var bundlePath = GetImplicitBundleInfo(path, groups, ref editable);

                    using (GUILayout.HorizontalScope h = new GUILayout.HorizontalScope())
                    {
                        if (!string.IsNullOrEmpty(bundlePath))
                        {
                            using (var disableScope = new EditorGUI.DisabledScope(!editable))
                            {
                                if (!editable)
                                    bundlePath = bundlePath + "/" + Path.GetFileNameWithoutExtension(path);
                                DrawTitleInfo(bundlePath, path);
                            }
                        }
                        else
                        {
                            DrawTitleInfoAdd(bundlePath, path);
                        }
                    }
                }
            }
        }
        private static void DrawTitleInfo(string bundlePath, string path)
        {
            // 已添加状态 - 绿色背景显示
            var originalColor = GUI.backgroundColor;
            // 显示enable toggle选项
            var isEnabled = CheckBundleEnable(groups.First().Value.bundlePath);
            if (!isEnabled)
            {
                GUI.backgroundColor = new Color(1f, 0.8f, 0.8f, 0.3f); // 浅红色背景
            }

            using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                var groupNames = groups.Keys.Select(x => x.name).ToArray();

                var firstGroup = groups.First();

                var newEnabled = EditorGUILayout.Toggle(isEnabled, GUILayout.Width(20));
                if (newEnabled != isEnabled)
                {
                    // 切换所有资源的enable状态
                    foreach (var group in groups)
                    {
                        MarkBundleEnable(group.Value.bundlePath, newEnabled);
                    }
                }

                var buttonText = firstGroup.Value.nameRule == NameRule.FileName ? "分包:" : "整包:";
                if (GUILayout.Button(buttonText, GUILayout.ExpandWidth(false)))
                {
                    // 切换NameRule
                    var newRule = firstGroup.Value.nameRule == NameRule.FileName ? NameRule.Group : NameRule.FileName;
                    foreach (var group in groups)
                    {
                        group.Value.nameRule = newRule;
                    }
                    AssetBundleSetting.Save();
                }

                // 显示第一个组的bundle名称进行编辑
                var newbundlePath = EditorGUILayout.TextField(bundlePath, GUILayout.ExpandWidth(true));
                if (newbundlePath != bundlePath)
                {
                    // 如果有多个组，更新所有组的bundle名称
                    foreach (var group in groups)
                    {
                        SetBundleName(path, group.Key, newbundlePath);
                    }
                }

                // 如果有多个组，显示组数量提示
                if (groups.Count > 1)
                {
                    var style = new GUIStyle(GUI.skin.label);
                    style.normal.textColor = Color.yellow;
                    GUILayout.Label($"({groups.Count}组)", style, GUILayout.Width(40));
                }

                var maskId = GetGroupMask(groupNames);
                var newMaskId = EditorGUILayout.MaskField("", maskId, _groups, GUILayout.Width(100));
                if (newMaskId != maskId)
                {
                    SetGroupMask(newMaskId, path, bundlePath);
                }
            }

            GUI.backgroundColor = originalColor;
        }

        private static void DrawTitleInfoAdd(string bundleName, string path)
        {

            // 未添加状态 - 黄色背景显示
            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 1f, 0.7f, 0.5f); // 浅黄色背景

            using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                // 显示警告图标和标签
                GUILayout.Label(EditorGUIUtility.IconContent("d_console.warnicon.sml", "未配置AssetBundle"), GUILayout.Width(20));
                GUILayout.Label($"路径:", GUILayout.ExpandWidth(false));

                if (!_cacheName.TryGetValue(path, out var defaultName))
                    defaultName = path;
                defaultName = EditorGUILayout.TextField(defaultName, GUILayout.ExpandWidth(true));
                _cacheName[path] = defaultName;

                // 突出显示的添加按钮
                var buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.normal.textColor = Color.white;
                var oldBgColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f, 1f); // 绿色按钮

                if (GUILayout.Button("+ 添加", buttonStyle, GUILayout.Width(50)))
                {
                    var menu = new GenericMenu();
                    for (int i = 0; i < _groups.Length; i++)
                    {
                        var groupIndex = i; // 捕获循环变量
                        menu.AddItem(new GUIContent(_groups[i]), false, () =>
                        {
                            SetGroupMask(1 << groupIndex, path, defaultName);
                        });
                    }
                    menu.ShowAsContext();
                }

                GUI.backgroundColor = oldBgColor;
            }

            GUI.backgroundColor = originalColor;
        }

        private static void SetBundleName(string assetPath, AssetBundleGroup group, string newBundleName)
        {
            if (string.IsNullOrEmpty(newBundleName))
                return;

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var info = group.infos.Find(x => x.guid == guid || x.assetPath == assetPath);
            if (info != null)
            {
                // 如果bundlePath和bundleName相同，也更新bundlePath
                info.bundlePath = newBundleName;
                AssetBundleSetting.Save();
                EditorUtility.SetDirty(AssetBundleSetting.Instance);
            }
        }

        private static bool CheckBundleEnable(string bundlePath)
        {
            // 检查指定bundlePath的资源是否启用
            foreach (var group in AssetBundleSetting.Instance.groups)
            {
                var info = group.infos.Find(x => x.bundlePath == bundlePath);
                if (info != null)
                {
                    return !info.disable; // disable字段为false表示启用
                }
            }
            return true; // 默认启用
        }

        private static void MarkBundleEnable(string bundlePath, bool enabled)
        {
            bool hasChanges = false;

            // 更新所有匹配bundlePath的资源的enable状态
            foreach (var group in AssetBundleSetting.Instance.groups)
            {
                var info = group.infos.Find(x => x.bundlePath == bundlePath);
                if (info != null)
                {
                    info.disable = !enabled; // enabled为true时disable为false
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                // 保存设置
                AssetBundleSetting.Save();
                EditorUtility.SetDirty(AssetBundleSetting.Instance);
            }
        }

        private static void SetGroupMask(int newMaskId, string path, string customBundleName)
        {
            _cacheName[path] = customBundleName;
            var guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid))
                return;

            // 根据新的maskId确定哪些组被选中
            var selectedGroups = new List<string>();
            for (int i = 0; i < _groups.Length; i++)
            {
                if ((newMaskId & (1 << i)) != 0)
                {
                    selectedGroups.Add(_groups[i]);
                }
            }

            // 更新资源在各组中的状态
            for (int i = 0; i < AssetBundleSetting.Instance.groups.Count; i++)
            {
                var group = AssetBundleSetting.Instance.groups[i];
                var shouldBeInGroup = selectedGroups.Contains(group.name);
                var existingInfo = group.infos.Find(x => x.guid == guid);

                if (shouldBeInGroup && existingInfo == null)
                {
                    // 需要添加到组中，但当前不在组中
                    var bundleName = !string.IsNullOrEmpty(customBundleName) ? customBundleName : Path.GetFileNameWithoutExtension(path);
                    var newInfo = new AssetBundleInfo
                    {
                        guid = guid,
                        assetPath = path,
                        bundlePath = bundleName,
                        disable = false
                    };
                    group.infos.Add(newInfo);
                }
                else if (!shouldBeInGroup && existingInfo != null)
                {
                    // 不应该在组中，但当前在组中
                    group.infos.Remove(existingInfo);
                }
            }

            // 保存设置
            AssetBundleSetting.Save();

            // 刷新界面
            EditorUtility.SetDirty(AssetBundleSetting.Instance);
        }

        private static int GetGroupMask(string[] groupNames)
        {
            int maskId = 0;
            for (int i = 0; i < _groups.Length; i++)
            {
                var group = _groups[i];
                if (Array.IndexOf(groupNames, group) >= 0)
                {
                    maskId += 1 << i;
                }
            }
            return maskId;
        }

        private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            if (!AssetBundleSetting.Instance.projectwindowGui)
                return;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            groups.Clear();
            bool editable = true;
            var bundlePath = GetImplicitBundleInfo(path, groups, ref editable);
            if (string.IsNullOrEmpty(bundlePath))
                return;
            var enabled = true;
            foreach (var group in groups)
            {
                if (group.Value.disable)
                    enabled = false;
            }
            var iconRect = new Rect(selectionRect.x + selectionRect.width - 10, selectionRect.y, 10, 12);
            var color = GUI.color;
            GUI.color = enabled ? Color.green : Color.gray;
            GUI.DrawTexture(iconRect, s_packIcon.image);
            GUI.color = color;
        }

        public static string GetImplicitBundleInfo(string assetPath, Dictionary<AssetBundleGroup, AssetBundleInfo> groups, ref bool editable)
        {
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var groupInfos = AssetBundleSetting.Instance.groups.FindAll(x => x.infos.Find(y => y.guid == guid) != null);
            if (groupInfos != null && groupInfos.Count > 0)
            {
                foreach (var item in groupInfos)
                {
                    groups[item] = item.infos.Find(x => x.guid == guid);
                }
            }

            if (groups.Count == 0)
            {
                editable = false;
                var projectFolder = Path.GetFullPath(Environment.CurrentDirectory);
                if (!string.IsNullOrEmpty(assetPath) && Path.GetFullPath(assetPath) != projectFolder)
                {
                    return GetImplicitBundleInfo(Path.GetDirectoryName(assetPath), groups, ref editable);
                }
            }
            else
            {
                return groups.First().Value.bundlePath;
            }
            return null;
        }
    }
}
#endif
