using System;
using System.Diagnostics;

namespace UFrame.SubProcess
{
    /// <summary>
    /// Cmd 的摘要说明。
    /// </summary>
    public class CmdContent
    {
        private Process proc = null;
        /// <summary>
        /// 构造方法
        /// </summary>
        public CmdContent()
        {
            proc = new Process();
        }

        /// <summary>
        /// 执行CMD语句
        /// </summary>
        /// <param name="cmd">要执行的CMD命令</param>
        public string RunCmd(string cmd,string program = "cmd.exe")
        {
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = program;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            proc.StandardInput.WriteLine(cmd);
            proc.StandardInput.WriteLine("exit");
            string outStr = proc.StandardOutput.ReadToEnd();
            proc.Close();
            return outStr;
        }

        /// <summary>
        /// 执行并返回
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="onCmdLine"></param>
        public void RunCmd(string cmd, Action<string> onCmdLine, string program = "cmd.exe")
        {
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = program;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            proc.StandardInput.WriteLine(cmd);
            proc.StandardInput.WriteLine("exit");
            if (onCmdLine != null)
            {
                try
                {
                    onCmdLine(proc.StandardOutput.ReadLine());
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        onCmdLine(proc.StandardOutput.ReadLine());
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(e);
                }
            }
            proc.WaitForExit();
            proc.Close();
        }
        
        /// <summary>
        /// 打开软件并执行命令
        /// </summary>
        /// <param name="programName">软件路径加名称（.exe文件）</param>
        /// <param name="cmd">要执行的命令</param>
        public void RunProgram(string programName, string cmd = "")
        {
            Process proc = new Process();
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = programName;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            if (cmd.Length != 0)
            {
                proc.StandardInput.WriteLine(cmd);
            }
            proc.Close();
        }
    }
}