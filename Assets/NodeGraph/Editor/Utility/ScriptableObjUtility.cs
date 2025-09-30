/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_5122a2                                                                *
*  功能:                                                                              *
*   - 资源缓存器工具                                                                  *
*//************************************************************************************/

using UnityEngine;
using UnityEditor;

namespace UFrame.NodeGraph
{
    public class ScriptableObjUtility
    {
        public static void SetSubAssets(ScriptableObject[] subAssets, ScriptableObject mainAsset,bool clearOther = false,HideFlags hideFlags = HideFlags.None)
        {
            var path = AssetDatabase.GetAssetPath(mainAsset);
            var oldAssets = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (ScriptableObject subAsset in subAssets)
            {
                if (subAsset == mainAsset) continue;
                UnityEditor.EditorUtility.SetDirty(subAsset);
                if (System.Array.Find(oldAssets, x => x == subAsset) == null)
                {
                    AddSubAsset(subAsset, mainAsset, hideFlags);
                }
            }

            if(clearOther)
            {
                ClearSubAssets(mainAsset, subAssets);
            }
        }

        /// <summary>
        /// Adds the specified hidden subAsset to the mainAsset
        /// </summary>
        public static void AddSubAsset(ScriptableObject subAsset, ScriptableObject mainAsset,HideFlags hideFlag)
        {
            if (subAsset != null && mainAsset != null)
            {
                UnityEditor.AssetDatabase.AddObjectToAsset(subAsset, mainAsset);
                subAsset.hideFlags = hideFlag;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainAsset"></param>
        public static void ClearSubAssets(ScriptableObject mainAsset, ScriptableObject[] ignores = null)
        {
            if (mainAsset != null)
            {
                var path = AssetDatabase.GetAssetPath(mainAsset);
                var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

                foreach (var item in subAssets)
                {
                    if (item == mainAsset) continue;

                    if (ignores == null || System.Array.Find(ignores, x => x == item) == null)
                    {
                        if(item != null)
                        {
                            AssetDatabase.RemoveObjectFromAsset(item);
                        }
                        else
                        {
                            Debug.LogError("exists empty sub asset!");
                        }
                    }
                }
            }
        }
    }
}