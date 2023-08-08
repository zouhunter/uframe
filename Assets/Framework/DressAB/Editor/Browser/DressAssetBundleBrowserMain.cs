using UFrame.DressAssetBundle.Browser.AssetBundleDataSource;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute("Jagat.DressAssetBundle.Browsser")]

namespace UFrame.DressAssetBundle.Browser
{
    public class DressAssetBundleBrowserMain : EditorWindow, IHasCustomMenu, ISerializationCallbackReceiver
    {
        private static DressAssetBundleBrowserMain s_instance = null;
        internal static DressAssetBundleBrowserMain instance
        {
            get
            {
                if (s_instance == null)
                    s_instance = GetWindow<DressAssetBundleBrowserMain>();
                return s_instance;
            }
        }

        internal const float kButtonWidth = 150;

        enum Mode
        {
            Browser,
            Builder,
            Inspect,
        }
        [SerializeField]
        Mode m_Mode;

        [SerializeField]
        int m_DataSourceIndex;

        [SerializeField]
        internal DressAssetBundleManageTab m_ManageTab;

        [SerializeField]
        internal AssetBundleBuildTab m_BuildTab;

        [SerializeField]
        internal AssetBundleInspectTab m_InspectTab;

        private Texture2D m_RefreshTexture;

        const float k_ToolbarPadding = 15;
        const float k_MenubarPadding = 32;

        [MenuItem("Window/DressBundle Browser", priority = 2050)]
        static void ShowWindow()
        {
            s_instance = null;
            m_defineObject = AddressDefineObjectSetting.Instance.activeAddressDefineObject;
            instance.titleContent = new GUIContent("DressBundles");
            instance.Show();
        }

        [UnityEditor.Callbacks.OnOpenAsset()]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var defineObj = EditorUtility.InstanceIDToObject(instanceID) as AddressDefineObject;
            if (defineObj != null)
            {
                s_instance = null;
                m_defineObject = defineObj;
                instance.titleContent = new GUIContent("DressBundles");
                instance.Show();
                return true;
            }
            return false;
        }

        [SerializeField]
        internal bool multiDataSource = false;
        List<AssetBundleDataSource.ABDataSource> m_DataSourceList = null;
        private static AddressDefineObject m_defineObject;

        public virtual void AddItemsToMenu(GenericMenu menu)
        {
            if (menu != null)
                menu.AddItem(new GUIContent("Custom Sources"), multiDataSource, FlipDataSource);
        }
        internal void FlipDataSource()
        {
            multiDataSource = !multiDataSource;
        }

        private void OnEnable()
        {
            InitDataSources();

            if (m_defineObject)
            {
                m_DataSourceIndex = 0;
                for (int i = 0; i < m_DataSourceList.Count; i++)
                {
                    var source = m_DataSourceList[i];
                    if (source is DressAssetBundleDataSource && (source as DressAssetBundleDataSource).addressDefine == m_defineObject)
                    {
                        m_DataSourceIndex = i;
                        Models.Model.DataSource = source;
                        break;
                    }
                }
            }

            Rect subPos = GetSubWindowArea();
            if (m_ManageTab == null)
                m_ManageTab = new DressAssetBundleManageTab();
            m_ManageTab.OnEnable(subPos, this);
            if (m_BuildTab == null)
                m_BuildTab = new AssetBundleBuildTab();
            m_BuildTab.OnEnable(this);
            if (m_InspectTab == null)
                m_InspectTab = new AssetBundleInspectTab();
            m_InspectTab.OnEnable(subPos);

            m_RefreshTexture = EditorGUIUtility.FindTexture("Refresh");
        }
        private void InitDataSources()
        {
            //determine if we are "multi source" or not...
            multiDataSource = false;
            m_DataSourceList = new List<AssetBundleDataSource.ABDataSource>();
            foreach (var info in AssetBundleDataSource.ABDataSourceProviderUtility.CustomABDataSourceTypes)
            {
                var sources = info.GetMethod("CreateDataSources").Invoke(null, null) as List<AssetBundleDataSource.ABDataSource>;
                m_DataSourceList.AddRange(sources);
            }
            if (m_DataSourceList.Count > 1)
            {
                multiDataSource = true;
                if (m_DataSourceIndex >= m_DataSourceList.Count)
                    m_DataSourceIndex = 0;
                Models.Model.DataSource = m_DataSourceList[m_DataSourceIndex];
            }
        }
        private void OnDisable()
        {
            if (m_BuildTab != null)
                m_BuildTab.OnDisable();
            if (m_InspectTab != null)
                m_InspectTab.OnDisable();
        }

        public void OnBeforeSerialize()
        {
        }
        public void OnAfterDeserialize()
        {
        }

        private Rect GetSubWindowArea()
        {
            float padding = k_MenubarPadding;
            if (multiDataSource)
                padding += k_MenubarPadding * 0.5f;
            Rect subPos = new Rect(0, padding, position.width, position.height - padding);
            return subPos;
        }

        private void Update()
        {
            switch (m_Mode)
            {
                case Mode.Builder:
                    break;
                case Mode.Inspect:
                    break;
                case Mode.Browser:
                default:
                    m_ManageTab.Update();
                    break;
            }
        }

        private void OnGUI()
        {
            ModeToggle();

            switch (m_Mode)
            {
                case Mode.Builder:
                    m_BuildTab.OnGUI();
                    break;
                case Mode.Inspect:
                    m_InspectTab.OnGUI(GetSubWindowArea());
                    break;
                case Mode.Browser:
                default:
                    m_ManageTab.OnGUI(GetSubWindowArea());
                    break;
            }
        }

        void ModeToggle()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(k_ToolbarPadding);
            bool clicked = false;
            switch (m_Mode)
            {
                case Mode.Browser:
                    clicked = GUILayout.Button(m_RefreshTexture);
                    if (clicked)
                        m_ManageTab.ForceReloadData();
                    break;
                case Mode.Builder:
                    GUILayout.Space(m_RefreshTexture.width + k_ToolbarPadding);
                    break;
                case Mode.Inspect:
                    clicked = GUILayout.Button(m_RefreshTexture);
                    if (clicked)
                        m_InspectTab.RefreshBundles();
                    break;
            }

            float toolbarWidth = position.width - k_ToolbarPadding * 4 - m_RefreshTexture.width;
            //string[] labels = new string[2] { "Configure", "Build"};
            string[] labels = new string[] { "Configure", "Build", "Inspect" };
            m_Mode = (Mode)GUILayout.Toolbar((int)m_Mode, labels, "LargeButton", GUILayout.Width(toolbarWidth));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (multiDataSource)
            {
                //GUILayout.BeginArea(r);
                GUILayout.BeginHorizontal();

                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    if (GUILayout.Button("Bundle Data Source:", EditorStyles.label))
                    {
                        if(Models.Model.DataSource is DressAssetBundleDataSource)
                        {
                            var defineObj = (Models.Model.DataSource as DressAssetBundleDataSource).addressDefine;
                            EditorGUIUtility.PingObject(defineObj);
                        }
                    }
                    var c = new GUIContent(string.Format("{0} ({1})", Models.Model.DataSource.Name, Models.Model.DataSource.ProviderName), "Select Asset Bundle Set");
                    if (GUILayout.Button(c, EditorStyles.toolbarPopup))
                    {
                        GenericMenu menu = new GenericMenu();

                        for (int index = 0; index < m_DataSourceList.Count; index++)
                        {
                            var ds = m_DataSourceList[index];
                            if (ds == null)
                                continue;

                            if (index > 0)
                                menu.AddSeparator("");

                            var counter = index;
                            menu.AddItem(new GUIContent(string.Format("{0} ({1})", ds.Name, ds.ProviderName)), false,
                                () =>
                                {
                                    m_DataSourceIndex = counter;
                                    var thisDataSource = ds;
                                    Models.Model.DataSource = thisDataSource;
                                    m_ManageTab.ForceReloadData();
                                }
                            );

                        }

                        menu.ShowAsContext();
                    }

                    GUILayout.FlexibleSpace();
                    if (Models.Model.DataSource.IsReadOnly())
                    {
                        GUIStyle tbLabel = new GUIStyle(EditorStyles.toolbar);
                        tbLabel.alignment = TextAnchor.MiddleRight;

                        GUILayout.Label("Read Only", tbLabel);
                    }
                }

                GUILayout.EndHorizontal();
                //GUILayout.EndArea();
            }
        }


    }
}
