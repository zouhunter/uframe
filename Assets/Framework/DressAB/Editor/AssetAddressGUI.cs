//*************************************************************************************
//* 作    者： zouhunter
//* 创建时间： 2023-05-19
//* 描    述：

//* ************************************************************************************
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using Object = UnityEngine.Object;
using UnityEditorInternal;

namespace UFrame.DressAssetBundle.Editors
{
    [InitializeOnLoad]
    internal static class AssetAddressGUI
    {
        static GUIStyle s_toggleMixed;
        static GUIContent s_toggleText;
        static GUIContent s_togglesText;
        static GUIContent s_addresIcon;
        static GUIContent s_deleteIcon;
        static GUIContent s_packIcon;
        static GUIContent s_packIconDisable;
        static GUIContent s_refIcon;

        static Stack<AddressGatherInfo> s_gatherInfoPool = new Stack<AddressGatherInfo>();
        static List<AddressGatherInfo> s_gatherInfos = new List<AddressGatherInfo>();
        static string[] exclude = new string[] { ".cs", ".js", ".boo", ".exe", ".dll", ".meta", ".preset", ".asmdef" };
        private static AddressDefineObject[] addressDefineObjs;

        static AssetAddressGUI()
        {
            s_toggleText = new GUIContent("Address:");
            s_togglesText = new GUIContent("Address`s:");
            s_addresIcon = EditorGUIUtility.IconContent("d_ScriptableObject Icon", "locate");
            s_deleteIcon = EditorGUIUtility.IconContent("d_TreeEditor.Trash", "delete");
            s_packIcon = EditorGUIUtility.IconContent("d_CacheServerConnected@2x", "address");
            s_packIconDisable = EditorGUIUtility.IconContent("d_CacheServerDisabled@2x", "address");
            s_refIcon = EditorGUIUtility.IconContent("d_PreMatCylinder@2x", "ref");
            Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
        }

        [UnityEngine.TestTools.ExcludeFromCoverage]
        static void OnPostHeaderGUI(Editor editor)
        {
            EditorGUI.EndDisabledGroup();
            var changed = false;
            if (editor.targets.Length > 0)
            {
                // only display for the Prefab/Model importer not the displayed GameObjects
                if (editor.targets[0].GetType() == typeof(GameObject))
                    return;

                foreach (var t in editor.targets)
                {
                    if (t is AddressDefineObject)
                        return;
                }

                bool exists = GatherTargetInfos(editor.targets, out var activeDefObj);
                if (!exists)
                    return;

                var addressedCount = s_gatherInfos.FindAll(x => x.addressInfo != null && x.addressInfo.active).Count;
                using (var hor = new EditorGUILayout.HorizontalScope())
                {
                    s_toggleMixed = s_toggleMixed ?? new GUIStyle("ToggleMixed");

                    var firstInfo = s_gatherInfos[0];

                    if (s_gatherInfos.Count > 1)
                    {
                        using (var change = new EditorGUI.ChangeCheckScope())
                        {
                            var state = false;
                            if (addressedCount == s_gatherInfos.Count)
                            {
                                state = GUILayout.Toggle(true, s_toggleText, GUILayout.ExpandWidth(false));
                            }
                            else if (addressedCount == 0)
                            {
                                state = GUILayout.Toggle(false, s_toggleText, GUILayout.ExpandWidth(false));
                            }
                            else
                            {
                                state = GUILayout.Toggle(false, s_toggleText, s_toggleMixed, GUILayout.ExpandWidth(false));
                            }
                            if (change.changed)
                            {
                                changed = true;
                                foreach (var item in s_gatherInfos)
                                {
                                    SetActiveAddress(state, activeDefObj, item);
                                }
                            }
                        }
                    }
                    else
                    {
                        var active = firstInfo.addressInfo != null && firstInfo.addressInfo.active;
                        using (var change = new EditorGUI.ChangeCheckScope())
                        {
                            var textContent = s_toggleText;
                            if (firstInfo.addressInfo != null && firstInfo.addressInfo.split)
                                textContent = s_togglesText;

                            active = GUILayout.Toggle(active, textContent, GUILayout.ExpandWidth(false));
                            if (change.changed)
                            {
                                SetActiveAddress(active, activeDefObj, firstInfo);
                            }
                        }
                    }

                    if (s_gatherInfos.Count == 1)
                    {
                        bool inAddress = firstInfo.addressInfo != null;
                        bool inRef = firstInfo.assetRefBundle != null;
                        if (inAddress && !inRef)
                        {
                            if (Directory.Exists(firstInfo.path))
                            {
                                var split = EditorGUILayout.Toggle(firstInfo.addressInfo.split, EditorStyles.radioButton, GUILayout.Width(20));
                                if (firstInfo.addressInfo.split != split)
                                {
                                    firstInfo.addressInfo.split = split;
                                    changed = true;
                                }
                            }

                            using (var disableGroup = new EditorGUI.DisabledGroupScope(!firstInfo.addressInfo.active))
                            {
                                var address = EditorGUILayout.TextField(firstInfo.addressInfo.address);
                                if (address != firstInfo.addressInfo.address)
                                {
                                    Undo.RecordObject(activeDefObj, "delete address");
                                    firstInfo.addressInfo.address = address;
                                    changed = true;
                                }
                            }
                        }
                        else if (inRef)
                        {
                            var address = EditorGUILayout.TextField(firstInfo.assetRefBundle.address);
                            if (address != firstInfo.assetRefBundle.address)
                            {
                                Undo.RecordObject(activeDefObj, "delete address");
                                var refBundleOld = activeDefObj.refBundleList.Find(x => x.address == address && x != firstInfo.assetRefBundle);
                                if (refBundleOld != null)
                                {
                                    firstInfo.assetRefBundle.guids.Remove(firstInfo.guid);
                                    if (firstInfo.assetRefBundle.guids.Count == 0)
                                    {
                                        activeDefObj.refBundleList.Remove(firstInfo.assetRefBundle);
                                    }
                                    refBundleOld.guids.Add(firstInfo.guid);
                                }
                                else
                                {
                                    if (firstInfo.assetRefBundle.guids.Count == 1)
                                    {
                                        firstInfo.assetRefBundle.address = address;
                                    }
                                    else
                                    {
                                        var refBundle = new AssetRefBundle();
                                        refBundle.address = address;
                                        refBundle.guids.Add(firstInfo.guid);
                                        RemoveRefBundle(activeDefObj, firstInfo.guid);
                                        activeDefObj.refBundleList.Add(refBundle);
                                        firstInfo.assetRefBundle = refBundle;
                                    }
                                }
                            }
                            changed = true;
                        }
                        else
                        {
                            using (var disableGroup = new EditorGUI.DisabledGroupScope(true))
                            {
                                var previewAddress = GetPreviewAddress(firstInfo.path, activeDefObj);
                                EditorGUILayout.SelectableLabel(previewAddress, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                            }
                        }

                        if (inRef && inAddress && firstInfo.addressInfo.active)
                        {
                            firstInfo.addressInfo.address = firstInfo.assetRefBundle.address;
                            RemoveRefBundle(activeDefObj, firstInfo.guid);
                            firstInfo.assetRefBundle = null;
                        }

                        using (var disableGroup = new EditorGUI.DisabledGroupScope(false))
                        {
                            using (var change = new EditorGUI.ChangeCheckScope())
                            {
                                inRef = EditorGUILayout.Toggle(inRef, GUILayout.Width(20));
                                if (change.changed)
                                {
                                    if (inRef)
                                    {
                                        var address = firstInfo.path.Replace(".", "_");
                                        if (inAddress)
                                        {
                                            address = firstInfo.addressInfo.address;
                                            activeDefObj.addressList.Remove(firstInfo.addressInfo);
                                            firstInfo.addressInfo = null;
                                        }
                                        var refBundle = activeDefObj.refBundleList.Find(x => x.address == address);
                                        if (refBundle == null)
                                        {
                                            refBundle = new AssetRefBundle();
                                            refBundle.address = address;
                                            activeDefObj.refBundleList.Add(refBundle);
                                        }
                                        if (!refBundle.guids.Contains(firstInfo.guid))
                                        {
                                            refBundle.guids.Add(firstInfo.guid);
                                        }
                                        firstInfo.assetRefBundle = refBundle;
                                    }
                                    else if (!inRef)
                                    {
                                        RemoveRefBundle(activeDefObj, firstInfo.guid);
                                        firstInfo.assetRefBundle = null;
                                    }
                                }
                                changed = true;
                            }
                        }
                        if (!inRef && inAddress)
                        {
                            using (var disableGroup = new EditorGUI.DisabledGroupScope(!firstInfo.addressInfo.active))
                            {
                                var flags = (ushort)EditorGUILayout.MaskField(firstInfo.addressInfo.flags, activeDefObj.flags, GUILayout.Width(120));
                                if (firstInfo.addressInfo.flags != flags)
                                {
                                    firstInfo.addressInfo.flags = flags;
                                    changed = true;
                                }
                            }
                        }

                        if (GUILayout.Button(s_addresIcon, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(20)))
                        {
                            EditorGUIUtility.PingObject(activeDefObj);
                        }

                        if (inAddress || inRef)
                        {
                            Undo.RecordObject(activeDefObj, "delete address");
                            if (GUILayout.Button(s_deleteIcon, EditorStyles.label, GUILayout.Width(20), GUILayout.Height(20)))
                            {
                                if (inAddress)
                                    activeDefObj.addressList.Remove(firstInfo.addressInfo);
                                if (inRef)
                                {
                                    RemoveRefBundle(activeDefObj, firstInfo.guid);
                                }
                                firstInfo.assetRefBundle = null;
                                firstInfo.addressInfo = null;
                                changed = true;
                            }
                        }

                    }
                    else if (s_gatherInfos.Count > 1)
                    {
                        EditorGUILayout.LabelField("select element num:" + s_gatherInfos.Count + " address num:" + addressedCount);
                    }
                }
                if (changed && activeDefObj)
                    EditorUtility.SetDirty(activeDefObj);
            }
        }

        private static AddressDefineObject FindAddressDefineObject(string guid)
        {
            var activeDefObj = AddressDefineObjectSetting.Instance.activeAddressDefineObject;
            if (CheckContans(activeDefObj, guid))
                return activeDefObj;

            if (addressDefineObjs == null)
            {
                var guids = AssetDatabase.FindAssets("t:AddressDefineObject");
                addressDefineObjs = new AddressDefineObject[guids.Length];
                for (int i = 0; i < guids.Length; i++)
                {
                    addressDefineObjs[i] = AssetDatabase.LoadAssetAtPath<AddressDefineObject>(AssetDatabase.GUIDToAssetPath(guids[i]));
                }
            }

            for (int i = 0; i < addressDefineObjs.Length; i++)
            {
                var currentAddDefineObj = addressDefineObjs[i];
                if (!currentAddDefineObj || currentAddDefineObj == activeDefObj)
                    continue;
                if (CheckContans(currentAddDefineObj, guid))
                    return currentAddDefineObj;
            }
            return activeDefObj;
        }

        private static bool CheckContans(AddressDefineObject addressObj, string guid)
        {
            if (addressObj?.addressList?.Find(x => x.guid == guid) != null)
                return true;
            if (addressObj?.refBundleList?.Find(x => x.guids.Contains(guid)) != null)
                return true;
            return false;
        }

        private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            var activeDefObj = FindAddressDefineObject(guid);
            if (activeDefObj == null)
                return;
            var isMainDefine = activeDefObj == AddressDefineObjectSetting.Instance.activeAddressDefineObject;
            var addressName = GetImplicitAddressName(activeDefObj, AssetDatabase.GUIDToAssetPath(guid), true);
            if (!string.IsNullOrEmpty(addressName))
            {
                var iconRect = new Rect(selectionRect.x + selectionRect.width - 10, selectionRect.y, 10, 12);
                var color = GUI.color;
                GUI.color = isMainDefine ? Color.green : Color.white;
                var addressInfo = activeDefObj.addressList.Find(x => x.address == addressName);
                if (addressInfo != null)
                {
                    GUI.DrawTexture(iconRect, addressInfo.active ? s_packIcon.image:s_packIconDisable.image);
                }
                else
                {
                    GUI.DrawTexture(iconRect, s_refIcon.image);
                }
                GUI.color = color;
            }
        }

        public static string GetAddressName(AddressDefineObject addressDefine, string assetPath, bool includeRef)
        {
            if (addressDefine != null)
            {
                var guid = AssetDatabase.GUIDFromAssetPath(assetPath).ToString();
                var info = addressDefine.addressList.Find(x => x.guid == guid);
                if (info != null)
                    return info.address;

                if (includeRef)
                {
                    var refBundle = addressDefine.refBundleList.Find(x => x.guids.Contains(guid));
                    if (refBundle != null)
                        return refBundle.address;
                }
            }
            return "";
        }

        public static string GetImplicitAddressName(AddressDefineObject addressDefine, string assetPath, bool includeRef)
        {
            var folder = assetPath;
            var projectFolder = System.IO.Path.GetFullPath(System.Environment.CurrentDirectory);
            while (!string.IsNullOrEmpty(folder) && System.IO.Path.GetFullPath(folder) != projectFolder)
            {
                var bundleName = GetAddressName(addressDefine, folder, includeRef);
                if (!string.IsNullOrEmpty(bundleName))
                    return bundleName;
                folder = System.IO.Path.GetDirectoryName(folder);
            }
            return "";
        }


        private static void SetActiveAddress(bool active, AddressDefineObject activeDefObj, AddressGatherInfo gatherInfo)
        {
            Debug.LogError("SetActiveAddress:" + active + "-" + gatherInfo.path);

            if (active)
            {
                AddressInfo addressInfo = activeDefObj.addressList.Find(x => x.guid == gatherInfo.guid);
                if (addressInfo == null)
                {
                    addressInfo = new AddressInfo();
                    addressInfo.address = GetPreviewAddress(gatherInfo.path, activeDefObj);
                    activeDefObj.addressList.Add(addressInfo);
                }
                addressInfo.guid = gatherInfo.guid;
                addressInfo.active = true;
            }
            else if (!active)
            {
                AddressInfo info = activeDefObj.addressList.Find(x => x.guid == gatherInfo.guid);
                info.active = false;
            }
        }

        public static string GetPreviewAddress(string path, AddressDefineObject addressDefObj)
        {
            var ext = System.IO.Path.GetExtension(path);
            var parent = path;
            string parentDressName = null;
            bool split = false;
            while (parent != null)
            {
                parent = System.IO.Path.GetDirectoryName(parent);
                if (string.IsNullOrEmpty(parent))
                    break;
                var rPath = System.IO.Path.GetRelativePath(System.Environment.CurrentDirectory, parent);
                if (string.IsNullOrEmpty(rPath))
                    break;

                var guid = AssetDatabase.GUIDFromAssetPath(rPath).ToString();
                if (string.IsNullOrEmpty(guid))
                    break;

                var addressInfo = addressDefObj.addressList.Find(x => x.guid == guid);
                var refBundleInfo = addressDefObj.refBundleList.Find(x => x.guids.Contains(guid));
                if (refBundleInfo != null)
                {
                    parentDressName = refBundleInfo.address;
                    break;
                }
                if (addressInfo != null)
                {
                    parentDressName = addressInfo.address;
                    split = addressInfo.split;
                    break;
                }
            }
            if (!string.IsNullOrEmpty(parentDressName))
            {
                if (split)
                {
                    path = path.Substring(0, path.Length - ext.Length);
                    return parentDressName + "/" + System.IO.Path.GetRelativePath(parent, path);
                }
                else
                {
                    return parentDressName + "/" + System.IO.Path.GetRelativePath(parent, path);
                }
            }
            return path.Replace(".", "_");
        }

        private static void RemoveRefBundle(AddressDefineObject defObj, string guid)
        {
            var refBundle = defObj.refBundleList.Find(x => x.guids.Contains(guid));
            if (refBundle != null)
            {
                refBundle.guids.Remove(guid);
                if (refBundle.guids.Count == 0)
                {
                    defObj.refBundleList.Remove(refBundle);
                }
                EditorUtility.SetDirty(defObj);
            }
        }

        internal static bool GatherTargetInfos(Object[] targets, out AddressDefineObject ado)
        {
            if (s_gatherInfos != null)
            {
                foreach (var item in s_gatherInfos)
                {
                    item.addressInfo = null;
                    item.assetRefBundle = null;
                    s_gatherInfoPool.Push(item);
                }
                s_gatherInfos.Clear();
            }

            ado = null;

            foreach (var t in targets)
            {
                if (TryGetPathAndGUIDFromTarget(t, out var path, out var guid))
                {
                    var currentAdo = FindAddressDefineObject(guid);

                    if (ado != null && currentAdo != ado)
                        return false;
                    ado = currentAdo;
                    if (!ado)
                        continue;
                    var mainAssetType = AssetDatabase.GetMainAssetTypeAtPath(path);
                    if (mainAssetType != null)
                    {
                        if (s_gatherInfos == null)
                            s_gatherInfos = new List<AddressGatherInfo>();

                        AddressGatherInfo gatherInfo;
                        if (s_gatherInfoPool.Count > 0)
                        {
                            gatherInfo = s_gatherInfoPool.Pop();
                        }
                        else
                            gatherInfo = new AddressGatherInfo();
                        gatherInfo.guid = guid;
                        gatherInfo.target = t;
                        gatherInfo.path = path;
                        var entry = ado.addressList.Find(x => x.guid == guid);
                        if (entry != null)
                        {
                            gatherInfo.addressInfo = entry;
                        }
                        var refBundle = ado.refBundleList.Find(x => x.guids.Contains(guid));
                        if (refBundle != null)
                        {
                            gatherInfo.assetRefBundle = refBundle;
                        }
                        s_gatherInfos.Add(gatherInfo);
                    }
                }
            }
            return s_gatherInfos != null && s_gatherInfos.Count > 0;
        }

        public static bool TryGetPathAndGUIDFromTarget(UnityEngine.Object target, out string path, out string guid)
        {
            guid = string.Empty;
            path = string.Empty;
            if (!(target is AssetImporter) && !AssetDatabase.IsMainAsset(target))
                return false;
            if (target == null)
                return false;
            path = AssetDatabase.GetAssetOrScenePath(target);
            if (Array.IndexOf(exclude, Path.GetExtension(path)) >= 0)
                return false;
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(target, out guid, out long id))
                return false;
            return true;
        }
    }

    public class AddressGatherInfo
    {
        public string guid;
        public Object target;
        public string path;
        public AddressInfo addressInfo;
        public AssetRefBundle assetRefBundle;
    }
}

