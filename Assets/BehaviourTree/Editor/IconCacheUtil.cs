using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor;

using UnityEngine;
using System;


namespace UFrame.BehaviourTree
{
    public class IconCacheUtil
    {
        private string _actionIcon = "41b36d97da5a09a49b4a35a6fd18c506";
        private string _conditionIcon = "05c3c1cf306a9264a859ae5258b19dd2";
        private string _compositeIcon = "39d36ca6efc896a4991c5d0e79d2ee60";
        private string _directorIcon = "8171d41762a45fc4689821ca7e4b46fa";
        private string _treeIcon = "29c333b9e07329e4e893932b7484cf5e";
        private string _variantTreeIcon = "2dbb98af2ef5ce140a62652fa5ce2fba";
        private Dictionary<string, Texture2D> _guidIcons;
        static IconCacheUtil util;
        private Dictionary<BaseNode, Texture2D> _nodeTextures = new Dictionary<BaseNode, Texture2D>();
        private Dictionary<BTree, Texture2D> _treeTextures = new Dictionary<BTree, Texture2D>();
        private float _lastSelectScriptTime;
        static IconCacheUtil()
        {
            util = new IconCacheUtil();
            util._guidIcons = new Dictionary<string, Texture2D>();
        }

        public static Texture2D GetTaskIcon(TaskType type)
        {
            switch (type)
            {
                case TaskType.Action:
                    return GetTextureByGUID(util._actionIcon);
                case TaskType.Condition:
                    return GetTextureByGUID(util._conditionIcon);
                case TaskType.Composite:
                    return GetTextureByGUID(util._compositeIcon);
                case TaskType.Deractor:
                    return GetTextureByGUID(util._directorIcon);
                default:
                    break;
            }
            return null;
        }

        public static Texture2D GetTextureByGUID(string guid)
        {
            if (!util._guidIcons.TryGetValue(guid, out Texture2D icon))
            {
                icon = util._guidIcons[guid] = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid));
            }
            return icon;
        }

        /// <summary>
        /// 双击打脚本
        /// </summary>
        /// <param name="iconRect"></param>
        /// <param name="node"></param>
        public static void DrawIcon(Rect iconRect, BaseNode node)
        {
            if (!util._nodeTextures.TryGetValue(node, out var texture))
            {
                texture = util._nodeTextures[node] = FindNodeIconFromNode(node);
            }
            if (texture != null)
            {

                if (GUI.Button(iconRect, new GUIContent(texture)))
                {
                    OpenEditScript(node.GetType(), util._lastSelectScriptTime != System.DateTime.Now.Second);
                    util._lastSelectScriptTime = System.DateTime.Now.Second;
                }
            }
        }


        /// <summary>
        /// 双击子树
        /// </summary>
        public static bool DrawSubTree(Rect iconRect, BTree tree)
        {
            if (!tree)
                return false;

            if (!util._treeTextures.TryGetValue(tree, out var texture))
            {
                texture = util._treeTextures[tree] = FindTreeIcon(tree);
            }
            if (texture != null)
            {
                if (GUI.Button(iconRect, new GUIContent(texture)))
                {
                    if (util._lastSelectScriptTime != System.DateTime.Now.Second)
                    {
                        Selection.activeObject = tree;
                        EditorGUIUtility.PingObject(tree);
                    }
                    else
                    {
                        AssetDatabase.OpenAsset(tree);
                    }
                    util._lastSelectScriptTime = System.DateTime.Now.Second;
                }
                return true;
            }
            return false;
        }


        private static void OpenEditScript(Type type, bool locateOnly)
        {
            string[] guids = AssetDatabase.FindAssets($"{type.Name} t:script");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && script.GetClass() == type)
                {
                    if (locateOnly)
                    {
                        EditorGUIUtility.PingObject(script);
                    }
                    else
                    {
                        AssetDatabase.OpenAsset(script);
                    }
                }
            }
        }

        private static Texture2D FindTreeIcon(BTree tree)
        {
            if (tree is BVariantTree)
            {
                return IconCacheUtil.GetTextureByGUID(util._variantTreeIcon);
            }
            else
            {
                return IconCacheUtil.GetTextureByGUID(util._treeIcon);
            }
        }

        public static Texture2D FindNodeIconFromNode(BaseNode node)
        {
            var type = node.GetType();
            var iconAttr = type.GetCustomAttribute<IconAttribute>();
            if (iconAttr != null)
            {
                return IconCacheUtil.GetTextureByGUID(iconAttr.path);
            }
            if (node is ActionNode)
                return IconCacheUtil.GetTaskIcon(TaskType.Action);
            else if (node is ConditionNode)
                return IconCacheUtil.GetTaskIcon(TaskType.Condition);
            else if (node is CompositeNode)
                return IconCacheUtil.GetTaskIcon(TaskType.Composite);
            else if (node is DecoratorNode)
                return IconCacheUtil.GetTaskIcon(TaskType.Deractor);
            return IconCacheUtil.GetTextureByGUID(util._actionIcon);
        }

    }
}
