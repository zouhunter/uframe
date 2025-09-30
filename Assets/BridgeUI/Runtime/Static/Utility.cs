/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 工具类                                                                          *
*//************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEngine.UI;

namespace UFrame.BridgeUI
{
    public static class Utility
    {
        public readonly static List<Assembly> assembles;
        public readonly static Assembly GameAssembly;
        static Utility()
        {
            var currAssemble = typeof(Utility).Assembly;
            assembles = new List<Assembly>();
            assembles.Add(currAssemble);
            var allAssemble = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in allAssemble)
            {
                if (!assembles.Contains(assembly))
                    assembles.Add(assembly);
                else if (assembly.FullName.StartsWith("Assembly-CSharp,"))
                    GameAssembly = assembly;
            }
        }

        public static void SetTranform(Transform item, UILayerType layer, int layerIndex, Transform parent, Dictionary<int, Transform> rootDic, Dictionary<int, IUIPanel> transRefDic)
        {
            if (parent == null) return;

            var layerID = (int)layer;
            Transform root = null;
            if (!rootDic.TryGetValue(layerID, out root) || root == null)
            {
                string rootName = string.Format("{0}|{1}", layerID, layer);
                root = parent.transform.Find(rootName);

                if (root == null)
                {
                    if (parent is RectTransform)
                    {
                        var rectParent = new GameObject(rootName, typeof(RectTransform)).GetComponent<RectTransform>();
                        root = rectParent;
                        rectParent.SetParent(parent, false);
                        rectParent.anchorMin = Vector2.zero;
                        rectParent.anchorMax = Vector2.one;
                        rectParent.offsetMin = Vector3.zero;
                        rectParent.offsetMax = Vector3.zero;
                        rectParent.anchoredPosition = Vector2.zero;
                    }
                    else
                    {
                        root = new GameObject(rootName).transform;
                        root.SetParent(parent, true);
                    }

                    root.gameObject.layer = LayerMask.NameToLayer("UI");

                    if (rootName.StartsWith("-1"))
                    {
                        root.SetAsLastSibling();
                    }
                    else
                    {
                        int i = 0;
                        for (; i < parent.childCount; i++)
                        {
                            var ritem = parent.GetChild(i);
                            if (ritem.name.StartsWith("-1"))
                            {
                                break;
                            }
                            if (string.Compare(rootName, ritem.name) < 0)
                            {
                                break;
                            }
                        }
                        root.SetSiblingIndex(i);
                    }
                }

                rootDic[layerID] = root;
            }

            item.transform.SetParent(root, !(item.GetComponent<Transform>() is RectTransform));

            int id = 0;

            if (transRefDic != null)
            {
                var childCount = root.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    var obj = root.GetChild(i);
                    IUIPanel panel;
                    if (!transRefDic.TryGetValue(obj.GetInstanceID(), out panel) || obj == item || panel.UType.layerIndex <= layerIndex)
                    {
                        id = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            item.SetSiblingIndex(id);
        }

        public static List<Type> InitSupportTypes()
        {
            var supportedTypes = new List<System.Type>()
            {
                typeof(BindingReference),
                typeof(LayoutElement),
                typeof(GridLayoutGroup),
                typeof(VerticalLayoutGroup),
                typeof(HorizontalLayoutGroup),
                typeof(BridgeUIControl),

                typeof(Slider),
                typeof(Toggle),
                typeof(Button),
                typeof(Mask),
                typeof(Dropdown),
                typeof(InputField),

                typeof(RawImage),
                typeof(Image),
                typeof(Text),

                typeof(RectTransform),
                typeof(Transform),
                typeof(GameObject),

                //typeof(ColorBlock),
                //typeof(SpriteState),

                typeof(Color),
                typeof(Vector2),
                typeof(Vector3),
                typeof(Vector4),
                typeof(Vector2Int),
                typeof(Vector3Int),

                typeof(char),
                typeof(byte),
                typeof(ushort),
                typeof(int),
                typeof(float),
                typeof(string),
                typeof(bool),

                typeof(Sprite),
                typeof(Texture),
                typeof(Mesh),
                typeof(Shader),
                typeof(TextAsset),
            };
            return supportedTypes;
        }

        public static GameObjectPool CreatePool(Transform parent, out GameObject target)
        {
            target = new GameObject("ItemPool");
            var poolTrans = target.GetComponent<Transform>();
            poolTrans.SetParent(parent);
            var objPool = new GameObjectPool(poolTrans);
            return objPool;
        }

        public static Type FindTypeInAllAssemble(string typename)
        {
            for (int i = 0; i < assembles.Count; i++)
            {
                Type type = assembles[i].GetType(typename);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }

        public static List<Type> GetAllTypes()
        {
            List<Type> allTypes = new List<Type>();
            for (int i = 0; i < assembles.Count; i++)
            {
                var types = assembles[i].GetTypes();
                allTypes.AddRange(types);
            }
            return allTypes;
        }

        public static string ChangeType(object value)
        {
            if (value == null)
                return null;

            if (value is System.IConvertible)
            {
                return value.ToString();
            }
            else if(value is Color)
            {
                var color = (Color)value;
                return MergeString(color.r, color.g, color.b, color.a);
            }
            else if (value is Vector2)
            {
                var vector = (Vector2)value;
                return MergeString(vector.x, vector.y);
            }
            else if (value is Vector3)
            {
                var vector = (Vector3)value;
                return MergeString(vector.x, vector.y,vector.z);
            }
            else if (value is Vector4)
            {
                var vector = (Vector4)value;
                return MergeString(vector.x, vector.y, vector.z,vector.w);
            }
            else if (value is Vector2Int)
            {
                var vector = (Vector2Int)value;
                return MergeString(vector.x, vector.y);
            }
            else if (value is Vector3Int)
            {
                var vector = (Vector3Int)value;
                return MergeString(vector.x, vector.y,vector.z);
            }
            else if (value is Rect)
            {
                var rect = (Rect)value;
                return MergeString(rect.x, rect.y, rect.width,rect.height);
            }
            return null;
        }

        public static T ChangeType<T>(string value)
        {
            if (string.IsNullOrEmpty(value))
                return default(T);

            var type = typeof(T);
            if (typeof(System.IConvertible).IsAssignableFrom(typeof(T)))
            {
                return (T)System.Convert.ChangeType(value, typeof(T));
            }
            else if (type == typeof(Color))
            {
                var array = ToFloatArray(value);
                Color color = new Color();
                if (array != null && array.Length > 0) color.r = array[0];
                if (array != null && array.Length > 1) color.g = array[1];
                if (array != null && array.Length > 2) color.b = array[2];
                if (array != null && array.Length > 3) color.a = array[3];
                return (T)(object)color;
            }
            else if (type == typeof(Vector2))
            {
                var array = ToFloatArray(value);
                Vector2 vector = new Vector2();
                if (array != null && array.Length > 0) vector[0] = array[0];
                if (array != null && array.Length > 1) vector[1] = array[1];
                return (T)(object)vector;
            }
            else if (type == typeof(Vector3))
            {
                var array = ToFloatArray(value);
                Vector3 vector = new Vector3();
                if (array != null && array.Length > 0) vector[0] = array[0];
                if (array != null && array.Length > 1) vector[1] = array[1];
                if (array != null && array.Length > 2) vector[2] = array[2];
                return (T)(object)vector;
            }
            else if (type == typeof(Vector4))
            {
                var array = ToFloatArray(value);
                Vector4 vector = new Vector4();
                if (array != null && array.Length > 0) vector[0] = array[0];
                if (array != null && array.Length > 1) vector[1] = array[1];
                if (array != null && array.Length > 2) vector[2] = array[2];
                if (array != null && array.Length > 3) vector[3] = array[3];
                return (T)(object)vector;
            }
            else if (type == typeof(Vector2Int))
            {
                var array = ToIntArray(value);
                Vector2Int vector = new Vector2Int();
                if (array != null && array.Length > 0) vector[0] = array[0];
                if (array != null && array.Length > 1) vector[1] = array[1];
                return (T)(object)vector;
            }
            else if (type == typeof(Vector3Int))
            {
                var array = ToIntArray(value);
                Vector3Int vector = new Vector3Int();
                if (array != null && array.Length > 0) vector[0] = array[0];
                if (array != null && array.Length > 1) vector[1] = array[1];
                if (array != null && array.Length > 2) vector[2] = array[2];
                return (T)(object)vector;
            }
            else if (type == typeof(Rect))
            {
                var array = ToFloatArray(value);
                Rect rect = new Rect();
                if (array != null && array.Length > 0) rect.x = array[0];
                if (array != null && array.Length > 1) rect.y = array[1];
                if (array != null && array.Length > 2) rect.width = array[2];
                if (array != null && array.Length > 3) rect.height = array[3];
                return (T)(object)rect;
            }
            return default(T);
        }

        public static int[] ToIntArray(string value)
        {
            var array = value?.Split('$');
            if (array == null)
                return null;

            var valueArray = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                int.TryParse(array[i], out var subValue);
                valueArray[i] = subValue;
            }
            return valueArray;
        }

        public static float[] ToFloatArray(string value)
        {
            var array = value?.Split('$');
            if (array == null)
                return null;

            var valueArray = new float[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                float.TryParse(array[i], out var subValue);
                valueArray[i] = subValue;
            }
            return valueArray;
        }

        public static string MergeString(params int[] array)
        {
           return string.Join("$", array);
        }
        public static string MergeString(params float[] array)
        {
            return string.Join("$", array);
        }
    }
}