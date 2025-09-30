using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using UFrame.SharpZipLib.Zip;
using UFrame.SharpZipLib.Checksums;
using System.Collections.Generic;

namespace UFrame.SharpZipLib
{
    /// <summary>
    /// This is a C# .Net Standard library which enables zipping and unzipping strings in memory.
    /// Zip and Unzip in memory using System.IO.Compression.
    /// </summary>
    /// <remarks>
    /// Please check if System.IO.Compression and Linq are included.
    /// </remarks>
    public static class ZipHelper
    {
        public static int zipLevel = 6;
        #region 压缩数据
        /// <summary>
        /// Zips a string into a zipped byte array.
        /// </summary>
        /// <param name="textToZip">The text to be zipped.</param>
        /// <param name="zippedFileName">Optional alternative filename</param>
        /// <param name="compressionLevel">Compressionlevel (default optimal)</param>
        /// <returns>byte[] representing a zipped stream (Encoding.Default)</returns>
        public static byte[] ZipText(string textToZip, string zippedFileName = "zipped.txt", CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var demoFile = zipArchive.CreateEntry(zippedFileName, compressionLevel);

                    using (var entryStream = demoFile.Open())
                    {
                        using (var streamWriter = new StreamWriter(entryStream, Encoding.Default))
                        {
                            streamWriter.Write(textToZip);
                        }
                    }
                }

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Zips a byte array into a zipped byte array.
        /// </summary>
        /// <param name="byteArrayToZip">The byte array to be zipped.</param>
        /// <param name="zippedFileName">Optional alternative filename</param>
        /// <param name="compressionLevel">Compressionlevel (default optimal)</param>
        /// <returns>byte[] representing a zipped stream</returns>
        public static byte[] ZipBytes(byte[] byteArrayToZip, string zippedFileName = "zipped.txt", CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var demoFile = zipArchive.CreateEntry(zippedFileName, compressionLevel);

                    using (var entryStream = demoFile.Open())
                    {
                        using (var streamWriter = new StreamWriter(entryStream)) // encoding is not relevant
                        {
                            streamWriter.BaseStream.Write(byteArrayToZip, 0, byteArrayToZip.Length);
                        }
                    }
                }

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Unzip a zipped byte array into a string.
        /// </summary>
        /// <param name="zippedBuffer">The byte array to be unzipped</param>
        /// <returns>string representing the original stream (Encoding.Default)</returns>
        public static string UnZipBytesToText(byte[] zippedBuffer)
        {
            using (var zippedStream = new MemoryStream(zippedBuffer))
            {
                using (var archive = new ZipArchive(zippedStream))
                {
                    ZipArchiveEntry entry = null;
                    var enuerator = archive.Entries.GetEnumerator();
                    while (enuerator.MoveNext())
                    {
                        entry = enuerator.Current;
                        break;
                    }
                    if (entry != null)
                    {
                        using (var unzippedEntryStream = entry.Open())
                        {
                            using (var ms = new MemoryStream())
                            {
                                unzippedEntryStream.CopyTo(ms);
                                var unzippedArray = ms.ToArray();

                                return Encoding.Default.GetString(unzippedArray);
                            }
                        }
                    }

                    return null;
                }
            }
        }

        /// <summary>
        /// Unzip a zipped byte array into a string.
        /// </summary>
        /// <param name="zippedBuffer">The byte array to be unzipped</param>
        /// <returns>string representing the original byte array stream</returns>
        public unsafe static byte[] UnZipByteArray(byte[] zippedBuffer)
        {
            using (var zippedStream = new MemoryStream(zippedBuffer,false))
            {
                using (var archive = new ZipArchive(zippedStream))
                {
                    ZipArchiveEntry entry = null;
                    var enuerator = archive.Entries.GetEnumerator();
                    while (enuerator.MoveNext())
                    {
                        entry = enuerator.Current;
                        break;
                    }
                    if (entry != null)
                    {
                        using (var unzippedEntryStream = entry.Open())
                        {
                            using (var ms = new MemoryStream())
                            {
                                unzippedEntryStream.CopyTo(ms);
                                var unzippedArray = ms.ToArray();

                                return unzippedArray;
                            }
                        }
                    }

                    return null;
                }
            }
        }
        #endregion

        #region 压缩文件
        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="destinationZipFilePath"></param>
        public static void Zip(string sourceFilePath, string destinationZipFilePath)
        {
            if (sourceFilePath[sourceFilePath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                sourceFilePath += System.IO.Path.DirectorySeparatorChar;
            using(var fileStream = File.Create(destinationZipFilePath))
            {
                using (ZipOutputStream zipStream = new ZipOutputStream(fileStream))
                {
                    zipStream.SetLevel(zipLevel);  // 压缩级别 0-9
                    var subFiles = Directory.GetFileSystemEntries(sourceFilePath);
                    Zip(sourceFilePath, subFiles, zipStream);
                    zipStream.Finish();
                    zipStream.Close();
                }
            }
        }
        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="sourceFolderPath"></param>
        /// <param name="destinationZipFilePath"></param>
        public static void Zip(string sourceFolderPath,string[] filesPath, string destinationZipFilePath)
        {
            if (sourceFolderPath[sourceFolderPath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                sourceFolderPath += System.IO.Path.DirectorySeparatorChar;

            using(var fileStream = File.Create(destinationZipFilePath))
            {
                using (ZipOutputStream zipStream = new ZipOutputStream(fileStream))
                {
                    zipStream.SetLevel(zipLevel);  // 压缩级别 0-9
                    Zip(sourceFolderPath, filesPath, zipStream);
                    zipStream.Finish();
                    zipStream.Close();
                }
            }
           
        }
        /// <summary>
        /// 递归压缩文件
        /// </summary>
        /// <param name="sourceFilePath">待压缩的文件或文件夹路径</param>
        /// <param name="zipStream">打包结果的zip文件路径（类似 D:\WorkSpace\a.zip）,全路径包括文件名和.zip扩展名
        /// <param name="staticFile"></param>
        private static void Zip(string rootPath, string[] filesArray, ZipOutputStream zipStream)
        {
            Crc32 crc = new Crc32();
            rootPath = rootPath.Replace("\\", "/");
            foreach (string file in filesArray)
            {
                if (Directory.Exists(file))                     //如果当前是文件夹，递归
                {
                    var subFiles = Directory.GetFileSystemEntries(file);
                    Zip(rootPath, subFiles, zipStream);
                    continue;
                }

                using(FileStream fileStream = File.OpenRead(file))
                {
                    byte[] buffer = new byte[fileStream.Length];
                    fileStream.Read(buffer, 0, buffer.Length);
                    string tempFile = file.Replace("\\","/").Replace(rootPath, string.Empty);
                    ZipEntry entry = new ZipEntry(tempFile);
                    entry.DateTime = DateTime.Now;
                    entry.Size = fileStream.Length;
                    entry.IsCrypted = true;
                    entry.IsEncrypted = true;
                    fileStream.Close();
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    zipStream.PutNextEntry(entry);
                    zipStream.Write(buffer, 0, buffer.Length);
                }
            }
        }
        #endregion

        #region 解压缩
        public unsafe static bool UnZip(byte[] fileData, string zipedFolder)
        {
            zipedFolder = System.IO.Path.GetFullPath(zipedFolder);

            bool result = true;
            ZipEntry ent = null;
            string fileName;

            if (!Directory.Exists(zipedFolder))
                Directory.CreateDirectory(zipedFolder);

            try
            {
                fixed (byte* pB = &fileData[0])
                {
                    using (var fileStream = new UnmanagedMemoryStream(pB, fileData.Length))
                    {
                        byte[] data = new byte[512];
                        using (var zipStream = new ZipInputStream(fileStream, 1024))
                        {
                            while ((ent = zipStream.GetNextEntry()) != null)
                            {
                                if (!string.IsNullOrEmpty(ent.Name))
                                {
                                    fileName = Path.Combine(zipedFolder, ent.Name).Replace('\\', '/');

                                    if (fileName.EndsWith("/"))
                                    {
                                        Directory.CreateDirectory(fileName);
                                        continue;
                                    }

                                    var dir = System.IO.Path.GetDirectoryName(fileName);
                                    if (!System.IO.Directory.Exists(dir))
                                        System.IO.Directory.CreateDirectory(dir);

                                    using (var fs = File.Create(fileName))
                                    {
                                        while (true)
                                        {
                                            var size = zipStream.Read(data, 0, data.Length);
                                            if (size > 0)
                                                fs.Write(data, 0, size);
                                            else
                                                break;
                                        }
                                        fs.Close();
                                    }
                                }
                            }
                        }

                    }
                }
            }
            catch(Exception e)
            {
                result = false;
                UnityEngine.Debug.LogException(e);
            }
            finally
            {
                ent = null;
                GC.Collect();
                GC.Collect(1);
            }
            return result;
        }

        public unsafe static bool UnZip(byte[] fileData, Dictionary<string,byte[]> fileDic)
        {
            bool result = true;
            ZipEntry ent = null;
            string fileName;

            try
            {
                fixed (byte* pB = &fileData[0])
                {
                    using (var fileStream = new UnmanagedMemoryStream(pB, fileData.Length))
                    {
                        byte[] data = new byte[512];
                        using (var zipStream = new ZipInputStream(fileStream, 1024))
                        {
                            var fs = new MemoryStream();
                            while ((ent = zipStream.GetNextEntry()) != null)
                            {
                                if (!string.IsNullOrEmpty(ent.Name))
                                {
                                    fileName = ent.Name.Replace('\\', '/');
                                    if (fileName.EndsWith("/"))
                                    {
                                        continue;
                                    }
                                    fs.Seek(0, SeekOrigin.Begin);
                                    fs.SetLength(0);
                                    while (true)
                                    {
                                        var size = zipStream.Read(data, 0, data.Length);
                                        if (size > 0)
                                            fs.Write(data, 0, size);
                                        else
                                            break;
                                    }
                                    fileDic[fileName] = fs.ToArray();
                                }
                            }
                            fs.Close();
                            fs.Dispose();
                        }

                    }
                }
            }
            catch (Exception e)
            {
                result = false;
                UnityEngine.Debug.LogException(e);
            }
            finally
            {
                ent = null;
                GC.Collect();
                GC.Collect(1);
            }
            return result;
        }


        public unsafe static bool UnZipToStream(byte[] fileData, string path, Stream stream)
        {
            bool result = true;
            ZipEntry ent = null;
            string fileName;

            try
            {
                fixed (byte* pB = &fileData[0])
                {
                    using (var fileStream = new UnmanagedMemoryStream(pB, fileData.Length))
                    {
                        byte[] data = new byte[512];
                        using (var zipStream = new ZipInputStream(fileStream, 1024))
                        {
                            while ((ent = zipStream.GetNextEntry()) != null)
                            {
                                if (!string.IsNullOrEmpty(ent.Name))
                                {
                                    fileName = ent.Name.Replace('\\', '/');
                                    if (fileName.EndsWith("/"))
                                    {
                                        continue;
                                    }

                                    if(path == fileName)
                                    {
                                        while (true)
                                        {
                                            var size = zipStream.Read(data, 0, data.Length);
                                            if (size > 0)
                                                stream.Write(data, 0, size);
                                            else
                                                break;
                                        }
                                        break;
                                    }
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception e)
            {
                result = false;
                UnityEngine.Debug.LogException(e);
            }
            finally
            {
                ent = null;
                GC.Collect();
                GC.Collect(1);
            }
            return result;
        }

        public static bool UnZip(string fileToUnZip, string zipedFolder)
        {
            fileToUnZip = System.IO.Path.GetFullPath(fileToUnZip);
            zipedFolder = System.IO.Path.GetFullPath(zipedFolder);

            bool result = true;
            ZipInputStream zipStream = null;
            ZipEntry ent = null;
            string fileName;

            if (!File.Exists(fileToUnZip))
                return false;

            if (!Directory.Exists(zipedFolder))
                Directory.CreateDirectory(zipedFolder);

            try
            {
                using (FileStream fileStream = File.OpenRead(fileToUnZip))
                {
                    byte[] data = new byte[512];
                    zipStream = new ZipInputStream(fileStream, 4096);
                    while ((ent = zipStream.GetNextEntry()) != null)
                    {
                        if (!string.IsNullOrEmpty(ent.Name))
                        {
                            fileName = Path.Combine(zipedFolder, ent.Name);
                            fileName = fileName.Replace('\\', '/');

                            if (fileName.EndsWith("/"))
                            {
                                Directory.CreateDirectory(fileName);
                                continue;
                            }

                            var dir = System.IO.Path.GetDirectoryName(fileName);
                            if (!System.IO.Directory.Exists(dir))
                                System.IO.Directory.CreateDirectory(dir);

                            using (var fs = File.Create(fileName))
                            {
                                while (true)
                                {
                                    var size = zipStream.Read(data, 0, data.Length);
                                    if (size > 0)
                                        fs.Write(data, 0, size);
                                    else
                                        break;
                                }
                                fs.Close();
                            }
                                
                        }
                    }
                }
            }
            catch (Exception e)
            {
                result = false;
                UnityEngine.Debug.LogException(e);
            }
            finally
            {
                if (zipStream != null)
                {
                    zipStream.Close();
                    zipStream.Dispose();
                }
                if (ent != null)
                {
                    ent = null;
                }
                GC.Collect();
                GC.Collect(1);
            }
            return result;
        }
        #endregion
    }
}