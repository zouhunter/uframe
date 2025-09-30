/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 参数设定                                                                        *
*//************************************************************************************/
using UnityEngine;
using UnityEditor;

namespace UFrame.BridgeUI
{
    public static class UISetting
    {
        private static string prefer_bundle_name_format = "bridgeui_setting_prefer_bundle_name_format";
        private static string _bundleNameformat;
        public static string bundleNameFormat
        {
            get
            {
                if (string.IsNullOrEmpty(_bundleNameformat))
                {
                    if (!EditorPrefs.HasKey(prefer_bundle_name_format))
                    {
                        _bundleNameformat = "bridgeui/panels/{0}.assetbundle";
                    }
                    else
                    {
                        _bundleNameformat = EditorPrefs.GetString(prefer_bundle_name_format);
                    }
                }
                return _bundleNameformat;
            }
            set
            {
                if (_bundleNameformat != value)
                {
                    _bundleNameformat = value;
                    EditorPrefs.SetString(prefer_bundle_name_format, _bundleNameformat);
                }
            }
        }

        private static string prefer_script_path = "bridgeui_setting_prefer_script_path";
        private static string _script_path;
        public static string script_path
        {
            get
            {
                if (string.IsNullOrEmpty(_script_path))
                {
                    if (!EditorPrefs.HasKey(prefer_script_path))
                    {
                        _script_path = "Assets/Scripts/UI";
                    }
                    else
                    {
                        _script_path = EditorPrefs.GetString(prefer_script_path);
                    }
                    if (!System.IO.Directory.Exists(_script_path))
                    {
                        System.IO.Directory.CreateDirectory(_script_path);
                    }
                }
                return _script_path;
            }
            set
            {
                if (_script_path != value)
                {
                    _script_path = value;
                    EditorPrefs.SetString(prefer_script_path, _script_path);
                }
            }
        }


        private static string prefer_prefab_path = "bridgeui_setting_prefer_prefab_path";
        private static string _prefab_path;
        public static string prefab_path
        {
            get
            {
                if (string.IsNullOrEmpty(_prefab_path))
                {
                    if (!EditorPrefs.HasKey(prefer_prefab_path))
                    {
                        _prefab_path = "Assets/Resources/Prefabs/UI";
                    }
                    else
                    {
                        _prefab_path = EditorPrefs.GetString(prefer_prefab_path);
                    }
                    if (!System.IO.Directory.Exists(_prefab_path))
                    {
                        System.IO.Directory.CreateDirectory(_prefab_path);
                    }
                }
                return _prefab_path;
            }
            set
            {
                if (_prefab_path != value)
                {
                    _prefab_path = value;
                    EditorPrefs.SetString(prefer_prefab_path, _prefab_path);
                }
            }
        }


        private static string prefer_defult_nameSpace = "bridgeui_setting_prefer_defult_nameSpace";
        private static string _defultNameSpace;
        public static string defultNameSpace
        {
            get
            {
                if (string.IsNullOrEmpty(_defultNameSpace))
                {
                    if (!EditorPrefs.HasKey(prefer_defult_nameSpace))
                    {
                        _defultNameSpace = "View";
                    }
                    else
                    {
                        _defultNameSpace = EditorPrefs.GetString(prefer_defult_nameSpace);
                    }
                }
                return _defultNameSpace;
            }
            set
            {
                if (_defultNameSpace != value)
                {
                    _defultNameSpace = value;
                    EditorPrefs.SetString(prefer_defult_nameSpace, _defultNameSpace);
                }
            }
        }

        private static string prefer_common_nameSpace = "bridgeui_setting_prefer_common_nameSpace";
        private static string _commonNameSpace;
        public static string commonNameSpace
        {
            get
            {
                if (string.IsNullOrEmpty(_commonNameSpace))
                {
                    if (!EditorPrefs.HasKey(prefer_common_nameSpace))
                    {
                        _commonNameSpace = "Common";
                    }
                    else
                    {
                        _commonNameSpace = EditorPrefs.GetString(prefer_common_nameSpace);
                    }
                }
                return _commonNameSpace;
            }
            set
            {
                if (_commonNameSpace != value)
                {
                    _commonNameSpace = value;
                    EditorPrefs.SetString(prefer_common_nameSpace, _commonNameSpace);
                }
            }
        }


        private static string prefer_user_name = "bridgeui_setting_prefer_user_name";
        private static string _userName;
        public static string userName
        {
            get
            {
                if (string.IsNullOrEmpty(_userName))
                {
                    if (!EditorPrefs.HasKey(prefer_user_name))
                    {
                        _userName = "zouhunter";
                    }
                    else
                    {
                        _userName = EditorPrefs.GetString(prefer_user_name);
                    }
                }
                return _userName;
            }
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    EditorPrefs.SetString(prefer_user_name, _userName);
                }
            }
        }
    }
}