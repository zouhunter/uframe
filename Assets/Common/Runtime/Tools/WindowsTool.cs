using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using UnityEngine;

namespace UFrame
{
    public static class WindowsTool
    {
        public const int OFN_ALLOWMULTISELECT = 0x00000200;
        public const int OFN_EXPLORER = 0x00080000;
        public const int OFN_FILEMUSTEXIST = 0x00001000;
        public const int OFN_PATHMUSTEXIST = 0x00000800;
        public const int OFN_NOCHANGEDIR = 0x00000008;
        public const int OFN_HIDEREADONLY = 0x4;
        public const int OFN_FORCESHOWHIDDEN = 0x10000000;

        public const uint BIF_RETURNONLYFSDIRS = 0x0001;  // For finding a folder to start document searching
        public const uint BIF_DONTGOBELOWDOMAIN = 0x0002;  // For starting the Find Computer
        public const uint BIF_STATUSTEXT = 0x0004;  // Top of the dialog has 2 lines of text for BROWSEINFO.lpszTitle and one line if
                                                    // this flag is set.  Passing the message BFFM_SETSTATUSTEXTA to the hwnd can set the
                                                    // rest of the text.  This is not used with BIF_USENEWUI and BROWSEINFO.lpszTitle gets
                                                    // all three lines of text.
        public const uint BIF_RETURNFSANCESTORS = 0x0008;
        public const uint BIF_EDITBOX = 0x0010;   // Add an editbox to the dialog
        public const uint BIF_VALIDATE = 0x0020;   // insist on valid result (or CANCEL)

        public const uint BIF_NEWDIALOGSTYLE = 0x0040;   // Use the new dialog layout with the ability to resize
                                                         // Caller needs to call OleInitialize() before using this API
        public const uint BIF_USENEWUI = 0x0040 + 0x0010; //(BIF_NEWDIALOGSTYLE | BIF_EDITBOX);

        public const uint BIF_BROWSEINCLUDEURLS = 0x0080;   // Allow URLs to be displayed or entered. (Requires BIF_USENEWUI)
        public const uint BIF_UAHINT = 0x0100;   // Add a UA hint to the dialog, in place of the edit box. May not be combined with BIF_EDITBOX
        public const uint BIF_NONEWFOLDERBUTTON = 0x0200;   // Do not add the "New Folder" button to the dialog.  Only applicable with BIF_NEWDIALOGSTYLE.
        public const uint BIF_NOTRANSLATETARGETS = 0x0400;  // don't traverse target as shortcut

        public const uint BIF_BROWSEFORCOMPUTER = 0x1000;  // Browsing for Computers.
        public const uint BIF_BROWSEFORPRINTER = 0x2000;// Browsing for Printers
        public const uint BIF_BROWSEINCLUDEFILES = 0x4000; // Browsing for Everything
        public const uint BIF_SHAREABLE = 0x8000;  // sharable resources displayed (remote shares, requires BIF_USENEWUI)


        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class OpenFileName
        {
            public int structSize;
            public IntPtr dlgOwner;
            public IntPtr instance;
            public String filter;
            public String customFilter;
            public int maxCustFilter;
            public int filterIndex;
            public String file;
            public int maxFile;
            public String fileTitle;
            public int maxFileTitle;
            public String initialDir;
            public String title;
            public int flags;
            public short fileOffset;
            public short fileExtension;
            public String defExt;
            public IntPtr custData;
            public IntPtr hook;
            public String templateName;
            public IntPtr reservedPtr;
            public int reservedInt;
            public int flagsEx;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private class OpenFileNameMuti
        {
            public int structSize = 0;
            public IntPtr dlgOwner = IntPtr.Zero;
            public IntPtr instance = IntPtr.Zero;
            public string filter;
            public string customFilter;
            public int maxCustFilter = 0;
            public int filterIndex = 0;
            public IntPtr file;
            public int maxFile = 0;
            public string fileTitle;
            public int maxFileTitle = 0;
            public string initialDir;
            public string title;
            public int flags = 0;
            public short fileOffset = 0;
            public short fileExtension = 0;
            public string defExt;
            public IntPtr custData = IntPtr.Zero;
            public IntPtr hook = IntPtr.Zero;
            public string templateName;
            public IntPtr reservedPtr = IntPtr.Zero;
            public int reservedInt = 0;
            public int flagsEx = 0;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class OpenDialogDir
        {
            public IntPtr hwndOwner = IntPtr.Zero;
            public IntPtr pidlRoot = IntPtr.Zero;
            public String pszDisplayName = null;
            public String lpszTitle = null;
            public UInt32 ulFlags = 0;
            public IntPtr lpfn = IntPtr.Zero;
            public IntPtr lParam = IntPtr.Zero;
            public int iImage = 0;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct CHOOSECOLOR
        {
            public Int32 lStructSize;//指定结构的长度(字节)
            public IntPtr hwndOwner;//拥有对话框的窗口的句柄。该成员可以是任意有效的窗口句柄，或在对话框没有所有者时，可为NULL
            public IntPtr hInstance;//如果Flag成员设置了CC_ENABLETEMPLATEHANDLE标识符时，该成员是一个包含了对话框模板的内存对象的句柄。如果 CC_ENABLETEMPLATE 标识符被设置时，该成员是一个包含了对话框的模块句柄。如果上述两个标识符都未被设置，则该成员被忽略。
            public Int32 rgbResult;//如果该成员为0或CC_RGBINIT未被设置，初始颜色是黑色
            public IntPtr lpCustColors;//指向一个包含16个值的数组，该数组包含了对话框中自定义颜色的红、绿、蓝(RGB)值。
            public Int32 Flags;//一个可以让你初始化颜色对话框的位集
            public long lCustData; //指定应用程序自定义的数据，该数据会被系统发送给钩子程序
            public UIntPtr lpfnHook;//指向CCHookProc钩子程序的指针，该钩子可以处理发送给对话框的消息。该成员只在CC_ENABLEHOOK标识被设定的情况下才可用，否则该成员会被忽略。
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpTemplateName;//指向一个NULL结尾的字符串，该字符串是对话框模板资源的名字。
        }

        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetOpenFileName([In, Out] OpenFileNameMuti ofn);

        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);

        [DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SHBrowseForFolder([In, Out] OpenDialogDir ofn);

        [DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool SHGetPathFromIDList([In] IntPtr pidl, [In, Out] char[] fileName);

        [DllImport("comdlg32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChooseColorA(ref CHOOSECOLOR pChoosecolor);//对应的win32API
        [DllImport("comdlg32.dll")]
        public extern static int CommDlgExtendedError();
        /*
        *1. 删除文件的时候确认操作：SHFILEOPSTRUCT里面的成员fFlags默认情况下会有确认操作的提示框，如果您不想要提示   框，可以 shf.fFlags =  FOF_NOCONFIRMATION;
        *2. 删除到回收站和永久删除：shf.fFlags =  FOF_ALLOWUNDO，顾名思义，允许撤销的话将删除到回收站，否则将永久删   除。
        *3. 操作文件或者文件夹的时候显示进度条：shf.fFlags =  FOF_SIMPLEPROGRESS
        *4. 对于要删除的文件或者文件夹使用“\0\0”结束，即"D:\\New Text Document.txt\0\0"
        *5. 如果要操作多个文件或者文件夹 ，不同的路径用“\0”间隔，即"D:\\New Text Document.txt\0D:\\New Folder\0\0"
        */
        public static class FileEx
        {
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]

            public struct SHFILEOPSTRUCT
            {

                public IntPtr hwnd;

                [MarshalAs(UnmanagedType.U4)]
                public int wFunc;

                public string pFrom;

                public string pTo;

                public short fFlags;

                [MarshalAs(UnmanagedType.Bool)]
                public bool fAnyOperationsAborted;

                public IntPtr hNameMappings;

                public string lpszProgressTitle;

            }

            [DllImport("shell32.dll", CharSet = CharSet.Auto)]

            static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

            const int FO_RENAME = 4;

            const int FO_DELETE = 3;

            const int FO_COPY = 2;

            const int FO_MOVE = 1;

            const int FOF_ALLOWUNDO = 0x40;

            const int FOF_NOCONFIRMATION = 0x10;     //Don't     prompt     the     user.;       

            const int FOF_SIMPLEPROGRESS = 0x100;

            public static void SendToRecyclyBin(string path, bool prompt = true)
            {

                SHFILEOPSTRUCT shf = new SHFILEOPSTRUCT();

                shf.wFunc = FO_DELETE;

                shf.fFlags = (short)((prompt ? FOF_ALLOWUNDO : FOF_NOCONFIRMATION) | FOF_SIMPLEPROGRESS);

                shf.pFrom = path;

                //返回0表示操作成功
                SHFileOperation(ref shf);

            }

            public static bool Copy(string from, string to, bool prompt = true)
            {
                SHFILEOPSTRUCT shf = new SHFILEOPSTRUCT();

                shf.wFunc = FO_COPY;

                shf.fFlags = (short)((prompt ? FOF_ALLOWUNDO : FOF_NOCONFIRMATION) | FOF_SIMPLEPROGRESS);

                shf.pFrom = from;

                shf.pTo = to;

                return SHFileOperation(ref shf) == 0;

            }

            public static bool Move(string from, string to, bool prompt = true)
            {

                SHFILEOPSTRUCT shf = new SHFILEOPSTRUCT();
                shf.wFunc = FO_MOVE;

                shf.fFlags = (short)((prompt ? FOF_ALLOWUNDO : FOF_NOCONFIRMATION) | FOF_SIMPLEPROGRESS);

                shf.pFrom = from;

                shf.pTo = to;

                return SHFileOperation(ref shf) == 0;

            }

            public static bool RENAME(string from, string to, bool prompt = true)
            {

                SHFILEOPSTRUCT shf = new SHFILEOPSTRUCT();

                shf.wFunc = FO_RENAME;

                shf.fFlags = FOF_ALLOWUNDO | FOF_SIMPLEPROGRESS;

                shf.pFrom = from;

                shf.pTo = to;
                return SHFileOperation(ref shf) == 0;

            }
        }

        /// 打开文件夹
        public static System.Diagnostics.Process OpenFolder(string path)
        {
            if (!System.IO.Directory.Exists(path)) path = "C:/";
            return System.Diagnostics.Process.Start(path, "/e");
        }

        //打开文件
        public static System.Diagnostics.Process OpenFile(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                return System.Diagnostics.Process.Start(filePath);
            }
            return null;
        }

        /// 源文件夹中的对象拷贝到目标文件夹
        public static void CopyFilesTo(string oringalFolder, string folder)
        {
            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }
            List<string> childs = new List<string>();
            var files = System.IO.Directory.GetFiles(oringalFolder);
            var dirs = System.IO.Directory.GetDirectories(oringalFolder);
            childs.AddRange(files);
            childs.AddRange(dirs);
            if (childs.Count > 0)
            {
                string oringal_files = string.Join("\0", childs.ToArray()) + "\0\0";
                FileEx.Copy(oringal_files, folder + "\0\0");
            }
        }

        /// 目标路径和原始路径文件夹名一样，也有可能是文件
        public static void Copy(string srcPath, string targetPath, bool propmt = true)
        {
            if (!System.IO.Directory.Exists(srcPath) && !System.IO.File.Exists(srcPath)) return;
            if (string.IsNullOrEmpty(targetPath)) return;

            var folder = System.IO.Path.GetDirectoryName(targetPath);

            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }

            FileEx.Copy(srcPath + "\0\0", folder + "\0\0", propmt);
        }

        //拷贝文件夹
        public static void CopyDir(string srcPath, string targetPath)
        {
            try
            {
                // 检查目标目录是否以目录分割字符结束如果不是则添加
                if (targetPath[targetPath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                {
                    targetPath += System.IO.Path.DirectorySeparatorChar;
                }
                // 判断目标目录是否存在如果不存在则新建
                if (!System.IO.Directory.Exists(targetPath))
                {
                    System.IO.Directory.CreateDirectory(targetPath);
                }
                // 得到源目录的文件列表，该里面是包含文件以及目录路径的一个数组
                // 如果你指向copy目标文件下面的文件而不包含目录请使用下面的方法
                // string[] fileList = Directory.GetFiles（srcPath）；
                string[] fileList = System.IO.Directory.GetFileSystemEntries(srcPath);
                // 遍历所有的文件和目录
                foreach (string file in fileList)
                {
                    // 先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
                    if (System.IO.Directory.Exists(file))
                    {
                        CopyDir(file, targetPath + System.IO.Path.GetFileName(file));
                    }
                    // 否则直接Copy文件
                    else
                    {
                        System.IO.File.Copy(file, targetPath + System.IO.Path.GetFileName(file), true);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //拷贝文件
        public static void CopyFile(string currentPath, string targetPath)
        {
            if (System.IO.File.Exists(currentPath))
            {
                var dir = System.IO.Path.GetDirectoryName(targetPath);
                if (!System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }
                System.IO.File.Copy(currentPath, targetPath, true);
            }
        }

        /// 打开文件夹并选中文件
        public static void OpenFolderAndSelectFile(string fileName)
        {
            if (!System.IO.File.Exists(fileName)) return;

            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
            psi.Arguments = " /select," + System.IO.Path.GetFullPath(fileName);
            System.Diagnostics.Process.Start(psi);
        }

        //打开并选中目录
        public static void OpenSelectFolder(string folder)
        {
            if (!System.IO.Directory.Exists(folder)) return;

            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
            psi.Arguments = " /e,/root," + System.IO.Path.GetFullPath(folder);
            var thread = new System.Threading.Thread(() =>
            {
                System.Diagnostics.Process.Start(psi);
            });
            thread.Start();
        }

        /// 文件选择目录
        public static string OpenFilePath(string title, string ext, string initialDir = null)
        {
            OpenFileName pth = new OpenFileName();
            pth.structSize = Marshal.SizeOf(pth);
            pth.filter = "All files (*.*)|*.*";
            pth.file = new string(new char[256]);
            pth.maxFile = pth.file.Length;
            pth.fileTitle = new string(new char[64]);
            pth.maxFileTitle = pth.fileTitle.Length;
            pth.initialDir = (initialDir ?? Application.dataPath).Replace("/", "\\"); //默认路径
            pth.title = title;
            pth.defExt = ext;
            pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
            if (GetOpenFileName(pth))
            {
                return pth.file; //选择的文件路径;  
            }
            return null;
        }

        //文件选择窗口
        public static string[] OpenFilesPath(string dialogTitle, string startPath = null, string filter = "All files (*.*)|*.*", bool showHidden = false)
        {
            var currDir = System.Environment.CurrentDirectory;

            string[] selectedFiles = null;

            const int MAX_FILE_LENGTH = 2048;

            OpenFileNameMuti ofn = new OpenFileNameMuti();

            ofn.structSize = Marshal.SizeOf(ofn);
            ofn.filter = filter.Replace("|", "\0") + "\0";
            ofn.fileTitle = new String(new char[MAX_FILE_LENGTH]);
            ofn.maxFileTitle = ofn.fileTitle.Length;
            ofn.initialDir = startPath;
            ofn.title = dialogTitle;
            ofn.flags = OFN_HIDEREADONLY | OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_ALLOWMULTISELECT;

            // Create buffer for file names
            ofn.file = Marshal.AllocHGlobal(MAX_FILE_LENGTH * Marshal.SystemDefaultCharSize);
            ofn.maxFile = MAX_FILE_LENGTH;

            // Initialize buffer with NULL bytes
            for (int i = 0; i < MAX_FILE_LENGTH * Marshal.SystemDefaultCharSize; i++)
            {
                Marshal.WriteByte(ofn.file, i, 0);
            }

            if (showHidden)
            {
                ofn.flags |= (int)OFN_FORCESHOWHIDDEN;
            }

            if (GetOpenFileName(ofn))
            {
                List<string> selectedFilesList = new List<string>();

                IntPtr filePointer = ofn.file;
                long pointer = (long)filePointer;
                string file = Marshal.PtrToStringAuto(filePointer);

                // Retrieve file names
                while (file.Length > 0)
                {
                    selectedFilesList.Add(file);

                    pointer += file.Length * Marshal.SystemDefaultCharSize + Marshal.SystemDefaultCharSize;
                    filePointer = (IntPtr)pointer;
                    file = Marshal.PtrToStringAuto(filePointer);
                }

                if (selectedFilesList.Count == 1)
                {
                    // Only one file selected with full path
                    Marshal.FreeHGlobal(ofn.file);
                    selectedFiles = selectedFilesList.ToArray();
                }
                else
                {
                    // Multiple files selected, add directory
                    selectedFiles = new string[selectedFilesList.Count - 1];

                    for (int i = 0; i < selectedFiles.Length; i++)
                    {
                        selectedFiles[i] = selectedFilesList[0];

                        if (!selectedFiles[i].EndsWith("\\"))
                        {
                            selectedFiles[i] += "\\";
                        }

                        selectedFiles[i] += selectedFilesList[i + 1];
                    }

                    // Return selected files
                    Marshal.FreeHGlobal(ofn.file);
                }
            }
            else
            {
                // "Cancel" pressed
                Marshal.FreeHGlobal(ofn.file);
            }
            System.Environment.CurrentDirectory = currDir;
            return selectedFiles;
        }

        /// 文件保存目录
        public static string SaveFilePath(string title,string name, string ext, string initialDir = null)
        {
            var currDir = System.Environment.CurrentDirectory;
            string filePath = null;
            OpenFileName pth = new OpenFileName();
            pth.structSize = Marshal.SizeOf(pth);
            pth.filter = "All files (*.*)|*.*";
            pth.file = new string(new char[256]);
            pth.maxFile = pth.file.Length;
            pth.fileTitle = new string(new char[64]);
            pth.maxFileTitle = pth.fileTitle.Length;
            pth.initialDir = (initialDir ?? Application.dataPath).Replace("/", "\\"); //默认路径
            pth.title = title;
            pth.defExt = ext;
            pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
            pth.file = name;
            if (GetSaveFileName(pth))
            {
                filePath = pth.file; //选择的文件路径;  
            }
            System.Environment.CurrentDirectory = currDir;
            return filePath;
        }

        /// 文件夹选择窗口
        public static string OpenFolderPath(string title)
        {
            string fullDirPath = null;
            var currDir = System.Environment.CurrentDirectory;
            OpenDialogDir ofn2 = new OpenDialogDir();
            ofn2.pszDisplayName = new string(new char[1024]);     // 存放目录路径缓冲区  
            ofn2.lpszTitle = title;// 标题  
            ofn2.ulFlags = BIF_RETURNONLYFSDIRS | BIF_USENEWUI | BIF_UAHINT/* | BIF_NONEWFOLDERBUTTON*/;
            IntPtr pidlPtr = SHBrowseForFolder(ofn2);
            char[] charArray = new char[1024];
            for (int i = 0; i < charArray.Length; i++)
                charArray[i] = '\0';

            if (SHGetPathFromIDList(pidlPtr, charArray))
            {
                fullDirPath = new String(charArray);
                fullDirPath = fullDirPath.Substring(0, fullDirPath.IndexOf('\0'));
            }
            System.Environment.CurrentDirectory = currDir;
            return fullDirPath;
        }

        /// 文件选择目录
        public static string OpenFileFolderPath(string title, string initialDir = null)
        {
            var currDir = System.Environment.CurrentDirectory;
            OpenFileName pth = new OpenFileName();
            pth.filter = "Folders|*\0\0";
            var fileChars = new char[256];
            var mark = "Select Folder";
            for (int i = 0; i < fileChars.Length; i++)
                fileChars[i] = mark.Length > i ? mark[i] : '\0';
            pth.file = new string(fileChars);
            pth.maxFile = pth.file.Length;
            var titleChars = new char[100];
            for (int i = 0; i < titleChars.Length; i++)
                titleChars[i] = title.Length > i ? title[i] : '\0';
            pth.fileTitle = new string(titleChars);
            pth.maxFileTitle = pth.fileTitle.Length;
            pth.initialDir = (initialDir ?? Application.dataPath).Replace("/", "\\"); //默认路径
            pth.structSize = Marshal.SizeOf(pth);
            string resultFolder = null;
            try
            {
                if (GetOpenFileName(pth))
                {
                    resultFolder = System.IO.Path.GetDirectoryName(pth.file); //选择的文件路径;  
                    if (!System.IO.Directory.Exists(resultFolder))
                    {
                        System.IO.Directory.CreateDirectory(resultFolder);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            System.Environment.CurrentDirectory = currDir;
            //Debug.LogError(resultFolder);
            return resultFolder;
        }

        //颜色选择窗口
        public static UnityEngine.Color OpenChoiseColor()
        {
            CHOOSECOLOR choosecolor = new CHOOSECOLOR();
            choosecolor.lStructSize = Marshal.SizeOf(choosecolor);
            choosecolor.hwndOwner = IntPtr.Zero;
            choosecolor.rgbResult = 0x808080;//颜色转成int型
            choosecolor.lpCustColors = Marshal.AllocCoTaskMem(64);
            choosecolor.Flags = 0x00000001;
            UnityEngine.Color color = Color.white;
            if (ChooseColorA(ref choosecolor))
            {
                Marshal.FreeHGlobal(choosecolor.lpCustColors);
                int result = choosecolor.rgbResult;
                color.b = ((result & 0xFF0000) >> 16) / 255f;
                color.g = ((result & 0x00FF00) >> 8) / 255f;
                color.r = ((result & 0x0000FF)) / 255f;
                color.a = 1;
            }
            else
            {
                Debug.Log(CommDlgExtendedError());
            }
            return color;
        }
    }
}