//*************************************************************************************
//* 作    者： 
//* 创建时间： 2021-12-19 10:22:53
//*  描    述：

//* ************************************************************************************
using System;
using System.Collections.Generic;

using UnityEngine;

namespace UFrame.NetSocket
{
    internal class PacketDataPool
    {
        private Queue<PacketData> m_dataQueue = new Queue<PacketData>();
        private Stack<PacketData> m_poolList = new Stack<PacketData>();
        private List<PacketData> m_processList = new List<PacketData>();
        private byte[] headBytes = new byte[4];

        internal int BufferSize { get; private set; }
        internal int HeadSize => 4;
        internal PacketDataPool(int bufferSize)
        {
            BufferSize = bufferSize;
        }

        internal PacketData GetEmptyPacketData()
        {
            if (m_poolList.Count > 0)
            {
                return m_poolList.Pop();
            }
            return new PacketData(BufferSize);
        }

        internal void EnqueueDataPacket(PacketData packetData)
        {
            if (packetData.offset == packetData.length)
            {
                Debug.LogError("空数据包移除:" + packetData.length);
                packetData.Clear();
                m_poolList.Push(packetData);
            }
            else
            {
                //Debug.Log("新增加数据包:" + packetData.length);
                m_dataQueue.Enqueue(packetData);
            }
        }

        internal bool CheckExistData(out int dataLength)
        {
            if (m_dataQueue.Count == 0 && m_processList.Count == 0)
            {
                dataLength = 0;
                return false;
            }
            while (m_dataQueue.Count > 0)
            {
                var packet = m_dataQueue.Dequeue();
                m_processList.Add(packet);
            }
            dataLength = 0;
            int existLength = 0;
            int readOffset = 0;
            for (int i = 0; i < m_processList.Count; i++)
            {
                var packet = m_processList[i];
                var packetLength = packet.length - packet.offset;
                for (int dataOffset = packet.offset; dataOffset < packet.length && readOffset < 4; dataOffset++)
                    headBytes[readOffset++] = packet.buffer[dataOffset];
                existLength += packetLength;
            }
            if (existLength >= headBytes.Length)
            {
                dataLength = BitConverter.ToInt32(headBytes, 0) + headBytes.Length;
                Debug.Log("解析出数据包长度:" + dataLength + " 总数据存在长度:" + existLength);
                return dataLength <= existLength;
            }
            else
            {
                Debug.Log("未解析出数据包长度:" + dataLength + " 数据队列长度:" + m_processList.Count);
            }
            return false;
        }

        internal bool CollectData(byte[] targetBuff, int dataLength)
        {
            int usedLength = 0;
            int usedPacketNum = 0;
            for (int i = 0; i < m_processList.Count; i++)
            {
                var packetData = m_processList[i];
                var packetLength = packetData.length - packetData.offset;
                var needLength = dataLength - usedLength;
                var needLengthNow = Mathf.Min(needLength, packetLength);

                if (packetLength > 0 && needLengthNow > 0)
                {
                    Buffer.BlockCopy(packetData.buffer, packetData.offset, targetBuff, usedLength, needLengthNow);
                    usedLength += needLengthNow;
                }
                if (packetLength <= needLength)
                {
                    usedPacketNum++;
                    if (packetLength == needLength)
                        break;
                }
                else
                {
                    packetData.offset = needLengthNow;
                    break;
                }
            }
            for (int i = 0; i < usedPacketNum && m_processList.Count > 0; i++)
            {
                var packetData = m_processList[0];
                packetData.Clear();
                m_poolList.Push(packetData);
                m_processList.RemoveAt(0);
            }
            return usedLength == dataLength;
        }
    }

    public class PacketData
    {
        public int offset;
        public int length;
        public byte[] buffer;
        public PacketData(int buffSize)
        {
            buffer = new byte[buffSize];
        }
        public void Clear()
        {
            offset = 0;
            length = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = 0;
            }
        }
        internal void MakePacket(ushort opcode, byte[] buffer, int offset, int len)
        {
            System.Buffer.BlockCopy(BitConverter.GetBytes(len + 2), 0, this.buffer, 0, 4);
            System.Buffer.BlockCopy(BitConverter.GetBytes(opcode), 0, this.buffer, 4, 2);
            System.Buffer.BlockCopy(buffer, offset, this.buffer, 6, len);
            this.offset = 0;
            this.length = len + 6;
        }

        internal void CopyBuff(byte[] buffer, int offset, int len)
        {
            System.Buffer.BlockCopy(buffer, offset, this.buffer, 0, len);
            this.offset = 0;
            this.length = len;
        }
    }
}