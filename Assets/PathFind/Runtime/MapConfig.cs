/****************************************************************************//*
 *   寻路系统                                                                  *
 *    - 查找起点到终点可行走路径                                               *
 *    - 读取并使用阻碍点坐标                                                   *
 *    - 地图点状态判断                                                         *
 *      //1、赤色【RGB】255,0,0【CMYK】0,100,100,0；                           *
 *      //                                                                     *
 *      //2、橙色【RGB】255,165,0【CMYK】0,35,100,0；                          *
 *      //                                                                     *
 *      //3、黄色【RGB】255,255,0【CMYK】0,0,100,0；                           *
 *      //                                                                     *
 *      //4、绿色【RGB】0,255,0【CMYK】100,0,100,0；                           *
 *      //                                                                     *
 *      //5、青色【RGB】0,127,255【CMYK】100,50,0,0；                          *
 *      //                                                                     *
 *      //6、蓝色【RGB】0,0,255【CMYK】100,100,0,0；                           *
 *      //                                                                     *
 *      //7、紫色【RGB】139,0,255【CMYK】45,100,0,0。                          *
 *******************************************************************************/

using UnityEngine;

namespace UFrame.PathFind
{
    [System.Serializable]
    public class MapLayer
    {
        public string name;
        public byte value;
        public Color color;
    }

    public class MapConfig : ScriptableObject
    {
        public MapLayer[] layers;
        public int width = 192;
        public int height = 108;
        public int textureSize = 800;
        [HideInInspector]
        public byte[] data;
        public int brushRadius = 5;
        [HideInInspector]
        public TextAsset lastEditMapText;

        public MapConfig()
        {
            InitDefaultLayers();
            ResetData(width, height);
        }

        public void InitDefaultLayers()
        {
            Color[] colors = new Color[] {
                new Color(1,0,0),//赤色
                new Color(1,0.65f,0),//橙色
                new Color(1,1,0),//黄色
                new Color(0,1,0),//绿色
                new Color(0,0.5f,1),//青色
                new Color(0,0,1),//蓝色
                new Color(0.55f,0,1),//紫色
                new Color(1,0,1),
            };
            layers = new MapLayer[colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                var mapLayer = new MapLayer();
                mapLayer.value = (byte)(1 << i);
                mapLayer.name = "L" + (i + 1);
                mapLayer.color = colors[i];
                layers[i] = mapLayer;
            }
        }

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

        public void CheckValied()
        {
            if(layers == null || layers.Length == 0)
            {
                InitDefaultLayers();
            }
            if (data.Length != width * height)
            {
                ChangeMapSize(width, height, true);
            }
        }

        public void ResetData(int width, int height)
        {
            if (this.data == null)
            {
                this.data = new byte[width * height];
            }
            else
            {
                ChangeMapSize(width, height, true);
            }
            this.width = width;
            this.height = height;
        }

        public bool ResetData(int width, int height, byte[] bytes)
        {
            this.width = width;
            this.height = height;
            if (bytes == null || bytes.Length != width * height)
                return false;
            this.data = bytes;
            return true;
        }

        public bool IsGridValid(int x, int y)
        {
            return x < width && y < height && x >= 0 && y >= 0;
        }

        // 从顶部切除或增加
        protected void ClampMapTop(int width, int height)
        {
            var newdata = new byte[width * height];
            if (this.width != width || this.height != height)
            {
                int index = 0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        index = y * width + x;
                        if (this.width > x && this.height > y)
                        {
                            newdata[index] = this[x, y];
                        }
                    }
                }
            }
            ResetData(width, height, newdata);
        }

        // 从两头切除或增加
        protected void ClampMapCenter(int width, int height)
        {
            var newdata = new byte[width * height];
            if (this.width != width || this.height != height)
            {
                int index = 0;
                int centerY = Mathf.FloorToInt(height * 0.5f);
                var action = new System.Action<int, int, int, int>((int x, int y, int oldx, int oldy) =>
                {
                    index = y * width + x;
                    if (this.width > oldx && oldx > 0 && this.height > oldy && oldy > 0)
                    {
                        newdata[index] = this[oldx, oldy];
                    }
                });

                int oldCenterY = Mathf.FloorToInt(this.height * 0.5f);
                for (int yc = 0; yc < centerY; yc++)
                {
                    int y0 = centerY - yc;
                    int y1 = centerY + yc;

                    int oldY0 = oldCenterY - yc;
                    int oldY1 = oldCenterY + yc;

                    int centerX = Mathf.FloorToInt(width * 0.5f);
                    int oldCenterX = Mathf.FloorToInt(this.width * 0.5f);
                    for (int xc = 0; xc < centerX; xc++)
                    {
                        int x0 = centerX - xc;
                        int x1 = centerX + xc;

                        int oldX0 = oldCenterX - xc;
                        int oldX1 = oldCenterX + xc;

                        action(x0, y0, oldX0, oldY0);
                        action(x0, y1, oldX0, oldY1);
                        action(x1, y0, oldX1, oldY0);
                        action(x1, y1, oldX1, oldY1);
                    }
                    action(centerX, centerY, oldCenterX, oldCenterY);
                }
            }
            ResetData(width, height, newdata);
        }
        //插值
        protected void ClampMapLerp(int width, int height)
        {
            var newdata = new byte[width * height];
            if (this.width != width || this.height != height)
            {
                int index = 0;
                for (int y = 0; y < height; y++)
                {
                    int oldY = (int)Mathf.Lerp(0, this.height, (float)y / (height - 1));

                    for (int x = 0; x < width; x++)
                    {
                        int oldX = (int)Mathf.Lerp(0, this.width, (float)x / (width - 1));

                        index = y * width + x;
                        if (this.width > oldX && this.height > oldY && oldY > 0 && oldX > 0)
                        {
                            newdata[index] = this[oldX, oldY];
                        }
                    }
                }
            }
            ResetData(width, height, newdata);
        }

        public void ChangeMapSize(int width, int height, bool lerp)
        {
            if (lerp)
            {
                ClampMapLerp(width, height);
            }
            else
            {
                ClampMapCenter(width, height);
            }
        }

        public void MoveOffset(int xOffset, int yOffset)
        {
            int xSample = xOffset >= 0 ? 1 : -1;
            int ySample = yOffset >= 0 ? 1 : -1;

            int xStart = xOffset >= 0 ? 0 : width - 1;
            int yStart = yOffset >= 0 ? 0 : height - 1;

            for (int y = 0; y < height; y++)
            {
                var currY = yStart + y * ySample;
                var targetY = (yOffset + currY + height) % (height);
                for (int x = 0; x < width; x++)
                {
                    var currX = xStart + x * xSample;
                    var targetX = (xOffset + currX + width) % (width);
                    var value = this[currX, currY];
                    this[currX, currY] = this[targetX, targetY];
                    this[targetX, targetY] = value;
                }
            }
        }
    }
}