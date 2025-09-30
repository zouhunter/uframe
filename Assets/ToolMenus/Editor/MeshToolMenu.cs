using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace UFrame.Editors
{
    public class MeshToolMenu
    {
        protected static int selectCount = 0;

        protected static bool NeedWait()
        {
            if (selectCount > 0)
            {
                selectCount--;
                return true;
            }
            selectCount = Selection.objects.Length - 1;
            return false;
        }

        [MenuItem("GameObject/UFrame/MeshTool/CreateMesh", priority = 0, validate = false)]
        public static void CreateMeshAssets()
        {
            if (Selection.activeGameObject)
            {
                var meshFiletrs = Selection.activeGameObject.GetComponentsInChildren<MeshFilter>(true);
                var assetname = Selection.activeGameObject;
                if (meshFiletrs.Length > 0)
                {
                    var firstMesh = meshFiletrs[0].sharedMesh;
                    firstMesh.name = "mesh0";
                    var path = string.Format("Assets/Arts/Meshs/{0}.asset", assetname.name);
                    AssetDatabase.CreateAsset(firstMesh, path);
                    AssetDatabase.Refresh();
                    firstMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    Debug.LogWarning("Created mesh:" + path);
                    for (int i = 1; i < meshFiletrs.Length; i++)
                    {
                        meshFiletrs[i].sharedMesh.name = "mesh" + i;
                        try
                        {
                            AssetDatabase.AddObjectToAsset(meshFiletrs[i].sharedMesh, path);
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }
                    AssetDatabase.Refresh();
                    EditorUtility.SetDirty(firstMesh);
                }
            }
        }

        [MenuItem("GameObject/UFrame/MeshTool/MergeMeshs", priority = 0, validate = false)]
        public static void MergeMesh()
        {
            if (NeedWait())
                return;

            List<MeshFilter> filters = new List<MeshFilter>();
            var objects = Selection.objects;
            Vector3 centerPos = Vector3.zero;
            int numItems = 0;
            string combineName = "";
            for (int i = 0; i < objects.Length; i++)
            {
                var obj = objects[i] as GameObject;
                if (obj)
                {
                    var fs = obj.GetComponentsInChildren<MeshFilter>();
                    if (fs != null && fs.Length > 0)
                    {
                        filters.AddRange(fs);
                        centerPos += obj.transform.position;
                        numItems += 1;
                    }
                    combineName = obj.name;
                }
            }

            if (numItems > 0)
            {
                //新建材质球数组
                List<Material> mats = new List<Material>();
                for (int i = 0; i < filters.Count; i++)
                {
                    var meshRender = filters[i].GetComponent<MeshRenderer>();
                    if (meshRender && !mats.Contains(meshRender.sharedMaterial))
                    {
                        mats.Add(meshRender.sharedMaterial);
                    }
                }

                centerPos /= numItems;
                //获取子物体MeshFilter组件
                CombineInstance[] combines = new CombineInstance[filters.Count];
                for (int i = 0; i < filters.Count; i++)
                {
                    combines[i].mesh = filters[i].sharedMesh;
                    combines[i].transform = Matrix4x4.Translate(-centerPos) * filters[i].transform.localToWorldMatrix;
                }
                Mesh finalMesh = new Mesh();
                finalMesh.CombineMeshes(combines, true, true);
                var go = new GameObject(combineName, typeof(MeshFilter), typeof(MeshRenderer));
                go.GetComponent<MeshFilter>().sharedMesh = finalMesh;
                go.GetComponent<MeshRenderer>().sharedMaterials = mats.ToArray();
            }
        }
    }
}