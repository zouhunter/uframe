using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnityEditor;
/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   表生成工具："Assets/Create/TableScript"                                           *
*   文件格式："gb2312/UTF-8"                                                          *
*   表格式：第1行为表名  
*      EffectConfig,特效名称,延时,持续,模型路径                                       *
*      $id,name,delay,duration,modelPath                                              *
*      int,string,float,long,string                                                   *
*                                             *
*   WPS编辑插件: https://github.com/zouhunter/WPS_COOL_CSV                            *
*                                                                                     *
*//************************************************************************************/

using UnityEngine;

namespace UFrame.TableCfg
{
    public class TableScriptEditor
    {
        private const string import = "TableScriptEditor.prefer_csv_folder";
        private const string export = "TableScriptEditor.prefer_cs_folder";

        public static Dictionary<string, System.Type> m_innerTypeDic;
        public static Dictionary<string, string> m_worpTypeNameDic;
        private static CsvHelper csvHelper => CsvHelper.Instance;

        static TableScriptEditor()
        {
            m_innerTypeDic = new Dictionary<string, System.Type>()
            {
                {"byte",typeof(System.Byte) },
                {"sbyte",typeof(System.SByte) },
                {"short",typeof(System.Int16) },
                {"int",typeof(System.Int32) },
                {"long",typeof(System.Int64) },
                {"ushort",typeof(System.UInt16) },
                {"uint",typeof(System.UInt32) },
                {"ulong",typeof(System.UInt64) },
                {"bool",typeof(System.Boolean) },
                {"float",typeof(System.Single) },
                {"double",typeof(System.Double) },
                {"string",typeof(System.String) },
            };
            m_worpTypeNameDic = new Dictionary<string, string>() {
                {"mstring","MString" },
            };
            foreach (var pair in m_innerTypeDic)
            {
                m_worpTypeNameDic[pair.Value.Name] = pair.Key;
            }
            if (TableCfgSetting.Instance.forceMString)
            {
                m_worpTypeNameDic["string"] = "MString";
                m_worpTypeNameDic["String"] = "MString";
            }
        }

        private static string LoadPreferFolder(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetString(key);
            }
            return Application.dataPath;
        }

        private static void SavePreferFolder(string key, string folder)
        {
            PlayerPrefs.SetString(key, folder);
            PlayerPrefs.Save();
        }

        public static List<string> TableDescs(TableData tableData)
        {
            if (tableData.Rows.Count < 1)
                tableData.Rows.Add(new List<string>());
            return tableData.Rows[0];
        }

        public static List<string> TableFields(TableData tableData)
        {
            if (tableData.Rows.Count < 1)
                tableData.Rows.Add(new List<string>());
            if (tableData.Rows.Count < 2)
                tableData.Rows.Add(new List<string>());
            return tableData.Rows[1];
        }

        public static List<string> TableTypes(TableData tableData)
        {
            if (tableData.Rows.Count < 1)
                tableData.Rows.Add(new List<string>());
            if (tableData.Rows.Count < 2)
                tableData.Rows.Add(new List<string>());
            if (tableData.Rows.Count < 3)
                tableData.Rows.Add(new List<string>());
            return tableData.Rows[2];
        }
        /// <summary> 
        /// 给定文件的路径，读取文件的二进制数据，判断文件的编码类型 
        /// </summary> 
        /// <param name=“filePath“>文件路径</param> 
        /// <returns>文件的编码类型</returns> 
        public static System.Text.Encoding GetFileEncoding(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                Encoding r = GetFileEncoding(fs);
                return r;
            }
        }

        /// <summary> 
        /// 通过给定的文件流，判断文件的编码类型 
        /// </summary> 
        /// <param name=“fs“>文件流</param> 
        /// <returns>文件的编码类型</returns> 
        public static System.Text.Encoding GetFileEncoding(FileStream fs)
        {
            //byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            //byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            //byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM 
            Encoding reVal = Encoding.GetEncoding("gb2312");
            using (BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default))
            {
                int i;
                int.TryParse(fs.Length.ToString(), out i);
                byte[] ss = r.ReadBytes(i);
                if ((ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
                {
                    reVal = new UTF8Encoding(true);
                }
                else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
                {
                    reVal = Encoding.BigEndianUnicode;
                }
                else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
                {
                    reVal = Encoding.Unicode;
                }
                else if (IsUTF8Bytes(ss))
                {
                    reVal = new UTF8Encoding(false);
                }
            }
            return reVal;
        }

        /// <summary> 
        /// 判断是否是不带 BOM 的 UTF8 格式 
        /// </summary> 
        /// <param name=“data“></param> 
        /// <returns></returns> 
        private static bool IsUTF8Bytes(byte[] data)
        {
            int charByteCounter = 1; //计算当前正分析的字符应还有的字节数 
            byte curByte; //当前分析的字节. 
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        //判断当前 
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X 
                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //若是UTF-8 此时第一位必须为1 
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                throw new Exception("非预期的byte格式");
            }
            return true;
        }

        // 从csv文件创建脚本
        public static string CreateScript(string csvFile, out string scriptName, bool ignoreRef)
        {
            var encoding = GetFileEncoding(csvFile);
            scriptName = GetFileFunName(System.IO.Path.GetFileNameWithoutExtension(csvFile)) + "Cfg";
            var table = csvHelper.GetCacheData();
            csvHelper.ReadCSVToTableData(csvFile, encoding, table);
            if (table != null)
            {
                if (TableDescs(table).Count > 0)
                {
                    var tableName = GetTableName(table);
                    if (tableName.StartsWith("&") && !ignoreRef)
                    {
                        Debug.Log("ignore ref table:" + tableName);
                        return null;
                    }
                    var match = System.Text.RegularExpressions.Regex.Match(tableName, @"[\w.]+");
                    if (match.Success)
                    {
                        scriptName = match.Value;
                    }
                }
                var script = MakeScriptFromTable(scriptName, table);
                if (!string.IsNullOrEmpty(script))
                {
                    return script;
                }
                else
                {
                    Debug.LogError("代码生成失败，请检查表格是否合格！" + csvFile);
                }
                table.Dispose();
            }
            else
            {
                Debug.LogError("表格读取失败，请选择正确的表格！" + csvFile);
            }
            return null;
        }

        static string GetTableName(TableData table)
        {
            if (table.Descs().Count > 0)
            {
                var tableName = table.Descs()[0];
                var match2 = System.Text.RegularExpressions.Regex.Match(tableName, @"\$[\w.]+");
                if (match2.Success)
                {
                    return tableName.Substring(1);
                }
                var match = System.Text.RegularExpressions.Regex.Match(tableName, @"[\w.]+Cfg");
                if (match.Success)
                {
                    return tableName;
                }
               
            }
            return GetFileFunName(table.name) + "Cfg";
        }

        // 创建代理表类
        static string CreateProxyTableScript(string csvFile, out string scriptName)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(csvFile);
            var cfgName = GetFileFunName(fileName);
            scriptName = cfgName + "Table";
            var table = csvHelper.GetCacheData();
            csvHelper.ReadCSVToTableData(csvFile, System.Text.Encoding.GetEncoding("GB2312"), table);
            if (table != null)
            {

                var script = MakeProxyTableScriptFromTable(scriptName, fileName, table);
                if (!string.IsNullOrEmpty(script))
                {
                    return script;
                }
                else
                {
                    Debug.LogError("代码生成失败,请检查表格是否合格！" + csvFile);
                }
                table.Dispose();
            }
            else
            {
                Debug.LogError("表格读取失败，请选择正确的表格！" + csvFile);
            }
            return null;
        }

        [MenuItem("Tools/TableScript/(from file)")]
        static void CreateTableScriptFromCsv()
        {
            var csvFile = EditorUtility.OpenFilePanel("先选择配制文档", LoadPreferFolder(import), "");
            if (!string.IsNullOrEmpty(csvFile))
            {
                SavePreferFolder(import, System.IO.Path.GetDirectoryName(csvFile));
                string scriptName;
                var script = CreateScript(csvFile, out scriptName, true);
                if (string.IsNullOrEmpty(script))
                    return;
                var savePath = EditorUtility.SaveFilePanel("保存文档", LoadPreferFolder(export), scriptName, "cs");
                if (!string.IsNullOrEmpty(savePath))
                {
                    var saveDir = System.IO.Path.GetDirectoryName(savePath);
                    SavePreferFolder(export, saveDir);
                    System.IO.File.WriteAllText(savePath, script, System.Text.Encoding.UTF8);
                    AssetDatabase.Refresh();
                    OpenSelectFolder(saveDir);
                }
            }
        }

        [MenuItem("Tools/TableScript/(from folder)")]
        static void CreateTableScriptFromCsvs()
        {
            var csvFolder = EditorUtility.OpenFolderPanel("先选择配制文档文件夹", LoadPreferFolder(import), "");
            if (!string.IsNullOrEmpty(csvFolder))
            {
                SavePreferFolder(import, csvFolder);

                var deepFile = System.IO.Directory.GetDirectories(csvFolder).Length > 0;
                if (deepFile)
                    deepFile = EditorUtility.DisplayDialog("选择", "遍历表子目录？", "是", "否");
                var option = deepFile ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;
                var files = new List<string>();
                files.AddRange(System.IO.Directory.GetFiles(csvFolder, "*.csv", option));
                files.AddRange(System.IO.Directory.GetFiles(csvFolder, "*.txt", option));
                if (files != null && files.Count > 0)
                {
                    var saveDir = EditorUtility.SaveFolderPanel("保存文档文件夹", LoadPreferFolder(export), "");
                    if (string.IsNullOrEmpty(saveDir))
                        return;

                    SavePreferFolder(export, saveDir);
                    for (int i = 0; i < files.Count; i++)
                    {
                        var csvFile = files[i];
                        string scriptName;
                        var script = CreateScript(csvFile, out scriptName, false);
                        if (!string.IsNullOrEmpty(scriptName) && !string.IsNullOrEmpty(script))
                        {
                            var savePath = string.Format("{0}/{1}.cs", saveDir, scriptName);
                            System.IO.File.WriteAllText(savePath, script, System.Text.Encoding.UTF8);
                        }
                    }
                    AssetDatabase.Refresh();
                    OpenSelectFolder(saveDir);
                }

            }
        }

        [MenuItem("Tools/TableScript/(table proxy from file)")]
        static void CreateTableProxyFromCsv()
        {
            var csvFile = EditorUtility.OpenFilePanel("先选择配制文档", LoadPreferFolder(import), "");
            if (!string.IsNullOrEmpty(csvFile))
            {
                SavePreferFolder(import, System.IO.Path.GetDirectoryName(csvFile));

                string scriptName;
                var script = CreateProxyTableScript(csvFile, out scriptName);
                if (!string.IsNullOrEmpty(scriptName) && !string.IsNullOrEmpty(script))
                {
                    var saveDir = EditorUtility.SaveFolderPanel("保存文档文件夹", LoadPreferFolder(export), "");
                    if (string.IsNullOrEmpty(saveDir))
                        return;
                    SavePreferFolder(export, saveDir);

                    var savePath = string.Format("{0}/{1}.cs", saveDir, scriptName);
                    System.IO.File.WriteAllText(savePath, script, System.Text.Encoding.UTF8);
                    AssetDatabase.Refresh();
                    OpenSelectFolder(saveDir);
                }
            }
        }

        [MenuItem("Tools/TableScript/(table proxy from folder)")]
        static void CreateTableProxysFromCsvs()
        {
            var csvFolder = EditorUtility.OpenFolderPanel("先选择配制文档文件夹", LoadPreferFolder(import), "");
            if (!string.IsNullOrEmpty(csvFolder))
            {
                SavePreferFolder(import, csvFolder);

                var deepFile = System.IO.Directory.GetDirectories(csvFolder).Length > 0;
                if (deepFile)
                    deepFile = EditorUtility.DisplayDialog("选择", "遍历表子目录？", "是", "否");
                var option = deepFile ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;
                var files = new List<string>();
                files.AddRange(System.IO.Directory.GetFiles(csvFolder, "*.csv", option));
                files.AddRange(System.IO.Directory.GetFiles(csvFolder, "*.txt", option));
                if (files != null && files.Count > 0)
                {
                    var saveDir = EditorUtility.SaveFolderPanel("保存文档文件夹", LoadPreferFolder(export), "");
                    if (string.IsNullOrEmpty(saveDir))
                        return;

                    SavePreferFolder(export, saveDir);
                    for (int i = 0; i < files.Count; i++)
                    {
                        var csvFile = files[i];
                        string scriptName;
                        var script = CreateProxyTableScript(csvFile, out scriptName);
                        if (!string.IsNullOrEmpty(scriptName) && !string.IsNullOrEmpty(script))
                        {
                            var savePath = string.Format("{0}/{1}.cs", saveDir, scriptName);
                            System.IO.File.WriteAllText(savePath, script, System.Text.Encoding.UTF8);
                        }
                    }
                    AssetDatabase.Refresh();
                    OpenSelectFolder(saveDir);
                }
            }
        }

        /// <summary>
        /// 创建table脚本相关脚本到指定文件夹
        /// </summary>
        /// <param name="csvFiles"></param>
        /// <param name="outDir"></param>
        public static void CreateTableScripts(string csvFolder, string ctrlName, string outDir)
        {
            var deepFile = System.IO.Directory.GetDirectories(csvFolder).Length > 0;
            if (deepFile)
                deepFile = EditorUtility.DisplayDialog("选择", "遍历表子目录？", "是", "否");
            var option = deepFile ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;
            var csvFiles = new List<string>();
            csvFiles.AddRange(System.IO.Directory.GetFiles(csvFolder, "*.csv", option));
            csvFiles.AddRange(System.IO.Directory.GetFiles(csvFolder, "*.txt", option));
            if (csvFiles == null || csvFiles.Count <= 0 || string.IsNullOrEmpty(outDir))
                return;

            if (!System.IO.Directory.Exists(outDir))
                System.IO.Directory.CreateDirectory(outDir);

            for (int i = 0; i < csvFiles.Count; i++)
            {
                var csvFile = csvFiles[i];
                string scriptName;
                var script = CreateScript(csvFile, out scriptName, false);
                if (!string.IsNullOrEmpty(scriptName) && !string.IsNullOrEmpty(script))
                {
                    var savePath = string.Format("{0}/{1}.cs", outDir, scriptName);
                    System.IO.File.WriteAllText(savePath, script, System.Text.Encoding.UTF8);
                }
            }
            var ctrlScriptPath = System.IO.Path.Combine(outDir, ctrlName + ".cs");
            var ctrlScript = MakeTableCtrlScript(ctrlName, csvFolder, csvFiles.ToArray());
            if (!string.IsNullOrEmpty(ctrlScript))
            {
                System.IO.File.WriteAllText(ctrlScriptPath, ctrlScript.Replace("\r\n", "\n"), System.Text.Encoding.UTF8);
            }
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 表单
        /// </summary>
        /// <param name="scriptName"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        private static string MakeScriptFromTable(string scriptName, TableData table)
        {
            string tableNameSpace = "UFrame.TableCfg";
            string namespaceStr = tableNameSpace;
            var lastPoint = scriptName.LastIndexOf('.');
            bool useTableNameSpace = false;
            if (lastPoint > 0)
            {
                namespaceStr = scriptName.Substring(0, lastPoint);
                scriptName = scriptName.Substring(lastPoint + 1);
                useTableNameSpace = true;
            }
            var cfgType = FindTypeInAllAssemble(scriptName);
            if (cfgType == null)
                cfgType = FindTypeInAllAssemble($"UFrame.TableCfg.{scriptName}");
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            bool makeValueMap = TableCfgSetting.Instance.valueMapConfigs.Contains($"{namespaceStr}.{scriptName}");
            var descs = table.Descs().Select(x => x.ToString()).ToList();
            var types = table.Types().Select(x => x.ToString()).ToList();
            var fields = table.Fields().Select(x => x.ToString()).ToList();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.IO;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using UnityEngine;");
            if (useTableNameSpace)
                sb.AppendLine(string.Format("using {0};", tableNameSpace));
            sb.AppendLine();
            sb.AppendLine("namespace " + namespaceStr);
            sb.AppendLine("{");
            sb.AppendLine("    [System.Serializable]");
            List<int> idIndexs = new List<int>();
            bool firstAsKey = false;
            if (fields.Find(x => x.StartsWith('$')) == null)
                firstAsKey = true;
            for (int i = 0; i < fields.Count; i++)
            {
                var type = types[i].ToString();
                var field = fields[i].ToString();
                var isKey = field.StartsWith("$") || (i == 0 && field.ToLower() == "id");
                if (field.StartsWith("$"))
                {
                    field = field.Substring(1, field.Length - 1);
                    fields[i] = field;
                }
                types[i] = GetRealTypeName(types[i], isKey);
                if (isKey || (firstAsKey && i == 0))
                {
                    idIndexs.Add(i);
                }
            }
            string classBaseType = "IRow";
            List<System.Tuple<string, string, string>> keyProps = new List<System.Tuple<string, string, string>>();
            if (idIndexs.Count > 0)
            {
                classBaseType = "IRow" + ToGenericString(idIndexs.Select(index => types[index].ToString()).ToArray());
                int keyIndex = 1;
                keyProps.AddRange(idIndexs.Select(index => new System.Tuple<string, string, string>(types[index].ToString(), "K" + (keyIndex++).ToString(), fields[index].ToString())).ToArray());
            }
            WriteCfgScript("    ", sb, scriptName, classBaseType, keyProps, null, fields, types, descs, makeValueMap);
            sb.AppendLine("}");
            return sb.ToString().Replace("\r\n", "\n");
        }

        /// <summary>
        /// 表控制模板
        /// </summary>
        /// <returns></returns>
        private static string MakeTableCtrlScript(string scriptName, string csvRoot, string[] csvFiles)
        {
            int cfgNum = csvFiles.Length;
            var fileNames = new string[cfgNum];
            var keyIds = new string[cfgNum];
            var tableFields = new string[cfgNum];
            var tableKeys = new Dictionary<int, string[]>();
            var tableResourcePaths = new string[cfgNum];
            var scriptTypes = new string[cfgNum];
            var tableArgArray = new string[cfgNum];
            var fileFuncNames = new string[cfgNum];

            for (int i = 0; i < cfgNum; i++)
            {
                var filePath = csvFiles[i];
                var ext = System.IO.Path.GetExtension(filePath);
                var fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                var csvTable = csvHelper.GetCacheData();
                csvTable.name = fileName;
                csvHelper.ReadCSVToTableData(csvFiles[i], System.Text.Encoding.GetEncoding("GB-2312"), csvTable);
                fileNames[i] = fileName;
                keyIds[i] = GetKeyIdName(fileName);
                fileFuncNames[i] = GetFileFunName(fileName);
                var resPath = filePath.Replace("\\", "/").Replace(csvRoot.Replace("\\", "/") + "/", "");
                tableResourcePaths[i] = resPath.Substring(0, resPath.Length - ext.Length);
                tableFields[i] = fileName[0].ToString().ToLower() + fileName.Substring(1) + "Table";
                List<string> keyTypes = new List<string>();
                bool firstAsKey = false;
                if (csvTable.Fields().Find(x => x.StartsWith('$')) == null)
                    firstAsKey = true;

                for (int j = 0; j < csvTable.Fields().Count; j++)
                {
                    var field = csvTable.Fields()[j].ToString();
                    var isKey = field.StartsWith("$") || (j == 0 && field.ToLower() == "id");
                    if (isKey || (firstAsKey && j == 0))
                    {
                        var typeStr = csvTable.Types()[j].ToString();
                        if (typeStr == "string" || m_innerTypeDic.ContainsKey(typeStr.ToLower()))
                        {
                            keyTypes.Add(typeStr);
                        }
                    }
                }
                scriptTypes[i] = fileName;
                if (csvTable.Descs().Count > 0 && csvTable.Descs()[0].ToString() != "id" && csvTable.Descs()[0].ToString() != "$id")
                {
                    scriptTypes[i] = GetTableName(csvTable);
                }

                var tableArgs = scriptTypes[i];

                if (keyTypes.Count > 0)
                {
                    tableKeys[i] = keyTypes.ToArray();

                    var genArgs = "";
                    for (int j = 0; j < keyTypes.Count; j++)
                    {
                        if (j == 0)
                            genArgs = keyTypes[j];
                        else
                            genArgs = genArgs + "," + keyTypes[j];
                    }
                    tableArgs = genArgs + "," + tableArgs;
                }
                tableArgArray[i] = tableArgs;
                csvTable.Dispose();
            }
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("///*************************************************************************************/");
            sb.Append("///* 创建时间：       "); sb.AppendLine(System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            sb.AppendLine("///* 说    明：       1.本代码由表格工具生成");
            sb.AppendLine("///                   2.扩展方式为继承");
            sb.AppendLine("///                   3.使用时请先调用StartLoadAll");
            sb.AppendLine("///* ************************************************************************************/\n");
            sb.AppendLine("using UnityEngine;\n");
            sb.AppendLine("namespace UFrame.TableCfg");
            sb.AppendLine("{");
            sb.AppendFormat("    public class {0} : TableController\n", scriptName);
            sb.AppendLine("    {");
            sb.AppendFormat("        public const int TABLE_NUM = {0};\n", cfgNum);
            for (int i = 0; i < cfgNum; i++)
            {
                sb.AppendFormat("        public static int {0} => {1};\n", keyIds[i], fileNames[i].GetHashCode());
            }
            for (int i = 0; i < cfgNum; i++)
            {
                sb.AppendFormat("        public Table<{0}> {1} {{ get; protected set; }}\n", tableArgArray[i], tableFields[i]);
            }
            sb.AppendLine("        protected int m_loadedCount;");
            sb.AppendLine("        protected TableLoadEvent m_onLoadCallBack;\n");
            sb.AppendLine("        public delegate void TableLoadEvent(int tableId, int loadedCount);\n");
            sb.AppendLine("        public virtual void StartLoadAll(TableLoadEvent onLoadCallBack)");
            sb.AppendLine("        {");
            sb.AppendLine("            m_loadedCount = 0;");
            sb.AppendLine("            m_onLoadCallBack = onLoadCallBack;");
            for (int i = 0; i < cfgNum; i++)
            {
                sb.AppendFormat("            LoadTable<{0}>({1}, \"{2}\", OnLoad{3}Table);\n", tableArgArray[i], keyIds[i], tableResourcePaths[i], fileFuncNames[i]);
            }
            sb.AppendLine("        }");

            for (int i = 0; i < cfgNum; i++)
            {
                sb.AppendFormat("        #region {0}", fileFuncNames[i]);
                string onLoadFuncStr = @"
        protected void OnLoad{0}Table(Table<{1}> table)
        {{
            m_loadedCount++;
            if (m_onLoadCallBack != null)
            {{
                {2} = table;
                m_onLoadCallBack.Invoke({3}, m_loadedCount);
            }}
            if (table == null)
            {{
                Debug.LogError({4});
            }}
        }}";
                var errorMsg = string.Format("\"failed load {0}\"", fileNames[i]);
                sb.AppendFormat(onLoadFuncStr, fileFuncNames[i], tableArgArray[i], tableFields[i], keyIds[i], errorMsg);

                string getTableFunStr = @"
        public Table<{0}> Get{1}Table()
        {{
            if ({2} != null)
                return {2};
            return GetTable<{0}>({3});
        }}";
                sb.AppendFormat(getTableFunStr, tableArgArray[i], fileFuncNames[i], tableFields[i], keyIds[i]);

                string tryGetTableFuncStr = @"
        public bool TryGet{0}Table(out Table<{1}> table)
        {{
            if ({2} != null)
            {{
                table = {2};
                return true;
            }}
            return TryGetTable<Table<{1}>>({3}, out table);
        }}";
                sb.AppendFormat(tryGetTableFuncStr, fileFuncNames[i], tableArgArray[i], tableFields[i], keyIds[i]);
                string getCfgFuncStr = @"
        public {0} Get{1}ByLine(int line)
        {{
            if ({2} != null)
                return {2}.GetByLine(line);
            return GetConfig<{0}>({3}, line);
        }}";
                sb.AppendFormat(getCfgFuncStr, scriptTypes[i], fileFuncNames[i], tableFields[i], keyIds[i]);

                if (tableKeys.TryGetValue(i, out var keyTypes))
                {
                    var getCfgByKeyFuncStr = @"
        public {0} Get{1}ByKey({2})
        {{
            if ({3} != null)
                return {3}.GetByKey({4});
            return GetConfig<{5}>({6}, {4});
        }}";
                    string keyArgs = "";
                    string keyArgs2 = "";
                    for (int j = 0; j < keyTypes.Length; j++)
                    {
                        keyArgs += string.Format("{0} key{1}", keyTypes[j], j + 1);
                        keyArgs2 += string.Format("key{0}", (j + 1));
                        if (j < keyTypes.Length - 1)
                        {
                            keyArgs += ",";
                            keyArgs2 += ",";
                        }
                    }
                    sb.AppendFormat(getCfgByKeyFuncStr, scriptTypes[i], fileFuncNames[i], keyArgs, tableFields[i], keyArgs2, tableArgArray[i], keyIds[i]);
                }
                sb.AppendFormat("\n        #endregion {0}\n\n", fileFuncNames[i]);
            }
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString().Replace("\r\n", "\n");
        }


        /// <summary>
        /// 单表代理控制器
        /// </summary>
        /// <param name="scriptName"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        private static string MakeProxyTableScriptFromTable(string scriptName, string fileName, TableData table)
        {
            try
            {
                var cfgName = GetTableName(table);
                var cfgType = FindTypeInAllAssemble(cfgName);
                if (cfgType == null)
                    cfgType = FindTypeInAllAssemble($"UFrame.TableCfg.{cfgName}");
                if (cfgType == null)
                {
                    Debug.LogError("failed find class type:" + cfgName);
                    return null;
                }
                var descs = table.Descs().Select(x => x.ToString()).ToList();
                var types = table.Types().Select(x => x.ToString()).ToList();
                var fields = table.Fields().Select(x => x.ToString()).ToList();
                string optionKeyTypes = "";
                bool firstAsKey = false;
                if (fields.Find(x => x.StartsWith('$')) == null)
                    firstAsKey = true;
                List<int> idIndexs = new List<int>();
                for (int i = 0; i < fields.Count; i++)
                {
                    var type = types[i].ToString();
                    var field = fields[i].ToString();
                    var isKey = field.StartsWith("$") || (i == 0 && field.ToLower() == "id");
                    if (field.StartsWith("$"))
                    {
                        field = field.Substring(1, field.Length - 1);
                        fields[i] = field;
                    }
                    types[i] = GetRealTypeName(types[i], isKey);
                    if (isKey || (firstAsKey && i == 0))
                    {
                        idIndexs.Add(i);
                        optionKeyTypes += $"{type},";
                    }
                }
                string classBaseType = "";
                if (cfgType != null)
                {
                    if (string.IsNullOrEmpty(cfgType.Namespace) || cfgType.Namespace == "UFrame.TableCfg")
                        classBaseType = cfgType.Name + ", ";
                    else
                        classBaseType = cfgType.FullName + ", ";
                }
                List<System.Tuple<string, string, string>> keyProps = new List<System.Tuple<string, string, string>>();
                if (idIndexs.Count > 0)
                {
                    classBaseType += "IRow" + ToGenericString(idIndexs.Select(index => types[index].ToString()).ToArray());
                    int keyIndex = 1;
                    keyProps.AddRange(idIndexs.Select(index => new System.Tuple<string, string, string>(types[index].ToString(), "K" + (keyIndex++).ToString(), fields[index].ToString())).ToArray());
                }
                else
                {
                    classBaseType += "IRow";
                }

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("//*************************************************************************************");
                sb.AppendLine("//* 作    者： z hunter");
                sb.AppendLine($"//* 创建时间： {System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}");
                sb.AppendLine("//* 描    述： 单表代理结构");
                sb.AppendLine("//* ************************************************************************************");

                sb.AppendLine("using System;");
                sb.AppendLine("using System.Collections;");
                sb.AppendLine("using System.Collections.Generic;");
                sb.AppendLine("using UnityEngine;");
                sb.AppendLine("using UFrame.TableCfg;");
                sb.AppendLine();

                sb.AppendLine($"public class {scriptName} : ProxyTable<{scriptName},{optionKeyTypes} {cfgType.Name}>");
                sb.AppendLine("{");
                sb.AppendLine($"    public override string FileName {{ get; protected set; }} = \"{fileName}\";");

                if (TableCfgSetting.Instance.genCsvReader)
                {
                    sb.AppendLine();
                    sb.AppendLine($"    protected override ILineRowReader CreateLineRowReader()");
                    sb.AppendLine("    {");
                    sb.AppendLine($"        return new {cfgType.Name}LineRowReader();");
                    sb.AppendLine("    }");
                }

                if (TableCfgSetting.Instance.genBinReader)
                {
                    sb.AppendLine();
                    sb.AppendLine($"    protected override IBinaryRowReader CreateBinaryRowReader()");
                    sb.AppendLine("    {");
                    sb.AppendLine($"        return new {cfgType.Name}BinaryRowReader();");
                    sb.AppendLine("    }");
                }

                if (TableCfgSetting.Instance.genBinWriter)
                {
                    sb.AppendLine();
                    sb.AppendLine($"    protected override IBinaryRowWriter CreateBinaryRowWriter()");
                    sb.AppendLine("    {");
                    sb.AppendLine($"        return new {cfgType.Name}BinaryRowWriter();");
                    sb.AppendLine("    }");
                }
                
                sb.AppendLine("}");
                return sb.ToString().Replace("\r\n", "\n");
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }
        private static void WriteCfgScript(string span, System.Text.StringBuilder sb, string className, string classBaseType,
            List<System.Tuple<string, string, string>> keyProps, System.Type cfgType, List<string> fields, List<string> types, List<string> descs, bool makeValueMap)
        {
            sb.AppendLine($"{span}public class {className} : {classBaseType}");
            sb.AppendLine($"{span}{{");

            if (keyProps.Count > 0)
            {
                for (int kIndex = 0; kIndex < keyProps.Count; kIndex++)
                {
                    var tuple = keyProps[kIndex];
                    var typeName = tuple.Item1;
                    var propName = tuple.Item2;
                    var propValue = tuple.Item3;

                    if (cfgType != null && (cfgType.GetField(propName) != null || cfgType.GetProperty(propName) != null))
                        continue;

                    sb.AppendLine($"{span}    public {typeName} {propName} => {propValue};");
                }
            }

            for (int i = 0; i < fields.Count; i++)
            {
                var typeName = types[i];
                var field = fields[i];

                if (cfgType != null && (cfgType.GetField(field) != null || cfgType.GetProperty(field) != null))
                    continue;

                sb.Append($"{span}    public ");
                sb.Append(typeName); sb.Append(" "); sb.Append(field); sb.Append(";");
                sb.Append($"/*{i}*/");
                sb.Append(" //"); if (descs.Count > i) sb.Append(descs[i].Replace("\n", "-"));
                sb.AppendLine();
            }

            if (makeValueMap)
            {
                sb.AppendLine();
                sb.AppendLine($"{span}    public Dictionary<string, object> valueMap = new Dictionary<string, object>();");
                sb.AppendLine($"{span}    public object this[string fieldName]");
                sb.AppendLine($"{span}    {{");
                sb.AppendLine($"{span}        get");
                sb.AppendLine($"{span}        {{");
                sb.AppendLine($"{span}            if (valueMap.TryGetValue(fieldName, out var value))");
                sb.AppendLine($"{span}                return value;");
                sb.AppendLine($"{span}            return null;");
                sb.AppendLine($"{span}        }}");
                sb.AppendLine($"{span}    }}");
            }

            sb.AppendLine($"{span}}}");
            
            
            if (TableCfgSetting.Instance.genBinWriter)
            {
                sb.AppendLine();
                CreateBinaryWriterScript(sb, span, className, fields, types);
            }

            if (TableCfgSetting.Instance.genCsvReader)
            {
                sb.AppendLine();
                CreateLineReaderScript(sb, span, className, fields, types, makeValueMap);
            }

            
            if (TableCfgSetting.Instance.genBinReader)
            {
                sb.AppendLine();
                CreateBinaryReaderScript(sb, span, className, fields, types);
            }
        }

        //csv line reader
        private static void CreateLineReaderScript(StringBuilder sb, string span, string className, List<string> fields, List<string> types, bool makeValueMap)
        {
            var memberSpan = span + "    ";
            var scriptLogicSpan = memberSpan + "    ";
            sb.AppendLine($"{span}public class {className}LineRowReader :  LineRowReader<{className}>");
            sb.AppendLine($"{span}{{");

            for (int i = 0; i < fields.Count; i++)
            {
                sb.AppendLine($"{memberSpan} private int i_{fields[i]};");
            }
            sb.AppendLine();
            sb.AppendLine($"{memberSpan}public override void SetFields(string[] fields)");
            sb.AppendLine($"{memberSpan}{{");
            sb.AppendLine($"{scriptLogicSpan}base.SetFields(fields);");
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                sb.AppendLine($"{scriptLogicSpan}i_{field} = Array.IndexOf(fields, \"{field}\");");
            }
            sb.AppendLine($"{memberSpan}}}");

            sb.AppendLine($"{memberSpan}public override void ReadDatas(List<string> values)");
            sb.AppendLine($"{memberSpan}{{");
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                var type = types[i].Trim();
                sb.Append(scriptLogicSpan);
                if (makeValueMap)
                    sb.Append($"row.valueMap[\"{field}\"] = ");

                sb.Append($"row.{field} = ");
                var valueStr = $"values[i_{field}]";
                if (type == "object" || type == "string")
                {
                    sb.AppendLine($"{valueStr};");
                }
                else if (type == "bool")
                {
                    sb.AppendLine($"UConvert.Instance.ToBool({valueStr});");
                }
                else
                {
                    string converKey = null;
                    if (m_innerTypeDic.TryGetValue(type.ToLower(), out System.Type typeInstance))
                    {
                        converKey = "To" + typeInstance.Name;
                    }
                    if (!string.IsNullOrEmpty(converKey))
                    {
                        sb.AppendLine($"Convert.{converKey}({valueStr});");
                    }
                    else
                    {
                        converKey = GetUConverKey(type);
                        sb.AppendLine($"UConvert.Instance.{converKey}({valueStr});");
                    }
                }
            }
            sb.AppendLine($"{memberSpan}}}");
            sb.AppendLine($"{span}}}");
        }

        //binary line writer
        private static void CreateBinaryWriterScript(StringBuilder sb, string span, string className, List<string> fields, List<string> types)
        {
            var memberSpan = span + "    ";
            var scriptLogicSpan = memberSpan + "    ";
            sb.AppendLine($"{span}public class {className}BinaryRowWriter :  BinaryRowWriter<{className}>");
            sb.AppendLine($"{span}{{");
            sb.AppendLine($"{memberSpan}public override void WriteDatas(BinaryWriter writer)");
            sb.AppendLine($"{memberSpan}{{");
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                var type = types[i].Trim();
                var valueStr = $"row.{field}";
                if (type == "object")
                {
                    sb.AppendLine($"{scriptLogicSpan}writer.Write({valueStr}.ToString());");
                }
                else if (m_innerTypeDic.ContainsKey(type) || "MString" == type)
                {
                    sb.AppendLine($"{scriptLogicSpan}writer.Write({valueStr});");
                }
                else
                {
                    var typeName = TypeReadableName(type);
                    sb.AppendLine($"{scriptLogicSpan}UConvert.Instance.Write{typeName}ToBinary({valueStr},writer);");
                }
            }
            sb.AppendLine($"{memberSpan}}}");
            sb.AppendLine($"{span}}}");
        }

        //binary line reader
        private static void CreateBinaryReaderScript(StringBuilder sb, string span, string className, List<string> fields, List<string> types)
        {
            var memberSpan = span + "    ";
            var scriptLogicSpan = memberSpan + "    ";
            sb.AppendLine($"{span}public class {className}BinaryRowReader :  BinaryRowReader<{className}>");
            sb.AppendLine($"{span}{{");
            sb.AppendLine($"{memberSpan}public override void ReadDatas(BinaryReaderContent reader)");
            sb.AppendLine($"{memberSpan}{{");
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                var type = types[i].Trim();
                sb.Append($"{scriptLogicSpan}row.{field} = ");
                if (type.ToLower() == "string" || type.ToLower() == "object")
                {
                    sb.AppendLine("reader.ReadString();");
                }
                else if (type.ToLower() == "MString")
                {
                    sb.AppendLine("reader.ReadMString();");
                }
                else
                {
                    if (m_innerTypeDic.TryGetValue(type.ToLower(), out var worpType))
                    {
                        sb.AppendLine($"reader.Read{worpType.Name}();");
                    }
                    else
                    {
                        var typeName = TypeReadableName(type);
                        sb.AppendLine($"UConvert.Instance.Read{typeName}(reader);");
                    }
                }
            }
            sb.AppendLine($"{memberSpan}}}");
            sb.AppendLine($"{span}}}");
        }

        /// <summary>
        /// 获取常量key
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string GetKeyIdName(string name)
        {
            var keyIdName = "";
            for (int i = 0; i < name.Length; i++)
            {
                var charI = name[i];
                if (i > 0 && charI < 97)
                {
                    keyIdName += "_";
                }
                keyIdName += charI.ToString().ToUpper();
            }
            return (keyIdName + "_ID").Replace("__", "_");
        }

        private static string GetFileFunName(string fileName)
        {
            var fileFuncName = "";
            var upperNext = false;
            for (int i = 0; i < fileName.Length; i++)
            {
                var charI = fileName[i];
                if (i == 0 && charI >= 97)
                {
                    fileFuncName += charI.ToString().ToUpper();
                }
                else
                {
                    if (charI == '_' && i < fileName.Length - 1)
                    {
                        upperNext = true;
                    }
                    else
                    {
                        if (upperNext)
                        {
                            fileFuncName += charI.ToString().ToUpper();
                        }
                        else
                        {
                            fileFuncName += charI;
                        }
                        upperNext = false;
                    }
                }
            }
            return fileFuncName;
        }


        private static string ToGenericString(params string[] args)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<");
            sb.Append(args[0]);
            for (int i = 1; i < args.Length; i++)
            {
                sb.Append(",");
                sb.Append(args[i]);
            }
            sb.Append(">");
            return sb.ToString();
        }

        private static string TypeReadableName(string type)
        {
            if (type.Contains("[]"))//数组
            {
                type = type.Replace("[]", "Array");
            }
            else if (type.Contains("[,]"))//二组数组
            {
                type = type.Replace("[,]", "DoubleArray");
            }
            foreach (var pair in m_innerTypeDic)
            {
                type = type.Replace(pair.Key, pair.Value.Name);
            }
            return type;
        }

        private static string GetRealTypeName(string type, bool isKey)
        {
            if (isKey && (type == "string" || type == "String"))
            {
                return "string";//key不用支持MString
            }
            foreach (var pair in m_worpTypeNameDic)
            {
                type = type.Replace(pair.Key, pair.Value);
            }
            return type;
        }


        /// <summary>
        /// 获取用户自己定义的转换方法名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GetUConverKey(string type)
        {
            var typeInstance = System.Type.GetType(type);
            if (typeInstance == null)
            {
                typeInstance = System.Type.GetType("System." + type);
            }
            if (typeInstance != null && typeof(System.IConvertible).IsAssignableFrom(typeInstance))
            {
                return "Parse<" + typeInstance.Name + ">";
            }
            var typeName = TypeReadableName(type);
            return "To" + typeName;
        }

        public static System.Type FindTypeInAllAssemble(string typename)
        {
            var allAssemble = System.AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < allAssemble.Length; i++)
            {
                System.Type type = allAssemble[i].GetType(typename);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }

        //打开并选中目录
        public static void OpenSelectFolder(string folder)
        {
            if (!System.IO.Directory.Exists(folder))
                return;
#if UNITY_EDITOR_WIN
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
            psi.Arguments = " /e,/root," + System.IO.Path.GetFullPath(folder);
            var thread = new System.Threading.Thread(() =>
            {
                System.Diagnostics.Process.Start(psi);
            });
            thread.Start();
#endif
        }
    }

    public static class TableDataExtend
    {
        public static List<string> Descs(this TableData tableData)
        {
            if (tableData.Rows.Count < 1)
                tableData.Rows.Add(new List<string>());
            return tableData.Rows[0];
        }

        public static List<string> Fields(this TableData tableData)
        {
            if (tableData.Rows.Count < 1)
                tableData.Rows.Add(new List<string>());
            if (tableData.Rows.Count < 2)
                tableData.Rows.Add(new List<string>());
            return tableData.Rows[1];
        }

        public static List<string> Types(this TableData tableData)
        {
            if (tableData.Rows.Count < 1)
                tableData.Rows.Add(new List<string>());
            if (tableData.Rows.Count < 2)
                tableData.Rows.Add(new List<string>());
            if (tableData.Rows.Count < 3)
                tableData.Rows.Add(new List<string>());
            return tableData.Rows[2];
        }

    }
}
