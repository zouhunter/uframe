//*************************************************************************************
//* 作    者： 邹杭特
//* 创建时间： 2021-11-08 10:59:23
//* 描    述：  

//* ************************************************************************************
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.AssetBundles
{
#if UNITY_EDITOR
    public class SimulationLoader : ICustomLoader
    {
        protected Queue<System.Tuple<string, string, System.Action<Object>>> m_asyncLoadQueue = new Queue<System.Tuple<string, string, System.Action<Object>>>();// Tuple<string, string, UnityAction<Object>>();
        protected Queue<System.Tuple<AsyncOperation, System.Action<AsyncOperation>>> m_sceneProgressQueue = new Queue<System.Tuple<AsyncOperation, System.Action<AsyncOperation>>>();
        public bool Actived { get { return true; } }
        private System.Action<bool> onInit { get; set; }
        public T LoadAsset<T>(string bundleName, string assetName) where T : UnityEngine.Object
        {
            bundleName = bundleName.ToLower();
            Debug.LogFormat("SimulationLoader.LoadAsset<T>({0},{1})", bundleName, assetName);
            string[] assetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName, assetName);
            if (assetPaths.Length == 0)
            {
                return null;
            }
            // @TODO: Now we only get the main object from the first asset. Should consider type also.
            T target = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPaths[0]);
            return target;
        }

        public T[] LoadAssets<T>(string bundleName, params string[] assetNames) where T : UnityEngine.Object
        {
            bundleName = bundleName.ToLower();
            Debug.LogFormat("SimulationLoader.LoadAssets<T>({0},assetNames)", bundleName);
            T[] objectPool = new T[assetNames.Length];
            for (int i = 0; i < assetNames.Length; i++)
            {
                objectPool[i] = LoadAsset<T>(bundleName, assetNames[i]);
            }
            return objectPool;
        }

        public T[] LoadAssets<T>(string bundleName) where T : UnityEngine.Object
        {
            bundleName = bundleName.ToLower();
            Debug.Log("SimulationLoader.LoadAssets<T>");
            string[] assetPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
            if (assetPaths.Length == 0)
            {
                Debug.LogError("There is no asset with type \"" + typeof(T).ToString() + "\" in " + bundleName);
            }
            List<T> items = new List<T>();
            for (int i = 0; i < assetPaths.Length; i++)
            {
                T item = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPaths[i]);
                if (item != null)
                {
                    items.Add(item);
                }
            }
            return items.ToArray();
        }

        public void LoadSceneAsync(string bundleName, string sceneName, bool isAddictive, System.Action<AsyncOperation> onProgressChanged)
        {
            bundleName = bundleName.ToLower();
            Debug.LogFormat("SimulationLoader.LoadSceneAsync<T>({0},{1})", bundleName, sceneName);
            string[] levelPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName, sceneName);
            AsyncOperation m_Operation;
            UnityEngine.SceneManagement.LoadSceneParameters loadParams = new UnityEngine.SceneManagement.LoadSceneParameters();
            if (isAddictive)
                loadParams.loadSceneMode = UnityEngine.SceneManagement.LoadSceneMode.Additive;
            else
                loadParams.loadSceneMode = UnityEngine.SceneManagement.LoadSceneMode.Single;
            m_Operation = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(levelPaths[0], loadParams);
            m_Operation.allowSceneActivation = false;
            m_sceneProgressQueue.Enqueue(new System.Tuple<AsyncOperation, System.Action<AsyncOperation>>(m_Operation, onProgressChanged));
        }

        public AsyncOperation LoadLevel(string bundleName, string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadModle)
        {
            bundleName = bundleName.ToLower();
            Debug.Log("SimulationLoader.LoadLevel");
            string[] levelPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName, sceneName);
            AsyncOperation operation = null;
            if (levelPaths.Length == 0)
                return operation;
            UnityEngine.SceneManagement.LoadSceneParameters loadParams = new UnityEngine.SceneManagement.LoadSceneParameters();
            loadParams.loadSceneMode = loadModle;
            operation = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(levelPaths[0], loadParams);
            return operation;
        }

        private void UpdateWaitLoadLevel()
        {
            if (m_sceneProgressQueue.Count > 0)
            {
                var tuple = m_sceneProgressQueue.Peek();
                var operation = tuple.Item1;

                if (operation != null)
                {
                    var onProgressChanged = tuple.Item2;
                    if (onProgressChanged != null)
                        onProgressChanged(operation);
                }

                if (operation.isDone)
                {
                    m_sceneProgressQueue.Dequeue();
                }
            }
        }

        public void LoadAssetAsync<T>(string bundleName, string assetName, System.Action<T> onAssetLoad, System.Action<float> onProgress) where T : UnityEngine.Object
        {
            bundleName = bundleName.ToLower();
            Debug.LogFormat("SimulationLoader.LoadAssetAsync({0},{1})", bundleName, assetName);
            var tupe = new System.Tuple<string, string, System.Action<Object>>(bundleName, assetName, new System.Action<Object>((obj) =>
            {
                if (onAssetLoad != null)
                {
                    if (obj != null)
                    {
                        onAssetLoad.Invoke((T)obj);
                    }
                    else
                    {
                        onAssetLoad.Invoke(null);
                    }
                }
                if (onProgress != null)
                {
                    onProgress.Invoke(1);
                }
            }));
            m_asyncLoadQueue.Enqueue(tupe);
        }

        protected void UpdateWaitLoadObject()
        {
            if (m_asyncLoadQueue.Count > 0)
            {
                var tupe = m_asyncLoadQueue.Dequeue();
                tupe.Item3.Invoke(LoadAsset<Object>(tupe.Item1, tupe.Item2));
            }
        }

        public void UpdateDownLand()
        {
            UpdateWaitLoadLevel();
            UpdateWaitLoadObject();
        }

        public void Dispose()
        {
            m_sceneProgressQueue.Clear();
            m_asyncLoadQueue.Clear();
        }

        public void SetInitCallBack(System.Action<bool> callback)
        {
            onInit = callback;
        }

        public void LoadBundleAsync(string bundleName, System.Action<AssetBundle, string> onAssetLoad, System.Action<float> onProgress = null, bool autoUnload = true)
        {
            bundleName = bundleName.ToLower();
            string[] levelPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
            Debug.LogError("failed load bundle from simulation loader!" + bundleName);
            if(levelPaths != null && levelPaths.Length > 0)
            {
                foreach (var item in levelPaths)
                {
                    Debug.Log(".sub assets:" + item);
                }
            }
        }

        public void UnloadAssetBundle(string bundleName, bool clearInstance = true)
        {
            Debug.LogError("failed unload bundle from simulation loader!"+ bundleName);
        }
    }
#endif
}