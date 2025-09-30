using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UFrame.Decision;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UFrame.Decision
{
    public class DecisionTree : ScriptableObject
    {
        [SerializeReference]
        public DecisionTreeNode rootNode;

        // 递归方法：根据输入数据判断结果
        public DecisionResult Evaluate(Dictionary<string, object> data)
        {
            return Evaluate(rootNode, data);
        }
        public DecisionResult Evaluate(System.Func<string, object> func)
        {
            return Evaluate(rootNode, func);
        }

        // 递归方法：生成决策树的文本描述
        public string GenerateTreeText()
        {
            StringBuilder sb = new StringBuilder();
            GenerateTreeText(rootNode, 0, sb);
            return sb.ToString();
        }

        public void ImportFromTreeText(string text)
        {
            // 新版结构化文本解析，支持新版GenerateTreeText格式
            if (string.IsNullOrEmpty(text)) return;
            var lines = text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            int index = 0;
            rootNode = ParseNodeFromText(lines, ref index, 0);
        }

        private DecisionTreeNode ParseNodeFromText(string[] lines, ref int index, int level)
        {
            if (index >= lines.Length) return null;
            string line = lines[index];
            int indent = 0;
            while (indent < line.Length && line[indent] == ' ') indent++;
            int curLevel = indent / 4;
            if (curLevel != level) return null;
            string content = line.Trim();

            // 根节点解析
            if (content.StartsWith("|---") && content.Contains("["))
            {
                // 例: |---  [树名] --- 问题
                int nameStart = content.IndexOf('[');
                int nameEnd = content.IndexOf(']');
                int dashIdx = content.IndexOf("---", nameEnd);
                string name = nameStart >= 0 && nameEnd > nameStart ? content.Substring(nameStart + 1, nameEnd - nameStart - 1) : "";
                string question = dashIdx > 0 ? content.Substring(dashIdx + 3).Trim() : "";
                var root = new DecisionRootNode { name = name, question = question };
                root.childs = new List<DecisionTreeNode>();
                index++;
                while (index < lines.Length)
                {
                    var child = ParseNodeFromText(lines, ref index, level + 1);
                    if (child == null) break;
                    root.childs.Add(child);
                }
                return root;
            }

            // 选择节点解析
            if (content.StartsWith("|---") && content.Contains("---") && (content.EndsWith("?") || content.EndsWith("？")))
            {
                // 例: |---  条件 --- 问题?
                int dash1 = content.IndexOf("|---");
                int dash2 = content.IndexOf("---", dash1 + 4);
                int qmark = content.LastIndexOf('?');
                if (qmark == -1) qmark = content.LastIndexOf('？');
                string condStr = content.Substring(dash1 + 4, dash2 - (dash1 + 4)).Trim();
                string question = content.Substring(dash2 + 3, qmark - (dash2 + 3)).Trim();
                var select = new DecisionSelectNode { question = question };
                select.condition = ParseConditionFromString(condStr);
                select.childs = new List<DecisionTreeNode>();
                index++;
                while (index < lines.Length)
                {
                    var child = ParseNodeFromText(lines, ref index, level + 1);
                    if (child == null) break;
                    select.childs.Add(child);
                }
                return select;
            }

            // 叶节点解析
            if (content.StartsWith("|--- ->") && content.Contains("=>"))
            {
                // 例: |--- -> 条件 => 结果
                int arrowIdx = content.IndexOf("->");
                int eqIdx = content.IndexOf("=>", arrowIdx);
                string condStr = content.Substring(arrowIdx + 2, eqIdx - (arrowIdx + 2)).Trim();
                string resultStr = content.Substring(eqIdx + 2).Trim();
                var leaf = new DecisionLeafNode();
                leaf.condition = ParseConditionFromString(condStr);
                leaf.result = ParseResultFromString(resultStr);
                index++;
                return leaf;
            }

            // 其他类型节点
            if (content.StartsWith("|---"))
            {
                // 例: |--- 条件
                string condStr = content.Substring(4).Trim();
                var node = new DecisionSelectNode { question = condStr };
                node.condition = ParseConditionFromString(condStr);
                if (node.condition != null)
                    node.question = condStr.Substring(condStr.LastIndexOf("---") + 3).Trim();
                node.childs = new List<DecisionTreeNode>();
                index++;
                while (index < lines.Length)
                {
                    var child = ParseNodeFromText(lines, ref index, level + 1);
                    if (child == null) break;
                    node.childs.Add(child);
                }
                return node;
            }

            return null;
        }

        protected string GenerateTreeText(DecisionTreeNode node, int level, StringBuilder sb)
        {
            string indent = new string(' ', level * 4);
            if (node == null)
                return sb.ToString();

            // 根节点（DecisionRootNode）
            if (node is DecisionRootNode root)
            {
                string question = string.IsNullOrEmpty(root.Question) ? "[Root]" : root.Question;
                sb.AppendLine($"{indent}|---  [{root.name}] --- {question}");
                foreach (var child in root.Children)
                {
                    GenerateTreeText(child, level + 1, sb);
                }
                return sb.ToString();
            }

            // 选择节点（DecisionSelectNode）
            if (node is DecisionSelectNode select)
            {
                string question = string.IsNullOrEmpty(select.Question) ? "[Select]" : select.Question;
                sb.AppendLine($"{indent}|---  {select.condition} --- {question}?");
                foreach (var child in select.Children)
                {
                    GenerateTreeText(child, level + 1, sb);
                }
                return sb.ToString();
            }

            // 叶节点（DecisionLeafNode）
            if (node is DecisionLeafNode leaf)
            {
                var result = leaf.Result;
                if (result != null)
                {
                    sb.AppendLine($"{indent}|--- -> {leaf.condition} => {result}");
                }
                else
                {
                    sb.AppendLine($"{indent}|--- -> [Empty Result]");
                }
                return sb.ToString();
            }

            // 其他类型节点（如自定义扩展）
            // 显示条件和子节点
            string condStr = node.condition != null ? node.condition.ToString() : "[No Condition]";
            sb.AppendLine($"{indent}|--- {condStr}");
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    GenerateTreeText(child, level + 1, sb);
                }
            }
            return sb.ToString();
        }

        private DecisionResult Evaluate(DecisionTreeNode node, Dictionary<string, object> data)
        {
            if (node.Result != null)
            {
                // 如果是叶节点，返回结果
                return node.Result;
            }

            // 根据条件判断走哪个子节点
            foreach (var child in node.Children)
            {
                child.status = child.condition.Check(data) ? 1 : 2;
                if (child.status == 1)
                {
                    return Evaluate(child, data);
                }
            }
            return null;
        }
        private DecisionResult Evaluate(DecisionTreeNode node, System.Func<string, object> data)
        {
            if (node.Result != null)
            {
                // 如果是叶节点，返回结果
                return node.Result;
            }

            // 根据条件判断走哪个子节点
            foreach (var child in node.Children)
            {
                if (child.condition.Check(data))
                {
                    return Evaluate(child, data);
                }
            }
            return null;
        }

        private DecisionCondition ParseConditionFromString(string condStr)
        {
            // 支持格式: key>value (desc) 或 key=value 或 key!=value (desc)
            string desc = null;
            string main = condStr;
            int parenStart = condStr.IndexOf('(');
            int parenEnd = condStr.LastIndexOf(')');
            if (parenStart == -1) parenStart = condStr.IndexOf('（');
            if (parenEnd == -1) parenEnd = condStr.LastIndexOf('）');
            if (parenStart >= 0 && parenEnd > parenStart)
            {
                if (parenStart < condStr.LastIndexOf("---"))
                    desc = condStr.Substring(parenStart + 1, parenEnd - parenStart - 1).Trim();
                main = condStr.Substring(0, parenStart).Trim();
            }
            // 支持的比较符
            string[] ops = new[] { ">=", "<=", "!=", ">", "<", "=" };
            CompireType compire = CompireType.Equal;
            string key = null;
            string valueStr = null;
            foreach (var op in ops)
            {
                int idx = main.IndexOf(op);
                if (idx > 0)
                {
                    key = main.Substring(0, idx).Trim();
                    valueStr = main.Substring(idx + op.Length).Trim();
                    switch (op)
                    {
                        case "=": compire = CompireType.Equal; break;
                        case ">": compire = CompireType.Bigger; break;
                        case "<": compire = CompireType.Lower; break;
                        case ">=": compire = CompireType.GreaterOrEqual; break;
                        case "<=": compire = CompireType.LessOrEqual; break;
                        case "!=": compire = CompireType.NotEqual; break;
                    }
                    break;
                }
            }
            // 尝试推断类型
            if (int.TryParse(valueStr, out int intVal))
            {
                return new DecisionIntCondition { key = key, value = intVal, checkCompire = compire, desc = desc };
            }
            if (float.TryParse(valueStr, out float floatVal))
            {
                return new DecisionFloatCondition { key = key, value = floatVal, checkCompire = compire, desc = desc };
            }
            if (bool.TryParse(valueStr, out bool boolVal))
            {
                return new DecisionBoolCondition { key = key, value = boolVal, checkCompire = compire, desc = desc };
            }
            if (!string.IsNullOrEmpty(valueStr))
            {
                return new DecisionStringCondition { key = key, value = valueStr, checkCompire = compire, desc = desc };
            }
            // 仅有key和desc
            return new DecisionCondition { key = key, checkCompire = compire, desc = desc };
        }

        private DecisionResult ParseResultFromString(string resultStr)
        {
            if (string.IsNullOrEmpty(resultStr)) return null;
            // 支持格式: key->value (desc) 或 key->value 或 key (desc)
            string desc = null;
            string main = resultStr;
            int parenStart = resultStr.IndexOf('(');
            int parenEnd = resultStr.LastIndexOf(')');
            if (parenStart == -1) parenStart = resultStr.IndexOf('（');
            if (parenEnd == -1) parenEnd = resultStr.LastIndexOf('）');
            if (parenStart >= 0 && parenEnd > parenStart)
            {
                desc = resultStr.Substring(parenStart + 1, parenEnd - parenStart - 1).Trim();
                main = resultStr.Substring(0, parenStart).Trim();
            }
            string key = null;
            string valueStr = null;
            int arrowIdx = main.IndexOf("->");
            if (arrowIdx >= 0)
            {
                key = main.Substring(0, arrowIdx).Trim();
                valueStr = main.Substring(arrowIdx + 2).Trim();
            }
            else
            {
                key = main.Trim();
            }
            // 类型推断
            if (!string.IsNullOrEmpty(valueStr))
            {
                if (int.TryParse(valueStr, out int intVal))
                {
                    return new DecisionIntResult { key = key, value = intVal, desc = desc };
                }
                if (float.TryParse(valueStr, out float floatVal))
                {
                    return new DecisionFloatResult { key = key, value = floatVal, desc = desc };
                }
                if (bool.TryParse(valueStr, out bool boolVal))
                {
                    return new DecisionBoolResult { key = key, value = boolVal, desc = desc };
                }
                return new DecisionStringResult { key = key, value = valueStr, desc = desc };
            }
            // 仅有key和desc
            return new DecisionResult { key = key, desc = desc };
        }

#if UNITY_EDITOR
        [ContextMenu("Import Tree Text From File")]
        public void ImportTreeTextFromFile()
        {
            var path = EditorUtility.OpenFilePanel("导入决策树文本", Application.dataPath, "txt");
            if (!string.IsNullOrEmpty(path))
            {
                var text = System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8);
                ImportFromTreeText(text);
                EditorUtility.SetDirty(this);
            }
        }

        [ContextMenu("Export Tree Text To File")]
        public void ExportTreeTextToFile()
        {
            var path = EditorUtility.SaveFilePanel("导出决策树文本", Application.dataPath, name + ".txt", "txt");
            if (!string.IsNullOrEmpty(path))
            {
                var text = GenerateTreeText();
                System.IO.File.WriteAllText(path, text, System.Text.Encoding.UTF8);
                EditorUtility.RevealInFinder(path);
            }
        }

        [ContextMenu("Import Tree Text From Clipboard")]
        public void ImportTreeTextFromClipboard()
        {
            var text = EditorGUIUtility.systemCopyBuffer;
            if (!string.IsNullOrEmpty(text))
            {
                ImportFromTreeText(text);
                EditorUtility.SetDirty(this);
            }
        }

        [ContextMenu("Export Tree Text To Clipboard")]
        public void ExportTreeTextToClipboard()
        {
            var text = GenerateTreeText();
            EditorGUIUtility.systemCopyBuffer = text;
            Debug.Log(text);
            Debug.Log("决策树文本已复制到剪切板");
        }
#endif
    }
}
