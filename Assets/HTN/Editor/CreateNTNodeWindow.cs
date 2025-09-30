using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UFrame.BehaviourTree;
using UnityEditor.Experimental.GraphView;
using System;
using System.Reflection;
using System.Linq;
using System.Text;

namespace UFrame.HTN
{
    public class CreateNTNodeWindow : ScriptableObject, ISearchWindowProvider
    {
        private Texture2D icon;
        private Action<BaseNode> createNodeAction;
        private Type baseType;
        private List<BaseNode> customNodes;
        public class ScriptTemplate
        {
            public string templateFile;
            public string subFolder;
            public string defaultFileName;
        }

        public void Initialise(Action<BaseNode> onCreate, Type baseType, List<BaseNode> customNodes)
        {
            this.createNodeAction = onCreate;
            this.baseType = baseType;
            this.customNodes = customNodes;
            icon = new Texture2D(1, 1);
            icon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            icon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
            };

            CreateTargetTypes("Conditions", typeof(ConditionNode), tree, context);
            CreateTargetTypes("Actions", typeof(ActionNode), tree, context);
            CreateTargetTypes("Methods", typeof(MethodNode), tree, context);
            CreateFromTreeNodes(tree, context);
            CreateNewScriptSearch(tree, context);
            return tree;
        }

        private void CreateFromTreeNodes(List<SearchTreeEntry> tree, SearchWindowContext context)
        {
            if (customNodes != null)
            {
                customNodes.RemoveAll(x => !baseType.IsAssignableFrom(x.GetType()));
                var groups = customNodes.GroupBy(x => x.GetType().BaseType.Name).ToList();
                if (groups.Count > 0)
                {
                    tree.Add(new SearchTreeGroupEntry(new GUIContent("Internals")) { level = 1 });
                    foreach (var group in groups)
                    {
                        tree.Add(new SearchTreeGroupEntry(new GUIContent(group.Key)) { level = 2 });
                        foreach (var node in group)
                        {
                            tree.Add(new SearchTreeGroupEntry(new GUIContent(node.name)) { level = 3 });

                            var copyNode = node;
                            var action = new Action(() =>
                            {
                                createNodeAction?.Invoke(copyNode);
                            });
                            tree.Add(new SearchTreeEntry(new GUIContent($"Reference({node.name})")) { level = 4, userData = action });

                            var cloneNode = System.Activator.CreateInstance(node.GetType()) as BaseNode;
                            EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(node), cloneNode);
                            cloneNode.name = node.name + "_" + node.GetHashCode();
                            var action2 = new Action(() =>
                            {
                                createNodeAction?.Invoke(cloneNode);
                            });
                            tree.Add(new SearchTreeEntry(new GUIContent($"Clone({node.name})")) { level = 4, userData = action2 });
                        }
                    }
                }
            }
        }

        private void CreateTargetTypes(string title, Type bType, List<SearchTreeEntry> tree, SearchWindowContext context)
        {
            if (baseType == null || !baseType.IsAssignableFrom(bType))
                return;

            tree.Add(new SearchTreeGroupEntry(new GUIContent(title)) { level = 1 });
            var types = TypeCache.GetTypesDerivedFrom(bType).Where(x => !x.IsAbstract && !x.IsGenericType).ToList();
            types.Sort((x, y) => string.Compare(x.Namespace + GetTypeName(x), y.Namespace + GetTypeName(y)));
            var entrySet = new HashSet<string>();
            foreach (var type in types)
            {
                CreateEntryFromNamespace(2, type, entrySet, tree, context);
            }
        }

        private void CreateEntryFromNamespace(int startLevel, Type type, HashSet<string> entrySet, List<SearchTreeEntry> tree, SearchWindowContext context)
        {
            if (type.IsAbstract)
                return;

            var fullName = GetTypeName(type);
            var paths = fullName.Split(new char[] { '.', '/' });
            var nowPath = "";
            for (int i = 0; i < paths.Length; i++)
            {
                var pathItem = paths[i];
                nowPath += pathItem;

                if (entrySet.Contains(nowPath))
                {
                    continue;
                }
                entrySet.Add(nowPath);

                if (i == paths.Length - 1)
                {
                    CreateNodeArgEntry(tree, context, i + startLevel, pathItem, type);
                }
                else
                {
                    tree.Add(new SearchTreeGroupEntry(new GUIContent(pathItem)) { level = i + startLevel });
                }
            }
        }

        static List<List<string>> GenerateAllCombinations(List<Dictionary<string, KeyValuePair<MemberInfo, object>>> listOfDicts)
        {
            List<List<string>> result = new List<List<string>>();
            GenerateCombinations(listOfDicts, 0, new List<string>(), result);
            return result;
        }

        static void GenerateCombinations(List<Dictionary<string, KeyValuePair<MemberInfo, object>>> listOfDicts, int index, List<string> current, List<List<string>> result)
        {
            if (index == listOfDicts.Count)
            {
                result.Add(new List<string>(current));
                return;
            }

            foreach (var key in listOfDicts[index].Keys)
            {
                current.Add(key);
                GenerateCombinations(listOfDicts, index + 1, current, result);
                current.RemoveAt(current.Count - 1);
            }
        }

        private void CreateNodeArgEntry(List<SearchTreeEntry> tree, SearchWindowContext context, int level, string pathItem, Type type)
        {
            var members = type.GetMembers(BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            List<Dictionary<string, KeyValuePair<MemberInfo, object>>> membersInfoMap = new();
            foreach (var member in members)
            {
                Type fieldType = null;
                bool isPrivateOrProtected = false;
                if (member is FieldInfo fieldInfo)
                {
                    fieldType = fieldInfo.FieldType;
                    isPrivateOrProtected = fieldInfo.IsPrivate || fieldInfo.IsFamily;
                }
                else if (member is PropertyInfo propInfo)
                {
                    fieldType = propInfo.PropertyType;
                    if (propInfo.GetSetMethod() == null)
                        continue;
                }
                else
                {
                    continue;
                }

                object[] defaluts = null;
                var attr = member.GetCustomAttribute<PrimaryArgAttribute>();
                var constsAttr = member.GetCustomAttribute<PrimaryConstsAttribute>();
                if (attr != null)
                {
                    if (attr.defaluts == null || attr.defaluts.Length == 0)
                    {
                        if (fieldType == typeof(bool))
                        {
                            defaluts = new object[2] { true, false };
                        }
                    }
                    else if (attr.defaluts != null)
                    {
                        defaluts = attr.defaluts;
                    }
                }
                else if (constsAttr != null)
                {
                    var fields = constsAttr.type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.GetField);
                    var mebArr = fields.Select(x => x.GetValue(member)).ToArray();
                    if (mebArr.Length > 0)
                        defaluts = mebArr;
                }

                if (defaluts == null && fieldType.IsEnum && !isPrivateOrProtected)
                {
                    var enumValues = fieldType.GetEnumValues();
                    defaluts = new object[enumValues.Length];
                    for (int i = 0; i < enumValues.Length; i++)
                    {
                        defaluts[i] = enumValues.GetValue(i);
                    }
                }

                if (defaluts == null && fieldType == typeof(bool) && !isPrivateOrProtected)
                {
                    defaluts = new object[2] { true, false };
                }

                if (defaluts != null)
                {
                    Dictionary<string, KeyValuePair<MemberInfo, object>> memberInfo = new();
                    foreach (var item in defaluts)
                    {
                        memberInfo[$"{member.Name}={item}"] = new KeyValuePair<MemberInfo, object>(member, item);
                    }
                    membersInfoMap.Add(memberInfo);
                }
            }
            if (membersInfoMap.Count > 0)
            {
                Dictionary<string, Dictionary<MemberInfo, object>> preloadArgDic = new Dictionary<string, Dictionary<MemberInfo, object>>();
                var combinations = GenerateAllCombinations(membersInfoMap);
                foreach (var combination in combinations)
                {
                    var key = string.Join("&", combination);
                    if (!preloadArgDic.TryGetValue(key, out var infoMap))
                    {
                        infoMap = preloadArgDic[key] = new Dictionary<MemberInfo, object>();
                    }
                    for (int i = 0; i < combination.Count; i++)
                    {
                        var memInfo = membersInfoMap[i][combination[i]];
                        infoMap[memInfo.Key] = memInfo.Value;
                    }
                }

                tree.Add(new SearchTreeGroupEntry(new GUIContent(pathItem)) { level = level });
                foreach (var item in preloadArgDic)
                {
                    var constratMembers = item.Value;
                    tree.Add(new SearchTreeEntry(new GUIContent(item.Key + $"({pathItem})"))
                    {
                        level = level + 1,
                        userData = new Action(() =>
                        {
                            CreateNode(type, context, constratMembers);
                        })
                    });
                }
            }
            else
            {
                tree.Add(new SearchTreeEntry(new GUIContent(pathItem))
                {
                    level = level,
                    userData = new Action(() =>
                    {
                        CreateNode(type, context);
                    })
                });
            }
        }

        private void CreateNewScriptSearch(List<SearchTreeEntry> tree, SearchWindowContext context)
        {
            tree.Add(new SearchTreeGroupEntry(new GUIContent("New Script...")) { level = 1 });

            if (baseType.IsAssignableFrom(typeof(ActionNode)))
            {
                System.Action createActionScript = () => CreateScript(TaskType.Action, context);
                tree.Add(new SearchTreeEntry(new GUIContent($"New Action Script")) { level = 2, userData = createActionScript });
            }
            if (baseType.IsAssignableFrom(typeof(ConditionNode)))
            {

                System.Action createConditionScript = () => CreateScript(TaskType.Condition, context);
                tree.Add(new SearchTreeEntry(new GUIContent($"New Condition Script")) { level = 2, userData = createConditionScript });
            }
            if (baseType.IsAssignableFrom(typeof(MethodNode)))
            {
                System.Action createCompositeScript = () => CreateScript(TaskType.Composite, context);
                tree.Add(new SearchTreeEntry(new GUIContent($"New Method Script")) { level = 2, userData = createCompositeScript });
            }
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            System.Action invoke = (System.Action)searchTreeEntry.userData;
            invoke();
            return true;
        }

        public void CreateNode(System.Type type, SearchWindowContext context, Dictionary<MemberInfo, object> args = null)
        {
            var node = Activator.CreateInstance(type) as BaseNode;
            node.name = GetTypeName(type);
            if (args != null)
            {
                foreach (var item in args)
                {
                    var member = item.Key;
                    var value = item.Value;
                    if (member is FieldInfo fieldInfo)
                    {
                        fieldInfo.SetValue(node, value);
                    }
                    if (member is PropertyInfo propInfo)
                    {
                        propInfo.SetValue(node, value);
                    }
                    if (node.name.Contains($"{{{member.Name}}}"))
                    {
                        node.name = node.name.Replace($"{{{member.Name}}}", value.ToString());
                    }
                }

            }
            createNodeAction?.Invoke(node);
        }

        private string GetTypeName(Type type)
        {
            var attribute = type.GetCustomAttribute<NodePathAttribute>();
            if (attribute != null)
            {
                return attribute.desc;
            }
            return type.Name;
        }

        public void CreateScript(string content)
        {
            EditorApplication.ExecuteMenuItem("Assets/Create/C# Script");
            EditorApplication.update = () =>
            {
                if (Selection.activeObject)
                {
                    var name = Selection.activeObject.name;
                    var path = AssetDatabase.GetAssetPath(Selection.activeObject);
                    content = content.Replace("$ClasName", name);
                    System.IO.File.WriteAllText(path, content);
                    EditorApplication.update = null;
                    AssetDatabase.Refresh();
                }
            };
        }
        public void CreateScript(TaskType type, SearchWindowContext context)
        {
            //模板可修改
            var baseType = typeof(BaseNode);
            switch (type)
            {
                case TaskType.Action:
                    baseType = typeof(ActionNode);
                    break;
                case TaskType.Condition:
                    baseType = typeof(ConditionNode);
                    break;
                case TaskType.Composite:
                    baseType = typeof(MethodNode);
                    break;
                default:
                    break;
            }
            var sb = new StringBuilder();
            sb.AppendLine($"using {baseType.Namespace};");
            sb.AppendLine("namespace Game");
            sb.AppendLine("{");
            sb.AppendLine($"    public class $ClasName : {baseType.Name}");
            sb.AppendLine("    {");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            CreateScript(sb.ToString());
        }

        public static void Show(Vector2 mousePosition, Action<BaseNode> source, Type baseType, List<BaseNode> custonNodes = null)
        {
            Vector2 screenPoint = GUIUtility.GUIToScreenPoint(mousePosition);
            CreateNTNodeWindow searchWindowProvider = CreateInstance<CreateNTNodeWindow>();
            searchWindowProvider.Initialise(source, baseType, custonNodes);
            SearchWindowContext windowContext = new SearchWindowContext(screenPoint, 240, 320);
            SearchWindow.Open(windowContext, searchWindowProvider);
        }
    }
}
