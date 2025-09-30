//*************************************************************************************
//* 作    者： 
//* 创建时间： 2022-07-06 10:19:26
//* 描    述：

//* ************************************************************************************
using System.Collections.Generic;
using UnityEngine;
using UFrame.TableCfg;
using UFrame.Pool;

namespace UFrame.Effects
{
    public class EffectCtrl
    {
        public System.Action<int, Vector3> onPlayAudio { get; set; }
        public System.Action<string, System.Action<GameObject>> prefabLoadFunc {get;set;}
        protected Dictionary<int, int> m_effectTypeDic;
        protected GameObjectPoolContext m_objectContext;

        public EffectCtrl(GameObjectPoolContext pool)
        {
            m_objectContext = pool;
            m_effectTypeDic = new Dictionary<int, int>();
        }
   
        public void Play(int effectId, Vector3 pos, bool playAudio = true, System.Action<GameObject> onCreate = null)
        {
            var effectInfo = EffectCfgTable.Instance.GetRow(effectId);
            if (effectInfo == null)
            {
                Debug.LogError("Effect Config NotExists!" + effectId);
                return;
            }

            if (effectInfo.sfx_id > 0 && playAudio)
            {
                onPlayAudio?.Invoke(effectInfo.sfx_id, pos);
            }

            if (m_objectContext.ExistPool(effectId))
            {
                var effect = m_objectContext.GetInstance(effectId);
                RecordInstance(effectId, effect);
                PlayEffectAtPos(effect, m_objectContext.Root, pos, effectInfo.duration);
                if (onCreate != null)
                {
                    onCreate.Invoke(effect);
                }
                return;
            }
            var sourcePath = effectInfo.source;
            prefabLoadFunc?.Invoke(sourcePath, (prefab)=> {
                if (prefab)
                {
                    m_objectContext.SetPrefab(effectId, prefab, true, false);
                    var instance = m_objectContext.GetInstance(effectId);
                    RecordInstance(effectId, instance);
                    PlayEffectAtPos(instance, m_objectContext.Root, pos, effectInfo.duration);
                    onCreate?.Invoke(instance);
                }
                else
                {
                    Debug.LogError("failed load prefab:" + effectId);
                }
            });
        }

        public void RecordInstance(int id, GameObject instance)
        {
            if (instance)
            {
                var instanceId = instance.GetInstanceID();
                m_effectTypeDic[instanceId] = id;
            }
        }

        public void Play(int effectId, Transform anchor, Vector3 offset = default, bool playAudio = true, System.Action<GameObject> onCreate = null)
        {
            var effectInfo = EffectCfgTable.Instance.GetRow(effectId);
            if (effectInfo == null)
            {
                Debug.LogError("Effect Config NotExists!" + effectId);
                return;
            }

            if (effectInfo.sfx_id > 0 && playAudio)
            {
                onPlayAudio?.Invoke(effectInfo.sfx_id, anchor.position);
            }

            if (m_objectContext.ExistPool(effectId))
            {
                var effect = m_objectContext.GetInstance(effectId);
                RecordInstance(effectId, effect);
                PlayEffectAtPos(effect, anchor, anchor.transform.position + offset, effectInfo.duration);
                if (onCreate != null)
                {
                    onCreate.Invoke(effect);
                }
                return;
            }

            var sourcePath = effectInfo.source;
            prefabLoadFunc?.Invoke(sourcePath, (prefab) =>
            {
                if (prefab)
                {
                    m_objectContext.SetPrefab(effectId, prefab, true, false);
                    var instance = m_objectContext.GetInstance(effectId);
                    RecordInstance(effectId, instance);
                    PlayEffectAtPos(instance, anchor, anchor.transform.position + offset, effectInfo.duration);
                    onCreate?.Invoke(instance);
                }
                else
                {
                    Debug.LogError("failed load prefab:" + effectId);
                }
            });
        }

        protected void PlayEffectAtPos(GameObject effect, Transform parent, Vector3 pos, float duration)
        {
            effect.transform.SetParent(parent);
            effect.transform.position = pos;
            effect.gameObject.SetActive(true);

            if (duration > 0)
            {
                var effectBehaviour = effect.GetComponent<EffectBehaviour>();
                if (!effectBehaviour)
                    effectBehaviour = effect.AddComponent<EffectBehaviour>();
                effectBehaviour.onRecover = SaveBackEffect;
                effectBehaviour.Play(duration * 0.001f);
            }
        }

        public void SaveBackEffect(GameObject effect)
        {
            if (!effect)
                return;

            var instanceId = effect.GetInstanceID();
            if (m_effectTypeDic.TryGetValue(instanceId, out int effectId))
            {
                m_objectContext.SaveBack(effectId, effect);
            }
            else
            {
                GameObject.Destroy(effect);
            }
        }

        public void Release()
        {
            if (m_objectContext != null)
            {
                m_objectContext.Recover();
            }
        }
    }
}