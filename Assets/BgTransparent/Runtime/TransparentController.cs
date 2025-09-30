using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace UFrame.BgTransparent
{
    public class TransparentController
    {
        private Material m_defaultMat;
        private HashSet<string> m_transparentItems = new HashSet<string>();
        private Dictionary<string, ShowGroupItem> m_operates = new Dictionary<string, ShowGroupItem>();

        //设置默认
        public void SetDefaultWorpMat(Material mat)
        {
            m_defaultMat = mat;
        }

        //注册组
        public void RegistGroupItem(string key, GameObject[] groupGameObjs, Material worpMat,string colorName, string texName, string worpColorName, string worpTexName)
        {
            ShowGroupItem groupItem = new ShowGroupItem();
            groupItem.key = key;
            groupItem.worpmat = worpMat;
            groupItem.worp = groupGameObjs;
            if(groupItem.worpmat == null)
            {
                groupItem.worpmat = m_defaultMat;
            }
            Debug.Assert(groupItem.worpmat,"worp material is empty !");
            var renderList = new List<Renderer>();
            foreach (var go in groupItem.worp)
            {
                var rds = go.GetComponentsInChildren<Renderer>(true);
                if (rds != null)
                {
                    renderList.AddRange(rds);
                }
            }
            groupItem.renderOperate = new RendererOperation(groupItem.worpmat, renderList.ToArray(), colorName, texName, worpColorName, worpTexName);
            m_operates[groupItem.key] = groupItem;
        }

        /// 透明指定模块
        public void Transparent(string key, bool showOthers = false)
        {
            if (!ContainModule(key))
                return;

            if (showOthers)
            {
                ReverTransparented();
            }
            TransparentGroupItem(key);
        }

        /// 透明除指定模块所有模块
        public void TranparentExclude(string key)
        {
            if (!ContainModule(key)) 
                return;

            foreach (var pair in m_operates)
            {
                var current = pair.Key;
                if (current != key)
                {
                    TransparentGroupItem(current);
                }
                else
                {
                    RevertGroupItem(key);
                }
            }
        }
       
        /// 显示所有被透明模块
        public void ReverTransparented()
        {
            foreach (var item in m_transparentItems)
            {
                RevertGroupItem(item);
            }
            m_transparentItems.Clear();
        }

        //判断是否存在模块
        public bool ContainModule(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                return m_operates.ContainsKey(key);
            }
            return false;
        }

        protected void RevertGroupItem(string key)
        {
            if (m_operates.ContainsKey(key))
            {
                m_operates[key].renderOperate.Recovery();
            }
            if (m_transparentItems.Contains(key))
            {
                m_transparentItems.Remove(key);
            }
        }

        protected void TransparentGroupItem(string key)
        {
            if (m_operates.TryGetValue(key, out var groupItem))
            {
                groupItem.renderOperate.WorpRenderers();
                m_transparentItems.Add(key);
            }
        }
    }
}