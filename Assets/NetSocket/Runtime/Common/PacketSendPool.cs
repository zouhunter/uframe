//*************************************************************************************
//* 作    者： 
//* 创建时间： 2021-12-19 12:43:35
//*  描    述：

//* ************************************************************************************
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UFrame.NetSocket
{
    public class PacketSendPool
    {
        private List<PacketData> m_packetPool = new List<PacketData>();
        private Queue<PacketData> m_packetSaveBack = new Queue<PacketData>();
        private int m_partSize;
        private static object lockBody = new object();
        public int PartSize => m_partSize;

        public PacketSendPool(int packetSize)
        {
            m_partSize = packetSize;
        }

        public PacketData GetEmptyPacket(int minSize)
        {
            lock (lockBody)
            {
                while (m_packetSaveBack.Count > 0)
                {
                    m_packetPool.Add(m_packetSaveBack.Dequeue());
                }
                for (int i = 0; i < m_packetPool.Count; i++)
                {
                    var packet = m_packetPool[i];
                    if (packet.buffer.Length > minSize)
                    {
                        return packet;
                    }
                }
                var packetData = new PacketData(Mathf.Max(minSize, minSize));
                return packetData;
            }
        }

        public void SaveBackPacket(PacketData data)
        {
            lock (lockBody)
            {
                data.Clear();
                m_packetSaveBack.Enqueue(data);
            }
        }
    }
}