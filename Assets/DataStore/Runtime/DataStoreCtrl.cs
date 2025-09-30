using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UFrame;

namespace UFrame.DataStore
{
    public class DataStoreCtrl 
    {
        public bool Valid { get; protected set; }
        protected const string FOLDER_NAME = "LocalStorage";
        protected BinaryStore m_binaryStore;
        protected Dictionary<string, byte[]> m_dataCatch = new Dictionary<string, byte[]>();

        public virtual void OnRegist()
        {
            Valid = true;
            var encryptKey = SystemInfo.deviceUniqueIdentifier + Application.productName;
            m_binaryStore = new BinaryStore(FOLDER_NAME, encryptKey);
        }

        public virtual void OnUnRegist()
        {
            Valid = false;
            m_dataCatch.Clear();
            m_binaryStore = null;
        }

        public virtual void ClearAllData()
        {
            var dataPathDir = System.IO.Path.Combine(Application.persistentDataPath, FOLDER_NAME);
            if (System.IO.Directory.Exists(dataPathDir))
            {
                System.IO.Directory.Delete(dataPathDir, true);
            }
            m_dataCatch.Clear();
        }

        public virtual void SetUserDir(string userid)
        {
            if (Valid)
            {
                m_binaryStore.ChangeDir(string.Format("{0}/{1}", FOLDER_NAME, userid));
            }
            else
            {
                Debug.LogError("InValid,Not Reg!");
            }
        }

        public virtual void SetEncrypt(string encrypt)
        {
            if (Valid)
            {
                m_binaryStore.SetEncrypt(encrypt);
            }
            else
            {
                Debug.LogError("InValid,Not Reg!");
            }
        }

        public virtual void SetString(string dataId, string value)
        {
            if (Valid)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(value);
                    SetData(dataId, bytes);
                }
                else
                {
                    SetData(dataId, null);
                }
            }
            else
            {
                Debug.LogError("InValid,Not Reg!");
            }
        }

        public virtual string GetString(string dataId)
        {
            if (Valid)
            {
                var catchData = GetData(dataId);
                if (catchData != null)
                {
                    return System.Text.Encoding.UTF8.GetString(catchData);
                }
            }
            else
            {
                Debug.LogError("InValid,Not Reg!");
            }
            return null;
        }


        public virtual void SetData(string dataId, byte[] data)
        {
            if (Valid)
            {
                if (data != null && m_dataCatch.TryGetValue(dataId, out var catchData) && catchData != null)
                {
                    if (data.Length == catchData.Length)
                    {
                        bool equalAll = true;
                        for (int i = 0; i < data.Length; i++)
                        {
                            if (catchData[i] != data[i])
                            {
                                equalAll = false;
                            }
                        }
                        if (equalAll)
                        {
                            return;
                        }
                    }
                }

                if (data == null)
                    m_dataCatch.Remove(dataId);
                else
                    m_dataCatch[dataId] = data;
                m_binaryStore.SaveTo(dataId, data);
            }
            else
            {
                Debug.LogError("InValid,Not Reg!");
            }
        }

        public virtual byte[] GetData(string dataId)
        {
            if (Valid)
            {
                if (!m_dataCatch.TryGetValue(dataId, out var catchData))
                {
                    var resultBytes = m_binaryStore.LoadFrom(dataId);
                    if (resultBytes != null)
                    {
                        m_dataCatch[dataId] = resultBytes;
                        catchData = resultBytes;
                    }
                }
                return catchData;
            }
            else
            {
                Debug.LogError("InValid,Not Reg!");
            }
            return null;
        }
    }
}