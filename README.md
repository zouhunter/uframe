# uframe (https://gitee.com/zouhunter/uframe)
extendable unity game framework,easy and powerfall


## - LitUI (https://github.com/zouhunter/uframe.git?path=Assets/Framework/LitUI)
        event Action<UIInfo> onUIOpenEvent;
        event Action<UIInfo> onUICloseEvent;
        void SetUIRoot(Transform root);
        void RegistUIInfos(params UIInfo[] infos);
        UIAsyncOperation Open(UIInfo info, object arg = null, Transform parent = null);
        UIAsyncOperation Open(string name, object arg = null, Transform parent = null);
        bool Close(string name);
        void GetActiveViews(List<string> names, bool includeStack = false);
        UIView FindView(string name);
        UIView[] FindViews(byte layer);
        bool Hide(string name);
        bool UnHide(string name);
        void HideALL(byte layer = byte.MaxValue);
        void UnHideALL(byte layer = byte.MaxValue);
        bool Stack(string name);
        bool UnStack(string name);
        void StackALL(byte layer = byte.MaxValue);
        void UnStackAll(byte layer = byte.MaxValue);
        void CleanStacks(bool hideOnly = true, byte layer = byte.MaxValue);
        
        support custom uiloader

## - DressAB (https://github.com/zouhunter/uframe.git?path=Assets/Framework/DressAB)
        AsyncPreloadOperation StartPreload(ushort flags);
        AsyncPreloadOperation StartPreload(params string[] address);
        bool ExistAddress(string address);
        bool TryGetAddressGroup(string address, out string addressGroup, out string assetName);
        AsyncBundleOperation LoadAssetBundleAsync(string address, ushort flags);
        AsyncAssetOperation<T> LoadAssetAsync<T>(string address, ushort flags = 0) where T : UnityEngine.Object;
        AsyncAssetOperation<T> LoadAssetAsync<T>(string address, string assetname, ushort flags = 0) where T : UnityEngine.Object;
        AsyncAssetsOperation<T> LoadAssetsAsync<T>(string address, ushort flags = 0) where T : UnityEngine.Object;
        AsyncSceneOperation LoadSceneAsync(string address, string sceneName = null, ushort flags = 0, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode = UnityEngine.SceneManagement.LoadSceneMode.Single);
        void PreloadAssetBundle(BundleItem bundleItem, System.Action<string, object> onLoadBundle, HashSet<BundleItem> deepLoading);

## - Manage (https://github.com/zouhunter/uframe.git?path=Assets/Framework/Manage)
        BaseGameManage<T>
        AgentContext<AgentContainer> : Agent where AgentContainer : AgentContext<AgentContainer>, new()
        Singleton<Agent> : UFrame.Agent where Agent : Singleton<Agent>, new()s
