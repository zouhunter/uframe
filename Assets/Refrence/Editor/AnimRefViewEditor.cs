using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UFrame.Refrence
{
    [CustomEditor(typeof(AnimClipRefView), true)]
    public class AnimRefViewEditor : ObjectRefEditor
    {
        protected override void FindSubObjectsBySingle(Object obj, List<Object> list)
        {
            if (obj is AnimationClip)
            {
                list.Add(obj);
            }
            else if (obj is RuntimeAnimatorController)
            {
                var ctrl = obj as RuntimeAnimatorController;
                if (ctrl.animationClips != null && ctrl.animationClips.Length > 0)
                {
                    list.AddRange(ctrl.animationClips);
                }
            }
            else if (obj is GameObject)
            {
                FindSubObjectsByGameObject(obj as GameObject, list);
            }
        }

        protected override bool CheckPlaceAble(Object obj)
        {
            if (obj is AnimationClip)
                return true;
            if (obj is GameObject)
                return true;
            if (obj is RuntimeAnimatorController)
                return true;
            if (obj is DefaultAsset)
                return true;
            return false;
        }

        protected override bool CheckIsValid(Object obj)
        {
            return obj is AudioClip;
        }

        protected void FindSubObjectsByGameObject(GameObject go, List<Object> list)
        {
            var animation = go.GetComponent<Animation>();
            var animator = go.GetComponent<Animator>();
            if (animation)
            {
                var enumerator = animation.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var state = enumerator.Current as AnimationState;
                    if (state.clip)
                    {
                        list.Add(state.clip);
                    }
                }
                if (animation.clip)
                {
                    list.Add(animation.clip);
                }
            }
            else if (animator && animator.runtimeAnimatorController)
            {
                var clips = animator.runtimeAnimatorController.animationClips;
                if (clips != null && clips.Length > 0)
                {
                    list.AddRange(clips);
                }
            }
            else
            {
                var assetType = PrefabUtility.GetPrefabAssetType(go);
                if (assetType == PrefabAssetType.Model)
                {
                    var path = AssetDatabase.GetAssetPath(go);
                    ImportAnimationFromPath(path, list);
                }
                else
                {
                    Debug.LogError(assetType + " not support!");
                }
            }
        }

        protected void ImportAnimationFromPath(string path, List<Object> list)
        {
            var importer = AssetImporter.GetAtPath(path);
            if (importer is ModelImporter)
            {
                var modelImporter = importer as ModelImporter;
                var modelAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var item in modelAssets)
                {
                    if (item is AnimationClip && System.Array.Find(modelImporter.clipAnimations, x => x.name == item.name) != null)
                    {
                        list.Add(item);
                    }
                }
            }
        }

    }
}
