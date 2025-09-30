/***********************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-24                                                                   *
*  版本: master_b9fdd5                                                                *
*  功能:                                                                              *
*   - 绑定辅助脚本                                                                    *
*//************************************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace UFrame.BridgeUI
{
    public class ReferenceCatchBehaiver : MonoBehaviour
    {
        public List<SerializeableReferenceItem> cacheItems = new List<SerializeableReferenceItem>();
        public void SetReferenceItems(List<ReferenceItem> items)
        {
            cacheItems = new List<SerializeableReferenceItem>();
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var cacheItem = new SerializeableReferenceItem();
                if (item.type != null)
                {
                    cacheItem.assembleName = item.type.Assembly.FullName;
                    cacheItem.typeName = item.type.FullName;
                }

                cacheItem.name = item.name;
                cacheItem.isArray = item.isArray;

                if (item.isArray)
                {
                    if (item.referenceTargets != null)
                    {
                        cacheItem.referenceInstenceIDs = new List<int>();
                        item.referenceTargets.ForEach(x =>
                        {
                            if (x)
                            {
                                cacheItem.referenceInstenceIDs.Add(x.GetInstanceID());
                            }
                        });
                    }
                    cacheItem.values = item.values;
                }
                else
                {
                    cacheItem.value = item.value;
                    if (item.referenceTarget)
                    {
                        cacheItem.refereneceInstenceID = item.referenceTarget.GetInstanceID();
                    }
                }
                cacheItems.Add(cacheItem);
            }
        }
    }

}
