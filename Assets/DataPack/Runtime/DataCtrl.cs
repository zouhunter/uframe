//*************************************************************************************
//* 作    者： 邹杭特
//* 创建时间： 2021-12-17 11:15:12
//* 描    述： 数据管理控制器

//* ************************************************************************************
using System.Collections.Generic;

namespace UFrame.DataPack
{
    public class DataCtrl
    {
        private Dictionary<int, Dictionary<int, DataInfo>> m_dataMap = new Dictionary<int, Dictionary<int, DataInfo>>();
        public event System.Action<int, int> dataChangeEvent;
        public event System.Action dataReloadEvent;
        private int m_currentGroup;
        private bool m_inDataChangeEvent;
        private Queue<DelyDataProcess> m_delyProcess = new Queue<DelyDataProcess>();
        public ref int ActiveGroupId => ref m_currentGroup;

        public void ClearAll()
        {
            m_dataMap.Clear();
            dataChangeEvent = null;
            m_currentGroup = 0;
            m_delyProcess.Clear();
        }

        public void CopyFrom(DataInfoGroup groupItem, bool overwrite = true)
        {
            if (groupItem == null || groupItem.infos == null)
                return;

            if (!m_dataMap.TryGetValue(groupItem.groupId, out var groupMap))
            {
                groupMap = new Dictionary<int, DataInfo>();
                m_dataMap[groupItem.groupId] = groupMap;
            }
            foreach (var info in groupItem.infos)
            {
                if (groupMap.ContainsKey(info.id) && !overwrite)
                    continue;
                groupMap[info.id] = info;
            }
            if (dataReloadEvent != null)
                dataReloadEvent.Invoke();
        }

        public void CopyFrom(List<DataInfoGroup> groups, bool overwrite = true)
        {
            if (groups == null)
                return;

            foreach (var groupItem in groups)
            {
                if (groupItem == null || groupItem.infos == null)
                    continue;

                if (!m_dataMap.TryGetValue(groupItem.groupId, out var groupMap))
                {
                    groupMap = new Dictionary<int, DataInfo>();
                    m_dataMap[groupItem.groupId] = groupMap;
                }
                foreach (var info in groupItem.infos)
                {
                    if (groupMap.ContainsKey(info.id) && !overwrite)
                        continue;
                    groupMap[info.id] = info;
                }
            }
            if (dataReloadEvent != null)
                dataReloadEvent.Invoke();
        }
        public void CopyFrom(int groupId, List<DataInfo> infos, bool overwrite = true)
        {
            if (infos == null)
                return;
            foreach (var dataInfo in infos)
            {
                if (dataInfo == null)
                    continue;
                if (!m_dataMap.TryGetValue(groupId, out var groupMap))
                {
                    groupMap = new Dictionary<int, DataInfo>();
                    m_dataMap[groupId] = groupMap;
                }
                if (groupMap.ContainsKey(dataInfo.id) && !overwrite)
                    continue;
                groupMap[dataInfo.id] = dataInfo;
            }
            if (dataReloadEvent != null)
                dataReloadEvent.Invoke();
        }
        public bool CopyFrom(string json, bool overwrite = true)
        {
            var dataStore = UnityEngine.JsonUtility.FromJson<DataJsonStore>(json);
            if (dataStore != null && dataStore.groupList != null)
            {
                CopyFrom(dataStore.groupList, overwrite);
                return true;
            }
            return false;
        }

        public string SerializeToJson()
        {
            var dataGroup = new List<DataInfoGroup>();
            CollectDatas(dataGroup);
            var dataStore = new DataJsonStore();
            dataStore.groupList = dataGroup;
            return UnityEngine.JsonUtility.ToJson(dataStore);
        }

        public void CollectDatas(Dictionary<int, List<DataInfo>> content)
        {
            if (content == null) return;
            foreach (var pair in m_dataMap)
            {
                if (!content.TryGetValue(pair.Key, out var list))
                {
                    list = new List<DataInfo>();
                    content[pair.Key] = list;
                }
                foreach (var dataPair in pair.Value)
                {
                    list.Add(dataPair.Value);
                }
            }
        }

        public void CollectDatas(Dictionary<int, Dictionary<int, DataInfo>> content)
        {
            if (content == null) return;
            foreach (var pair in m_dataMap)
            {
                if (!content.TryGetValue(pair.Key, out var list))
                {
                    list = new Dictionary<int, DataInfo>();
                    content[pair.Key] = list;
                }
                foreach (var dataPair in pair.Value)
                {
                    list[dataPair.Key] = dataPair.Value;
                }
            }
        }

        public void CollectDatas(List<DataInfoGroup> content)
        {
            if (content == null) return;
            foreach (var pair in m_dataMap)
            {
                var group = new DataInfoGroup();
                group.groupId = pair.Key;
                group.infos = new List<DataInfo>();
                foreach (var dataPair in pair.Value)
                {
                    group.infos.Add(dataPair.Value);
                }
                content.Add(group);
            }
        }

        public bool TryCollectGroup(int groupId, out DataInfoGroup groupInfo)
        {
            if (m_dataMap.TryGetValue(groupId, out var pairs))
            {
                groupInfo = new DataInfoGroup();
                groupInfo.groupId = groupId;
                groupInfo.infos = new List<DataInfo>();
                foreach (var pair in pairs)
                {
                    groupInfo.infos.Add(pair.Value);
                }
                return true;
            }
            groupInfo = null;
            return false;
        }

        public void SetData(int id, string value, int flag, int timeStamp)
        {
            SetData(m_currentGroup, id, value, flag, timeStamp);
        }

        public void SetData(int groupId, int id, string value, int flag, int timeStamp)
        {
            if (m_inDataChangeEvent && dataChangeEvent != null)
            {
                var process = new DelyDataProcess();
                process.add = true;
                process.groupId = groupId;
                process.id = id;
                process.data = value;
                process.flag = flag;
                process.timeStamp = timeStamp;
                m_delyProcess.Enqueue(process);
                return;
            }
            if (!m_dataMap.TryGetValue(groupId, out var groupMap))
            {
                groupMap = new Dictionary<int, DataInfo>();
                m_dataMap[groupId] = groupMap;
            }
            if (!groupMap.TryGetValue(id, out var dataInfo))
            {
                dataInfo = new DataInfo();
                dataInfo.id = id;
                groupMap[id] = dataInfo;
            }
            if (dataInfo.data != value)
            {
                dataInfo.data = value;
                m_inDataChangeEvent = true;
                dataChangeEvent?.Invoke(groupId, id);
                m_inDataChangeEvent = false;
            }
            DelyProcess();
        }

        public void RemoveData(int id)
        {
            RemoveData(m_currentGroup, id);
        }

        public void RemoveData(int groupId, int id)
        {
            if (m_inDataChangeEvent && dataChangeEvent != null)
            {
                var process = new DelyDataProcess();
                process.add = false;
                process.groupId = groupId;
                process.id = id;
                m_delyProcess.Enqueue(process);
                return;
            }

            bool dataRemove = false;
            if (m_dataMap.TryGetValue(groupId, out var groupMap))
            {
                if (groupMap.ContainsKey(id))
                {
                    groupMap.Remove(id);
                    dataRemove = true;
                }

                if (groupMap.Count == 0)
                {
                    m_dataMap.Remove(groupId);
                }
            }
            if (dataRemove)
            {
                m_inDataChangeEvent = true;
                dataChangeEvent?.Invoke(groupId, id);
                m_inDataChangeEvent = false;
            }
            DelyProcess();
        }

        private void DelyProcess()
        {
            while (m_delyProcess.Count > 0)
            {
                var dataProcess = m_delyProcess.Dequeue();
                if (dataProcess.add)
                {
                    SetData(dataProcess.groupId, dataProcess.id, dataProcess.data, dataProcess.flag, dataProcess.timeStamp);
                }
                else
                {
                    RemoveData(dataProcess.groupId, dataProcess.id);
                }
            }
        }

        public bool TryGetData(int id, out DataInfo info)
        {
            return TryGetData(m_currentGroup, id, out info);
        }

        public bool TryGetData(int groupId, int id, out DataInfo info)
        {
            if (m_dataMap.TryGetValue(groupId, out var groupDataMap))
            {
                if (groupDataMap.TryGetValue(id, out info))
                    return true;
            }
            info = null;
            return false;
        }

        public bool TryGetDataValue(int id, out string value)
        {
            return TryGetDataValue(m_currentGroup, id, out value);
        }

        public bool TryGetDataValue(int groupId, int id, out string value)
        {
            if (TryGetData(groupId, id, out var info))
            {
                value = info.data;
                return true;
            }
            value = null;
            return false;
        }
    }
}