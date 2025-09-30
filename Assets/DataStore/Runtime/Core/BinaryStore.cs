using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using UnityEngine;

namespace UFrame.DataStore
{
    public class BinaryStore
    {
        public byte[] rgbK { get; private set; }
        public byte[] rgbV { get; private set; }
        public string storeDir { get; private set; }
        protected object m_lockBody = new object();
        protected bool m_encrypt = false;

        public BinaryStore(string dirname, string encrypt)
        {
            ChangeDir(dirname);
            SetEncrypt(encrypt);
        }

        //���Ŀ¼
        public void ChangeDir(string dirname)
        {
            this.storeDir = System.IO.Path.Combine(Application.persistentDataPath, dirname);
            if (!string.IsNullOrEmpty(storeDir))
            {
                System.IO.Directory.CreateDirectory(storeDir);
            }
        }

        public void SetEncrypt(string encrypt)
        {
            if (string.IsNullOrEmpty(encrypt))
            {
                this.m_encrypt = false;
            }
            else
            {
                this.m_encrypt = true;
                using (MD5 mi = MD5.Create())
                {
                    var encryptKeyBytes = Encoding.Unicode.GetBytes(encrypt);
                    var hashCodes = mi.ComputeHash(encryptKeyBytes);
                    this.rgbK = new byte[8];
                    this.rgbV = new byte[8];
                    for (int i = 0; i < hashCodes.Length; i++)
                    {
                        if (i < 8) rgbK[i] = hashCodes[i];
                        else rgbV[i - 8] = hashCodes[i];
                    }
                }
            }
        }

        //��������
        public void SaveTo(string filename, byte[] data, bool threadSave = true)
        {
            if (string.IsNullOrEmpty(filename))
                return;

            if (m_encrypt && data != null)
            {
                try
                {
                    data = EncryptionTool.DesEncrypt(data, rgbK, rgbV);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }

            }

            var fullpath = System.IO.Path.Combine(storeDir, filename);

            var saveAction = new WaitCallback((arg) =>
            {
                lock (m_lockBody)
                {
                    if (data != null)
                        System.IO.File.WriteAllBytes(fullpath, data);
                    else if(System.IO.File.Exists(fullpath))
                        System.IO.File.Delete(fullpath);
                }
            });

            if (threadSave)
            {
                ThreadPool.QueueUserWorkItem(saveAction);
            }
            else
            {
                saveAction.Invoke(null);
            }
        }

        //��������
        public byte[] LoadFrom(string filename)
        {
            byte[] data = null;
            if (string.IsNullOrEmpty(filename))
                return null;

            var fullpath = System.IO.Path.Combine(storeDir, filename);
            if (System.IO.File.Exists(fullpath))
            {
                lock (m_lockBody)
                    data = System.IO.File.ReadAllBytes(fullpath);
            }
            if (m_encrypt && data != null)
            {
                try
                {
                    data = EncryptionTool.DesDecrypt(data, rgbK, rgbV);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }
            return data;
        }
    }
}