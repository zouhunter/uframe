using System.Collections.Generic;

namespace UFrame
{
    public class ObserverMap<Key>
    {
        private Dictionary<Key, List<BaseObserver<Key>>> m_observerMap;
        private Queue<DelyProcess> m_delyProcessQueue;
        private HashSet<Key> m_runinglock;
        private Stack<DelyProcess> m_delyProcessPool;

        public class DelyProcess
        {
            public Key observerKey;
            public BaseObserver<Key> observer;
            public bool add;
        }

        public ObserverMap()
        {
            m_observerMap = new Dictionary<Key, List<BaseObserver<Key>>>();
            m_delyProcessQueue = new Queue<DelyProcess>();
            m_runinglock = new HashSet<Key>();
            m_delyProcessPool = new Stack<DelyProcess>();
        }

        public void RegisterObserver(Key observerKey, BaseObserver<Key> observer)
        {
            if (m_runinglock.Contains(observerKey))
            {
                var delyProcess = GetProcess(true, observerKey, observer);
                m_delyProcessQueue.Enqueue(delyProcess);
            }
            else
            {
                RegistObserverpublic(observerKey, observer);
            }
        }

        private DelyProcess GetProcess(bool add, Key observerKey, BaseObserver<Key> observer)
        {
            DelyProcess process;
            if (m_delyProcessPool.Count > 0)
                process = m_delyProcessPool.Pop();
            else
                process = new DelyProcess();
            process.observerKey = observerKey;
            process.observer = observer;
            return process;
        }

        private void RegistObserverpublic(Key observerKey, BaseObserver<Key> observer)
        {
            if (!m_observerMap.TryGetValue(observerKey, out var observerList) || observerList == null)
            {
                observerList = m_observerMap[observerKey] = new List<BaseObserver<Key>>();
            }
            if (!observerList.Contains(observer))
            {
                observerList.Add(observer);
            }
        }

        public bool HasObserver(Key observerKey)
        {
            if (m_observerMap.TryGetValue(observerKey, out var list) && list != null && list.Count > 0)
                return true;
            return false;
        }

        public void RemoveObserver(Key observerKey, object context)
        {
            if (m_observerMap.TryGetValue(observerKey, out var observers))
            {
                if (m_runinglock.Contains(observerKey))
                {
                    foreach (var observer in observers)
                    {
                        var delyProcess = GetProcess(false, observerKey, observer);
                        m_delyProcessQueue.Enqueue(delyProcess);
                    }
                }
                else
                {
                    observers.RemoveAll(x => x.Context?.Equals(context)??false);
                }
            }
        }

        private void RemoveObserverpublic(Key observerKey, BaseObserver<Key> observer)
        {
            if (m_observerMap.TryGetValue(observerKey, out var observers))
            {
                observers.Remove(observer);
            }
        }

        public bool BeginNotify(Key key)
        {
            if (m_runinglock.Contains(key))
            {
                UnityEngine.Debug.LogError("notify stack error:" + key);
                return false;
            }
            m_runinglock.Add(key);
            return true;
        }

        public void EndNotify(Key key)
        {
            m_runinglock.Remove(key);
            while (m_delyProcessQueue.Count > 0)
            {
                var process = m_delyProcessQueue.Dequeue();
                if (process.add)
                {
                    RegistObserverpublic(process.observerKey, process.observer);
                }
                else
                {
                    RemoveObserverpublic(process.observerKey, process.observer);
                }
                m_delyProcessPool.Push(process);
            }
        }

        public void NotifyObservers(Key observerKey)
        {
            if (!BeginNotify(observerKey))
                return;

            if (m_observerMap.TryGetValue(observerKey, out var observers_ref) && observers_ref.Count > 0)
            {
                for (int i = 0; i < observers_ref.Count; i++)
                {
                    var observer = observers_ref[i];
                    if (observer == null)
                        continue;
                    try
                    {
                        if (!observer.Valid)
                        {
                            var delyProcess = GetProcess(false, observerKey, observer);
                            m_delyProcessQueue.Enqueue(delyProcess);
                            continue;
                        }
                        observer.NotifyObserver(observerKey);
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                        var delyProcess = GetProcess(false, observerKey, observer);
                        m_delyProcessQueue.Enqueue(delyProcess);
                    }
                }
            }
            EndNotify(observerKey);
        }
        public void NotifyObservers<T1>(Key observerKey,T1 body1)
        {
            if (!BeginNotify(observerKey))
                return;
            if (m_observerMap.TryGetValue(observerKey, out var observers_ref) && observers_ref.Count > 0)
            {
                for (int i = 0; i < observers_ref.Count; i++)
                {
                    var observer = observers_ref[i];
                    if (observer == null)
                        continue;
                    try
                    {
                        if (!observer.Valid)
                        {
                            var delyProcess = GetProcess(false, observerKey, observer);
                            m_delyProcessQueue.Enqueue(delyProcess);
                            continue;
                        }
                        observer.NotifyObserver(observerKey,body1);
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                        var delyProcess = GetProcess(false, observerKey, observer);
                        m_delyProcessQueue.Enqueue(delyProcess);
                    }
                }
            }
            EndNotify(observerKey);
        }
        public void NotifyObservers<T1, T2>(Key observerKey, T1 body1, T2 body2)
        {
            if (!BeginNotify(observerKey))
                return;
            if (m_observerMap.TryGetValue(observerKey, out var observers_ref) && observers_ref.Count > 0)
            {
                for (int i = 0; i < observers_ref.Count; i++)
                {
                    var observer = observers_ref[i];
                    if (observer == null)
                        continue;
                    try
                    {
                        if (!observer.Valid)
                        {
                            var delyProcess = GetProcess(false, observerKey, observer);
                            m_delyProcessQueue.Enqueue(delyProcess);
                            continue;
                        }
                        observer.NotifyObserver(observerKey,body1,body2);
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                        var delyProcess = GetProcess(false, observerKey, observer);
                        m_delyProcessQueue.Enqueue(delyProcess);
                    }
                }
            }
            EndNotify(observerKey);
        }
        public void NotifyObservers<T1, T2, T3>(Key observerKey, T1 body1, T2 body2, T3 body3)
        {
            if (!BeginNotify(observerKey))
                return;
            if (m_observerMap.TryGetValue(observerKey, out var observers_ref) && observers_ref.Count > 0)
            {
                for (int i = 0; i < observers_ref.Count; i++)
                {
                    var observer = observers_ref[i];
                    if (observer == null)
                        continue;
                    try
                    {
                        if (!observer.Valid)
                        {
                            var delyProcess = GetProcess(false, observerKey, observer);
                            m_delyProcessQueue.Enqueue(delyProcess);
                            continue;
                        }
                        observer.NotifyObserver(observerKey,body1,body2,body3);
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogException(e);
                        var delyProcess = GetProcess(false, observerKey, observer);
                        m_delyProcessQueue.Enqueue(delyProcess);
                    }
                }
            }
            EndNotify(observerKey);
        }
    }
}
