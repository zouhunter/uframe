/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-01                                                                   *
*  功能:                                                                              *
*   - 网络下载小助手                                                                  *
*//************************************************************************************/

using System;
using UnityEngine;
using System.Collections;

namespace UFrame
{
    [AddComponentMenu("UFrame/Component/UrlDownLoadBehaviour")]
    public class UrlDownLoadBehaviour : MonoBehaviour
    {
        [SerializeField]
        protected int m_timeOut = 60;//s
        protected float m_timer = 0;

        protected virtual void Awake()
        {
            Debug.Log("UrlDownLander Awake!");
        }

        protected virtual void OnDestroy()
        {
            Debug.Log("UrlDownLander OnDestroy!");
        }

        protected virtual void Update()
        {
            if (m_timeOut == 0)
                return;

            m_timer += Time.deltaTime;
            if(m_timer > m_timeOut)
            {
                Destroy(gameObject);
            }
        }

        public virtual void SetTimeOut(int timeOut)
        {
            m_timeOut = timeOut;
        }

        public virtual void Downlond(string url, Action<byte[]> callback)
        {
            m_timer = 0;
            StartCoroutine(DownlondAsync(url, callback));
        }

        protected IEnumerator DownlondAsync(string url, Action<byte[]> callback)
        {
            using (UnityEngine.Networking.UnityWebRequest uwq = UnityEngine.Networking.UnityWebRequest.Get(url))
            {
                uwq.SendWebRequest();
                yield return uwq;
                if (uwq.isDone)
                {
                    try
                    {
                        if (uwq.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                        {
                            callback(uwq.downloadHandler.data);
                        }
                        else
                        {
                            callback(null);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                    if (!string.IsNullOrEmpty(uwq.error))
                    {
                        Debug.LogError(uwq.error);
                    }
                }
            }
        }

    }
}