using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;
using System.Linq;

namespace UFrame.PathFind.Editors
{
    public class TwoDPathFindEditorWindow : EditorWindow
    {
        protected enum BrushType { Add, Remove, OverWrite }

        protected Camera m_refCamera;
        protected Camera m_viewCamera = null;
        protected Texture2D m_pathMapTexture = null;
        protected int m_texHeight = 0;
        protected int m_texWidth = 0;
        protected float gridPixelWidth = 0;
        protected float gridPixelHeight = 0;
        protected int visualFieldMask = 255;
        protected byte visualMask = 255;
        protected MapData m_lastMapData;
        protected float m_gridSize = 0.25f;
        protected MapConfig m_currMap;
        public const string ArrowLeft = "←";
        public const string ArrowRight = "→";
        public const string ArrowUp = "↑";
        public const string ArrowDown = "↓";
        protected const string prefer_last_map_guid = "TwoDPathFindEditorWindow.lastguid";
        protected GUIContent[] PantOptions = new GUIContent[] { new GUIContent("方型"), new GUIContent("圆形") };
        protected int pantIndex = 0;
        protected Vector2 m_lastPosition;
        protected string[] layerNames { get; set; }
        public MapLayer[] layers
        {
            get
            {
                return m_currMap.layers;
            }
        }

        [MenuItem("Window/UFrame/Map-Editor/TwoD", priority = 100, validate = false)]
        public static void OpenWindow()
        {
            GetWindow<TwoDPathFindEditorWindow>("2D寻路编辑器");
        }

        [UnityEditor.Callbacks.OnOpenAsset(0)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var mapConfig = EditorUtility.InstanceIDToObject(instanceID) as MapConfig;
            if (mapConfig)
            {
                var window = GetWindow<TwoDPathFindEditorWindow>();
                window.OpenMapConfig(mapConfig);
                return true;
            }
            return false;
        }

        protected void OpenMapConfig(MapConfig mapConfig = null)
        {
            if (mapConfig == null)
            {
                mapConfig = GetOrCreateAreaMap();
                UpdateLayerNames();
            }
            m_currMap = mapConfig;
            m_currMap.CheckValied();
            m_refCamera = Camera.main;
            if (m_refCamera != null)
            {
                ReCreateTexture();
            }
            RecordLastBytes();
            RecordCurrentMapGUID();
        }

        protected void ReCreateTexture()
        {
            // Init render texture
            m_texWidth = m_currMap.textureSize;
            m_texHeight = (int)((float)m_currMap.height * m_currMap.textureSize / m_currMap.width);

            gridPixelWidth = (float)m_texWidth / m_currMap.width;
            gridPixelHeight = (float)m_texHeight / m_currMap.height;

            m_pathMapTexture = new Texture2D(m_texWidth, m_texHeight);
            RepaintPathMapTexture();
            // Change the window's size
            minSize = new Vector2(m_texWidth, m_texHeight);
            maxSize = new Vector2(m_texWidth + 300, m_texHeight);
            wantsMouseMove = true;
            position = new Rect(position.x, position.y, maxSize.x, maxSize.y);
            UpdateViewCamera();
        }

        protected void UpdateLayerNames()
        {
            if (m_currMap && m_currMap.layers != null)
            {
                layerNames = m_currMap.layers.Select(x => x.name).ToArray();
            }
        }

        protected void OnGUI()
        {
            if (m_currMap == null)
                OpenMapConfig();
            if (m_viewCamera != null)
                DrawMap();
            GUILayout.BeginArea(new Rect(m_texWidth, 0, position.width - m_texWidth, position.height));
            DrawRightPanel();
            GUILayout.EndArea();
        }

        protected void DrawRightPanel()
        {
            using (var verticalScrop = new EditorGUILayout.VerticalScope())
            {
                DrawDefine();
                GUILayout.Space(10);
                DrawBrash();
                GUILayout.FlexibleSpace();
                DrawContrlButtons();
            }
        }

        protected void DrawDefine()
        {
            GUILayout.Label("[设定]", EditorStyles.boldLabel);
            GUI.Box(GUILayoutUtility.GetLastRect(), "");

            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    GUILayout.Label("Width:", GUILayout.Width(60));
                    var width = EditorGUILayout.IntField(m_currMap.width);
                    if (change.changed && width > 0)
                    {
                        ChangeMapSize(width, m_currMap.height);
                    }
                    if (GUILayout.Button("-", EditorStyles.miniButtonLeft, GUILayout.Width(30)))
                    {
                        ChangeMapSize(m_currMap.width - 1, m_currMap.height);
                    }
                    if (GUILayout.Button("+", EditorStyles.miniButtonRight, GUILayout.Width(30)))
                    {
                        ChangeMapSize(m_currMap.width + 1, m_currMap.height);
                    }
                }
            }
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    GUILayout.Label("Height:", GUILayout.Width(60));
                    var height = EditorGUILayout.IntField(m_currMap.height);
                    if (change.changed && height > 0)
                    {
                        ChangeMapSize(m_currMap.width, height);
                    }
                    if (GUILayout.Button("-", EditorStyles.miniButtonLeft, GUILayout.Width(30)))
                    {
                        ChangeMapSize(m_currMap.width, m_currMap.height - 1);
                    }
                    if (GUILayout.Button("+", EditorStyles.miniButtonRight, GUILayout.Width(30)))
                    {
                        ChangeMapSize(m_currMap.width, m_currMap.height + 1);
                    }
                }
            }
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    GUILayout.Label("Grid:", GUILayout.Width(60));
                    var gridSize = EditorGUILayout.FloatField(m_gridSize);
                    if (change.changed && gridSize > 0.1f)
                    {
                        m_gridSize = gridSize;
                        if (m_viewCamera)
                        {
                            m_viewCamera.orthographicSize = m_currMap.height * m_gridSize;
                        }
                    }
                }
            }

            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Camera:", GUILayout.Width(60));
                var newCamera = (Camera)EditorGUILayout.ObjectField(m_refCamera, typeof(Camera), true);
                if (newCamera != null && m_refCamera != newCamera)
                {
                    m_refCamera = newCamera;
                    ReCreateTexture();
                }
            }
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Map:", GUILayout.Width(60));
                using (var changeScope = new EditorGUI.ChangeCheckScope())
                {
                    m_currMap = EditorGUILayout.ObjectField(m_currMap, typeof(MapConfig), false) as MapConfig;
                    if (GUILayout.Button("new"))
                    {
                        m_currMap = CreateMapObject();
                        UpdateLayerNames();
                    }

                    if (changeScope.changed)
                    {
                        ReCreateTexture();
                        UpdateLayerNames();
                        RecordCurrentMapGUID();
                    }
                }

            }
        }

        protected virtual void ChangeMapSize(int newWidth, int newHeight)
        {
            RecordLastBytes();
            m_currMap.ChangeMapSize(newWidth, newHeight, false);
            ReCreateTexture();
        }

        protected void DrawBrash()
        {
            GUILayout.Label("[地刷]", EditorStyles.boldLabel);
            GUI.Box(GUILayoutUtility.GetLastRect(), "");

            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Radius");
                m_currMap.brushRadius = EditorGUILayout.IntField(m_currMap.brushRadius);
                pantIndex = EditorGUILayout.Popup(pantIndex, PantOptions);
            }

            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Texture Size:");
                var newTextureScale = EditorGUILayout.IntField(m_currMap.textureSize);
                if (m_currMap.textureSize != newTextureScale)
                {
                    if (newTextureScale < 100)
                    {
                        newTextureScale = 100;
                    }
                    m_currMap.textureSize = newTextureScale;
                    ReCreateTexture();
                }
            }
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(ArrowLeft))
                {
                    MoveMapOffset(1, 0);
                }
                if (GUILayout.Button(ArrowRight))
                {
                    MoveMapOffset(-1, 0);
                }
                if (GUILayout.Button(ArrowUp))
                {
                    MoveMapOffset(0, -1);
                }
                if (GUILayout.Button(ArrowDown))
                {
                    MoveMapOffset(0, 1);
                }
            }

            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                using (var changeScope = new EditorGUI.ChangeCheckScope())
                {
                    m_currMap.lastEditMapText = EditorGUILayout.ObjectField(m_currMap.lastEditMapText, typeof(TextAsset), false) as TextAsset;
                    if (changeScope.changed)
                    {
                        if (m_currMap.lastEditMapText != null)
                        {
                            var ok = ImportMapDataFrom(m_currMap.lastEditMapText.bytes);
                            if (!ok)
                            {
                                m_currMap.lastEditMapText = null;
                            }
                        }
                    }
                }
                if (GUILayout.Button("导入", EditorStyles.miniButtonRight, GUILayout.Width(60)))
                {
                    ImportMapData();
                }
                var exportName = m_currMap.lastEditMapText != null ? "记录" : "导出";
                if (GUILayout.Button(exportName, EditorStyles.miniButtonRight, GUILayout.Width(60)))
                {
                    ExportMapData();
                }
                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(10);
            DrawLayersChoise();
        }

        protected void MoveMapOffset(int xOffset, int yOffset)
        {
            RecordLastBytes();
            m_currMap.MoveOffset(xOffset, yOffset);
            ReCreateTexture();
        }

        protected void ExportMapData()
        {
            var bytes = m_currMap.data;
            if (bytes != null && bytes.Length > 0)
            {
                var saveFilePath = "";
                bool reImport = false;
                if (!m_currMap.lastEditMapText)
                {
                    saveFilePath = EditorUtility.SaveFilePanel("导出路径", Application.dataPath, "new_map_bytes", "txt");
                    if (saveFilePath.Replace("\\", "/").StartsWith(Application.dataPath))
                        reImport = true;
                }
                else
                {
                    saveFilePath = AssetDatabase.GetAssetPath(m_currMap.lastEditMapText);
                    saveFilePath = saveFilePath.Replace("Assets", Application.dataPath);
                }
                if (!string.IsNullOrEmpty(saveFilePath))
                    System.IO.File.WriteAllBytes(saveFilePath, bytes);
                AssetDatabase.Refresh();
                if (reImport)
                {
                    saveFilePath = saveFilePath.Replace(Application.dataPath, "Assets");
                    m_currMap.lastEditMapText = AssetDatabase.LoadAssetAtPath<TextAsset>(saveFilePath);
                }
            }
        }

        protected void ImportMapData()
        {
            var openFilePath = EditorUtility.OpenFilePanel("导入路径", Application.dataPath, "txt");
            if (!string.IsNullOrEmpty(openFilePath))
            {
                var bytes = System.IO.File.ReadAllBytes(openFilePath);
                bool ok = ImportMapDataFrom(bytes);
                if (ok && openFilePath.Replace("\\", "/").StartsWith(Application.dataPath))
                {
                    openFilePath = openFilePath.Replace(Application.dataPath, "Assets");
                    m_currMap.lastEditMapText = AssetDatabase.LoadAssetAtPath<TextAsset>(openFilePath);
                }
            }
        }

        protected bool ImportMapDataFrom(byte[] bytes)
        {
            if (m_currMap.height * m_currMap.width == bytes.Length)
            {
                m_currMap.ResetData(m_currMap.width, m_currMap.height, bytes);
                ReCreateTexture();
                RecordLastBytes();
                return true;
            }
            else
            {
                Debug.LogError("map date failed len:" + bytes.Length + ", but current map:" + m_currMap.width * m_currMap.height);
                return false;
            }
        }

        protected void DrawLayersChoise()
        {
            using (var hor = new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("[层级]", GUILayout.Width(60));
                if (layerNames == null)
                    UpdateLayerNames();
                var newLayerMask = EditorGUILayout.MaskField(visualFieldMask, layerNames);
                if (newLayerMask != visualFieldMask)
                {
                    visualFieldMask = newLayerMask;
                    visualMask = MaskFieldToMask(visualFieldMask);
                    RepaintPathMapTexture();
                }
            }
            GUI.Box(GUILayoutUtility.GetLastRect(), "");

            int maskValue = 1;
            for (int i = 0; i < layers.Length; i++)
            {
                var state = (visualFieldMask & maskValue) != 0;
                bool newstate = state;
                using (var hor = new EditorGUILayout.HorizontalScope())
                {
                    newstate = EditorGUILayout.ToggleLeft(layers[i].name, state, GUILayout.Width(60));
                    layers[i].color = EditorGUILayout.ColorField(layers[i].color);
                }
                if (newstate != state)
                {
                    if (newstate)
                    {
                        visualFieldMask |= maskValue;
                    }
                    else
                    {
                        visualFieldMask ^= maskValue;
                    }
                    visualMask = MaskFieldToMask(visualFieldMask);
                    RepaintPathMapTexture();
                }
                maskValue <<= 1;
            }
        }

        protected void DrawContrlButtons()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reload", EditorStyles.miniButtonLeft))
            {
                if (m_lastMapData != null && m_lastMapData.Valid())
                {
                    m_currMap.ResetData(m_lastMapData.width, m_lastMapData.height, m_lastMapData.data);
                    ReCreateTexture();
                }
            }
            if (GUILayout.Button("Save", EditorStyles.miniButton))
            {
                SaveCollisionMap();
                RecordLastBytes();
            }

            if (GUILayout.Button("Clear", EditorStyles.miniButtonRight))
            {
                if (m_lastMapData == null)
                {
                    RecordLastBytes();
                }
                for (int i = 0; i < m_currMap.data.Length; i++)
                {
                    m_currMap.data[i] = 0;
                }
                RepaintPathMapTexture();
            }
            GUILayout.EndHorizontal();
        }

        protected void RecordLastBytes()
        {
            m_lastMapData = new MapData();
            m_lastMapData.width = m_currMap.width;
            m_lastMapData.height = m_currMap.height;
            m_lastMapData.data = new byte[m_currMap.data.Length];
            m_currMap.data.CopyTo(m_lastMapData.data, 0);
        }

        protected void DrawMap()
        {
            if (Event.current.button == 0 && (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown))
            {
                Vector2 clickPos = Event.current.mousePosition;
                int gridX = (int)(clickPos.x / gridPixelWidth);
                int gridY = (int)((m_texHeight - clickPos.y) / gridPixelHeight);

                if (m_currMap.IsGridValid(gridX, gridY))
                {
                    BrushType brushType = BrushType.Add;
                    if (Event.current.shift)
                        brushType = BrushType.Remove;
                    else if (Event.current.control)
                        brushType = BrushType.OverWrite;
                    if (pantIndex == 0)
                        PaintBlockQuad(gridX, gridY, m_currMap.brushRadius, visualMask, brushType);
                    else if (pantIndex == 1)
                        PaintBlockCircle(gridX, gridY, m_currMap.brushRadius, visualMask, brushType);
                    m_pathMapTexture.Apply();
                    Repaint();
                }
                m_lastPosition = clickPos;
            }
            else if (Event.current.button == 1 && Event.current.type == EventType.MouseDown)
            {
                Vector2 clickPos = Event.current.mousePosition;

                int gridX = (int)(clickPos.x / gridPixelWidth);
                int gridY = (int)((m_texHeight - clickPos.y) / gridPixelHeight);

                int gridX1 = (int)(m_lastPosition.x / gridPixelWidth);
                int gridY1 = (int)((m_texHeight - m_lastPosition.y) / gridPixelHeight);

                int startX = gridX1 > gridX ? gridX : gridX1;
                int endX = gridX1 < gridX ? gridX : gridX1;

                int startY = gridY1 > gridY ? gridY : gridY1;
                int endY = gridY1 < gridY ? gridY : gridY1;

                if (m_currMap.IsGridValid(gridX, gridY))
                {
                    BrushType brushType = BrushType.Add;
                    if (Event.current.shift)
                        brushType = BrushType.Remove;
                    else if (Event.current.control)
                        brushType = BrushType.OverWrite;

                    PaintBlockVector(new Vector2Int(startX, startY), new Vector2Int(endX, endY), visualMask, brushType);
                    m_pathMapTexture.Apply();
                    Repaint();
                }
                m_lastPosition = clickPos;
            }

            m_viewCamera.Render();
            GUI.DrawTexture(new Rect(0f, 0f, m_texWidth, m_texHeight), m_viewCamera.targetTexture);
            GUI.DrawTexture(new Rect(0f, 0f, m_texWidth, m_texHeight), m_pathMapTexture);
        }

        private void UpdateViewCamera()
        {
            ClearCreatedCamera();
            // Init camera
            m_viewCamera = (Camera)Instantiate(m_refCamera);
            m_viewCamera.gameObject.SetActive(true);
            m_viewCamera.orthographic = true;
            m_viewCamera.CopyFrom(m_refCamera);
            m_viewCamera.targetTexture = new RenderTexture(m_texWidth, m_texHeight, 24);
            m_gridSize = 2 * m_viewCamera.orthographicSize / m_currMap.height;
        }

        protected Color GenLayersColor(byte layersValue)
        {
            Color resCol = Color.clear;
            for (int i = 0; i < layers.Length; i++)
            {
                var layer = layers[i];
                if ((layersValue & layer.value) != 0)
                    resCol += layers[i].color;
            }

            if (resCol.a > 1)
                resCol.a = 1;

            resCol.a *= 0.5f;
            return resCol;
        }

        protected byte MaskFieldToMask(int maskUIValue)
        {
            byte brushValue = 0;
            int curMask = 1;
            for (int i = 0; i < layers.Length; ++i)
            {
                if ((maskUIValue & curMask) != 0)
                    brushValue |= layers[i].value;

                curMask = curMask << 1;
            }

            return brushValue;
        }

        protected MapConfig GetOrCreateAreaMap()
        {
            string guid;
            string mapPath;
            MapConfig mapObj = null;
            if (EditorPrefs.HasKey(prefer_last_map_guid))
            {
                guid = EditorPrefs.GetString(prefer_last_map_guid);
                mapPath = AssetDatabase.GUIDToAssetPath(guid);
                mapObj = AssetDatabase.LoadAssetAtPath<MapConfig>(mapPath);
                if (mapObj != null)
                {
                    return mapObj;
                }
            }
            else
            {
                Debug.LogError(prefer_last_map_guid);
            }
            return CreateMapObject();
        }


        protected MapConfig CreateMapObject()
        {
            var mapObj = ScriptableObject.CreateInstance<MapConfig>();
            AssetDatabase.CreateAsset(mapObj, "Assets/new_map.asset");
            var mapPath = AssetDatabase.GetAssetPath(mapObj);
            AssetDatabase.Refresh();

            if (!string.IsNullOrEmpty(mapPath))
            {
                var guid = AssetDatabase.GUIDToAssetPath(mapPath);
                EditorPrefs.SetString(prefer_last_map_guid, guid);
            }
            else
            {
                Debug.LogError("can`t find asset path!");
            }
            return mapObj;
        }

        protected void RecordCurrentMapGUID()
        {
            if (m_currMap)
            {
                var mapPath = AssetDatabase.GetAssetPath(m_currMap);
                if (string.IsNullOrEmpty(mapPath))
                {
                    Debug.LogWarning("m_currMap path find failed!");
                    return;
                }
                var guid = AssetDatabase.AssetPathToGUID(mapPath);
                EditorPrefs.SetString(prefer_last_map_guid, guid);
            }
        }

        protected void SaveCollisionMap()
        {
            EditorUtility.SetDirty(m_currMap);
            AssetDatabase.Refresh();
        }

        void PaintGridColor(int x, int y, Color color)
        {
            int startXPixel = (int)(x * gridPixelWidth);
            int startYPixel = (int)(y * gridPixelHeight);

            int iGridWidth = Mathf.CeilToInt(gridPixelWidth);
            int iGridHeight = Mathf.CeilToInt(gridPixelHeight);

            Color[] colors = new Color[iGridWidth * iGridHeight];
            for (int i = 0; i < colors.Length; ++i)
                colors[i] = color;

            m_pathMapTexture.SetPixels(startXPixel, startYPixel, iGridWidth, iGridHeight, colors);
        }

        protected void PaintBlockCircle(int x, int y, int Radius, byte value, BrushType brushType)
        {
            var halfRadius = Radius * 0.5f;
            var startX = Mathf.FloorToInt(x - halfRadius);
            var startY = Mathf.FloorToInt(y - halfRadius);
            var endX = Mathf.CeilToInt(x + halfRadius);
            var endY = Mathf.CeilToInt(y + halfRadius);
            var center = new Vector2(x, y);
            for (; startX <= endX; ++startX)
            {
                for (int yIndex = startY; yIndex <= endY; ++yIndex)
                {
                    var distance = Vector2.Distance(new Vector2(startX, yIndex), center);
                    if (distance < halfRadius)
                    {
                        SetPointState(startX, yIndex, value, brushType);
                    }
                }
            }
        }

        void PaintBlockVector(Vector2Int startPos, Vector2Int endPos, byte value, BrushType brushType)
        {
            int startX = startPos.x;
            int startY = startPos.y;
            int endX = endPos.x;
            int endY = endPos.y;

            for (; startX <= endX; ++startX)
            {
                for (int yIndex = startY; yIndex <= endY; ++yIndex)
                {
                    SetPointState(startX, yIndex, value, brushType);
                }
            }
        }


        void PaintBlockQuad(int x, int y, int Radius, byte value, BrushType brushType)
        {
            int startX = x;
            startX = Mathf.Clamp(startX, 0, m_currMap.width - 1);
            int startY = y;
            startY = Mathf.Clamp(startY, 0, m_currMap.height - 1);
            int endX = startX + Radius - 1;
            endX = Mathf.Clamp(endX, 0, m_currMap.width - 1);
            int endY = startY + Radius - 1;
            endY = Mathf.Clamp(endY, 0, m_currMap.height - 1);

            for (; startX <= endX; ++startX)
            {
                for (int yIndex = startY; yIndex <= endY; ++yIndex)
                {
                    SetPointState(startX, yIndex, value, brushType);
                }
            }
        }

        protected void SetPointState(int x, int y, byte value, BrushType brushType)
        {
            if (brushType == BrushType.Add)
                m_currMap[x, y] = (byte)(m_currMap[x, y] | value);
            else if (brushType == BrushType.Remove)
                m_currMap[x, y] = (byte)(m_currMap[x, y] & (~value));
            else if (brushType == BrushType.OverWrite)
                m_currMap[x, y] = value;
            PaintGridColor(x, y, GenLayersColor((byte)(m_currMap[x, y] & visualMask)));
        }

        protected void RepaintPathMapTexture()
        {
            for (int i = 0; i < m_currMap.width; ++i)
            {
                for (int j = 0; j < m_currMap.height; ++j)
                {
                    Color gridColor = GenLayersColor((byte)(m_currMap[i, j] & visualMask));
                    PaintGridColor(i, j, gridColor);
                }
            }

            m_pathMapTexture.Apply();
            Repaint();
        }

        protected void OnDestroy()
        {
            ClearCreatedCamera();
            DestroyImmediate(m_pathMapTexture);
        }

        protected void ClearCreatedCamera()
        {
            foreach (Camera cam in FindObjectsOfType(typeof(Camera)))
            {
                if (cam.name.Contains("(Clone)"))
                    DestroyImmediate(cam.gameObject); // In editor mode use DestroyImmediate instead of Destroy
            }
        }
    }
}