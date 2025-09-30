using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace UFrame.Editors
{//中文
    public class SceneEditToolMenu
    {
        protected static int selectCount = 0;
        protected static string TargetMatFolder = "Assets/Test-Materials";

        protected static bool CheckWaitMenu()
        {
            if (selectCount > 0)
            {
                selectCount--;
                return true;
            }
            selectCount = Selection.objects.Length - 1;
            return false;
        }

        protected static Transform GetColliderRoot()
        {
            var colliderRoot = GameObject.Find("[Collider]");
            if (!colliderRoot)
            {
                colliderRoot = new GameObject("[Collider]");
                colliderRoot.transform.position = Vector3.zero;
            }
            return colliderRoot.transform;
        }

        public static MeshCollider CreateCombineCollider(params Object[] objects)
        {
            if (objects.Length == 0)
                return null;

            List<MeshFilter> filters = new List<MeshFilter>();
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
                var parent = GetColliderRoot();
                centerPos /= numItems;
                //鑾峰彇瀛愮墿浣揗eshFilter缁勪欢
                CombineInstance[] combines = new CombineInstance[filters.Count];
                for (int i = 0; i < filters.Count; i++)
                {
                    combines[i].mesh = filters[i].sharedMesh;
                    combines[i].transform = Matrix4x4.Translate(-centerPos) * filters[i].transform.localToWorldMatrix;
                }
                Mesh finalMesh = new Mesh();
                finalMesh.CombineMeshes(combines, true, true);
                return CreateColliderWithMesh(centerPos, parent, combineName, finalMesh);
            }
            return null;
        }

        public static MeshCollider CreateColliderWithMesh(Vector3 center, Transform parent, string combineName, Mesh mesh)
        {
            var road = new GameObject(combineName);
            road.transform.SetParent(parent);
            road.transform.position = center;
            road.AddComponent<MeshFilter>().sharedMesh = mesh;
            var collider = road.AddComponent<MeshCollider>();
            collider.convex = false;
            return collider;
        }

        public static MeshCollider CreateCollider(Transform target, Transform parent)
        {
            var copyName = target.name + target.GetInstanceID();
            Transform copy = null;
            if (parent)
            {
                copy = parent.Find(copyName);
            }
            if (copy == null)
            {
                copy = GameObject.Instantiate(target);
                copy.SetParent(parent);
                copy.name = copyName;
            }
            copy.position = target.position;
            copy.rotation = target.rotation;
            var colliders = copy.GetComponents<Collider>();
            for (int i = 0; i < colliders.Length; i++)
            {
                Object.DestroyImmediate(colliders[i]);
            }
            var render = copy.GetComponent<Renderer>();
            if (render)
            {
                var collider = copy.gameObject.AddComponent<MeshCollider>();
                collider.convex = false;
                render.enabled = false;
                //Selection.activeObject = copy.gameObject;
                return collider;
            }
            return null;
        }

        public static void OpenCheckScene(string scenePath, System.Action<Scene> onCheck)
        {
            if (onCheck == null)
                return;

            Scene scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);
            if (scene != null && scene.IsValid())
            {
                onCheck.Invoke(scene);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
                UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);

                var currentScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
                if (currentScenes.Count(x => x.path == scenePath) <= 0)
                {
                    currentScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                    EditorBuildSettings.scenes = currentScenes.ToArray();
                }
            }
        }


        [MenuItem("GameObject/UFrame/SceneEditTool/ChildSortName", priority = 0, validate = false)]
        public static void ChildSortName()
        {
            var target = Selection.activeTransform;
            if (target != null)
            {
                for (int i = 0; i < target.childCount; i++)
                {
                    var child = target.GetChild(i);
                    child.name = target.name + i;
                }
            }
        }

        [MenuItem("GameObject/UFrame/SceneEditTool/RevertYZ", priority = 0, validate = false)]
        public static void RevertYZ()
        {
            var target = Selection.activeTransform;
            if (target != null)
            {
                for (int i = 0; i < target.childCount; i++)
                {
                    var child = target.GetChild(i);
                    var pos = child.transform.localPosition;
                    var z = pos.z;
                    pos.z = pos.y;
                    pos.y = z;
                    UnityEditor.Undo.RecordObject(child.transform, "RevertYZ");
                    child.transform.localPosition = pos;
                }
            }
        }

        [MenuItem("GameObject/UFrame/SceneEditTool/ClearAllY", priority = 0, validate = false)]
        public static void ClearAllY()
        {
            var target = Selection.activeTransform;
            if (target != null)
            {
                for (int i = 0; i < target.childCount; i++)
                {
                    var child = target.GetChild(i);
                    var pos = child.transform.localPosition;
                    pos.y = 0;
                    UnityEditor.Undo.RecordObject(child.transform, "ClearAllY");
                    child.transform.localPosition = pos;
                }
            }
        }

        [MenuItem("GameObject/UFrame/SceneEditTool/RemoveCollider", priority = 0, validate = false)]
        public static void AutoCollider()
        {
            var target = Selection.activeTransform;
            if (target != null)
            {
                RetriveTransform(target, true, (child) =>
                 {
                     Undo.RecordObject(target, "AutoCollider");
                     var colliders = child.GetComponents<Collider>();
                     for (int i = 0; i < colliders.Length; i++)
                     {
                         Object.DestroyImmediate(colliders[i]);
                     }
                 });
            }
        }

        [MenuItem("GameObject/UFrame/SceneEditTool/GenColliders", priority = 0, validate = false)]
        public static void GenCollider()
        {
            if (CheckWaitMenu())
                return;

            var parent = GetColliderRoot();
            var targets = Selection.transforms;
            if (targets != null)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    var target = targets[i];
                    CreateCollider(target, parent);
                }
            }
        }

        [MenuItem("GameObject/UFrame/SceneEditTool/GenCombineCollider", false)]
        public static void GenCombineColliderFromSelection()
        {
            if (CheckWaitMenu())
                return;

            CreateCombineCollider(Selection.objects);
        }
        protected static void RetriveTransform(Transform parent, bool deep, System.Action<Transform> onRetrive)
        {
            if (onRetrive != null)
            {
                onRetrive.Invoke(parent);
            }

            if (deep)
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    var child = parent.GetChild(i);
                    RetriveTransform(child, true, onRetrive);
                }
            }
        }

        [MenuItem("GameObject/UFrame/SceneEditTool/ClearEmptyScripts", false)]
        public static void ClearEmptyScrits()
        {
            int dex = 0;
            for (int i = 0; i < Selection.transforms.Length; i++)
            {
                Transform[] allChilds = Selection.transforms[i].GetComponentsInChildren<Transform>();
                for (int j = 0; j < allChilds.Length; j++)
                {
                    GameObject go = allChilds[j].gameObject; //寰楀埌鏁扮粍涓?殑鍗曚釜object
                    dex += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                }
            }
            //灏嗗満鏅??缃?负dert
            if (dex > 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }
            Debug.Log("clear scripts len:" + dex);
        }

        [MenuItem("GameObject/UFrame/SceneEditTool/MakeParent")]
        public static void MakeParent()
        {
            if (CheckWaitMenu() || Selection.transforms.Length <= 0)
                return;

            var childs = Selection.transforms;
            var centerPos = Vector3.zero;
            var parentName = "";
            for (int i = 0; i < childs.Length; i++)
            {
                centerPos += childs[i].transform.position;
                if (i == 0)
                    parentName = childs[i].name;
                else
                    parentName += "-" + childs[i].name;
            }
            centerPos /= childs.Length;
            var oldParent = childs[0].parent;
            var parent = new GameObject(parentName).GetComponent<Transform>();
            parent.SetParent(oldParent, true);
            parent.transform.position = centerPos;
            for (int i = 0; i < childs.Length; i++)
            {
                childs[i].transform.SetParent(parent, true);
            }
        }

        public static Vector3[] GetCiclePoints(float range, int pointCount)
        {
            var points = new Vector3[pointCount];
            if (pointCount <= 0)
                return points;
            var angle = 360f / pointCount;
            var originDir = Vector3.forward * range;
            for (int i = 0; i < pointCount; i++)
            {
                points[i] = Quaternion.AngleAxis(angle * i, Vector3.up) * originDir;
            }
            return points;
        }

        //鍒涘缓涓缁勭幆褰㈠?锟?
        public static void CylinderGroup(Transform parent, GameObject prefab, float redio, int count, int yCount, float yOffset)
        {
            float halfY = Mathf.FloorToInt(yCount * 0.5f);
            for (int i = 0; i < yCount; i++)
            {
                var yPos = (i - halfY) * yOffset;
                var points = GetCiclePoints(redio, count);
                var center = new Vector3(0, yPos, 0);
                for (int j = 0; j < points.Length; j++)
                {
                    var point = points[j];
                    point.y = yPos;
                    var forward = point - center;
                    var child = GameObject.Instantiate(prefab);
                    child.name = string.Format("x{0}y{1}", j + 1, i + 1);
                    child.transform.SetParent(parent.transform);
                    child.transform.localPosition = point;
                    child.transform.forward = forward;
                }
            }
        }

        //鍒涘缓涓涓?洓鏂逛綋锟?
        public static void CreateTetragonalBody(Transform parent, GameObject prefab, int widthCount, float offset)
        {
            var minPos = (1 - widthCount) * offset * 0.5f;
            for (int i = 0; i < widthCount; i++)
            {
                float x = minPos + i * offset;
                for (int j = 0; j < widthCount; j++)
                {
                    float y = minPos + j * offset;
                    for (int k = 0; k < widthCount; k++)
                    {
                        float z = minPos + k * offset;

                        var child = GameObject.Instantiate(prefab);
                        child.name = string.Format("x{0}y{1}z{2}", j + 1, i + 1, k + 1);
                        child.transform.SetParent(parent.transform);
                        child.transform.localPosition = new Vector3(x, y, z);
                    }
                }
            }
        }


        [MenuItem("GameObject/UFrame/SceneEditTool/Clone-Materials", priority = 5000, validate = false)]
        public static void CloneMaterials()
        {
            if (CheckWaitMenu())
                return;

            var folder = System.IO.Path.GetFullPath(TargetMatFolder);
            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }
            Dictionary<string, Material> worpMaterialDic = new Dictionary<string, Material>();
            var files = System.IO.Directory.GetFiles(folder, "*.mat");
            for (int i = 0; i < files.Length; i++)
            {
                var matFilePath = files[i].Replace("\\", "/").Replace(Application.dataPath, "Assets");
                var matFile = AssetDatabase.LoadAssetAtPath<Material>(matFilePath);
                if (matFile != null)
                {
                    worpMaterialDic[matFile.name] = matFile;
                    Debug.Log("load a material from path:" + matFilePath);
                }
            }

            for (int i = 0; i < Selection.objects.Length; i++)
            {
                var obj = Selection.objects[i] as GameObject;
                if (obj)
                {
                    var meshRenders = obj.GetComponentsInChildren<MeshRenderer>();
                    if (meshRenders != null && meshRenders.Length > 0)
                    {
                        foreach (var meshRender in meshRenders)
                        {
                            if (meshRender.sharedMaterials.Length > 0)
                            {
                                for (int j = 0; j < meshRender.sharedMaterials.Length; j++)
                                {
                                    var oldMaterial = meshRender.sharedMaterials[j];
                                    if (worpMaterialDic.TryGetValue(oldMaterial.name, out Material newMat))
                                    {
                                        Debug.Log("ignore material:" + newMat.name);
                                    }
                                    else
                                    {
                                        var oldPath = AssetDatabase.GetAssetPath(oldMaterial);
                                        var newPath = TargetMatFolder + "/" + oldMaterial.name + ".mat";
                                        if (oldPath != newPath)
                                        {
                                            Debug.LogWarning("copy mat to:" + newPath);
                                            AssetDatabase.CopyAsset(oldPath, newPath);
                                            AssetDatabase.Refresh();
                                            newMat = AssetDatabase.LoadAssetAtPath<Material>(newPath);
                                            worpMaterialDic[newMat.name] = newMat;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }

        [MenuItem("GameObject/SceneEditor/Load-Clone-Materials", priority = 5000, validate = false)]
        public static void LoadCloneMaterials()
        {
            if (CheckWaitMenu())
                return;
            var folder = System.IO.Path.GetFullPath(TargetMatFolder);
            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }
            Dictionary<string, Material> worpMaterialDic = new Dictionary<string, Material>();
            var files = System.IO.Directory.GetFiles(folder, "*.mat");
            for (int i = 0; i < files.Length; i++)
            {
                var matFilePath = files[i].Replace("\\", "/").Replace(Application.dataPath, "Assets");
                var matFile = AssetDatabase.LoadAssetAtPath<Material>(matFilePath);
                if (matFile != null)
                {
                    worpMaterialDic[matFile.name] = matFile;
                    Debug.Log("load a material from:" + matFilePath);
                }
            }

            for (int i = 0; i < Selection.objects.Length; i++)
            {
                var obj = Selection.objects[i] as GameObject;
                if (obj)
                {
                    var meshRenders = obj.GetComponentsInChildren<MeshRenderer>();
                    if (meshRenders != null && meshRenders.Length > 0)
                    {
                        foreach (var meshRender in meshRenders)
                        {
                            if (meshRender.sharedMaterials.Length > 0)
                            {
                                var newMaterials = new Material[meshRender.sharedMaterials.Length];
                                for (int j = 0; j < newMaterials.Length; j++)
                                {
                                    var oldMaterial = meshRender.sharedMaterials[j];
                                    if (worpMaterialDic.TryGetValue(oldMaterial.name, out Material newMat) && oldMaterial != newMat)
                                    {
                                        newMaterials[j] = newMat;
                                        Debug.Log("replace material:" + oldMaterial.name);
                                    }
                                    else
                                    {
                                        newMaterials[j] = oldMaterial;
                                    }
                                }
                                Debug.Log("change render materials:" + meshRender.name, meshRender);
                                meshRender.sharedMaterials = newMaterials;
                                UnityEditor.Undo.RecordObject(meshRender, "change_mat");
                            }
                        }
                    }
                }
            }

        }
    }
}