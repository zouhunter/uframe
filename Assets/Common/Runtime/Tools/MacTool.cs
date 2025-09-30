using UnityEngine;
using System.Collections;
using System.Diagnostics;

namespace UFrame
{
    public class MacTool
    {
        public static void Bash(string cmd, string workingDirectory, bool startTerminal = false)
        {

            ProcessStartInfo startInfo = new ProcessStartInfo("/bin/bash")
            {
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };

            Process process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            string code = "";
            if (startTerminal)
            {
                code = "osascript -e 'tell application \"Terminal\" to do script \"" +
                       "" + cmd + "\"'";//in selected tab of the front window
            }
            else
            {
                code = cmd;
            }

            process.StandardInput.WriteLine(code);
            process.StandardInput.WriteLine("exit");
            process.StandardInput.Flush();

            string line = process.StandardOutput.ReadLine();

            while (line != null)
            {
                line = process.StandardOutput.ReadLine();
            }
            process.WaitForExit();
        }
    }
}