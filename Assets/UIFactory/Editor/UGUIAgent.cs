// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reflection;
// using Newtonsoft.Json.Linq;
// using UnityEditor;
// using UnityEditorInternal;
// using UnityEngine;
// using UnityMcpBridge.Editor.Helpers; // For Response class
// using UnityMcpBridge.Editor; // For Response class
// using UnityMcpBridge.Editor.Tools;

// namespace UFrame.UIFactory
// {
//     /// <summary>
//     /// Handles reading and clearing Unity Editor console log entries.
//     /// Uses reflection to access internal LogEntry methods/properties.
//     /// </summary>
//     public class UGUIAgent : McpTool
//     {
//         public override string ToolName => "ugui_agent";

//         public override object HandleCommand(JObject cmd)
//         {
//             Debug.LogError(cmd["action"]);
//             return Response.Success($"successfully.");
//         }
//     }
// }
