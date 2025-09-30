/****************************************************************************//*
 *   寻路系统                                                                  *
 *    - 查找起点到终点可行走路径                                               *
 *    - 读取并使用阻碍点坐标                                                   *
 *    - 地图点状态判断                                                         *
 *                                                                             *
 *******************************************************************************/

using System;

using UnityEngine;

namespace UFrame.PathFind
{
    [System.Serializable]
    public class MapData
    {
        public byte[] data;
        public int width;
        public int height;
        public byte this[int x, int y]
        {
            get
            {
                if (!IsGridValid(x, y))
                {
                    Debug.LogError("Grid (" + x + "," + y + ") is not exist! map width " + width + " height " + height);
                    return 0;
                }

                return data[y * width + x];
            }
            set
            {
                if (!IsGridValid(x, y))
                {
                    Debug.LogError("Grid (" + x + "," + y + ") is not exist! map width " + width + " height " + height);
                    return;
                }

                data[y * width + x] = value;
            }
        }
        public void Initialize(int width, int height)
        {
            this.width = width;
            this.height = height;
            this.data = new byte[width * height];
        }
        public bool Initialize(int width, int height, byte[] bytes)
        {
            this.width = width;
            this.height = height;
            if (bytes == null || bytes.Length != width * height)
                return false;
            this.data = bytes;
            return true;
        }

        public bool Valid()
        {
            return width > 0 && height > 0 && width * height == data.Length;
        }

        public bool IsGridValid(int x, int y)
        {
            return x < width && y < height && x >= 0 && y >= 0;
        }
    }
}