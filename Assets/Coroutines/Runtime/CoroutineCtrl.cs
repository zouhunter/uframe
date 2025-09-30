using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UFrame
{
    public class CoroutineCtrl
    {
        private CoroutineBehaviour m_behaviour;
        private Dictionary<int, Coroutine> m_coroutineMap = new Dictionary<int, Coroutine>();

        public void Init()
        {
            m_behaviour = new GameObject("[CoroutineCtrl]").AddComponent<CoroutineBehaviour>();
            Object.DontDestroyOnLoad(m_behaviour.gameObject);
        }

        public Coroutine StartCoroutine(IEnumerator enumerator)
        {
            if (!m_behaviour)
                return null;
            return m_behaviour.StartCoroutine(enumerator);
        }

        public void StopCoroutine(Coroutine croutine)
        {
            if (m_behaviour)
                m_behaviour.StopCoroutine(croutine);
        }

        public void StopAllCoroutines()
        {
            if (m_behaviour)
                m_behaviour.StopAllCoroutines();
            m_coroutineMap.Clear();
        }
    }
}