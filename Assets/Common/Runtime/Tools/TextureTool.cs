/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   图片相关工具。                                                                *
*//************************************************************************************/

using UnityEngine;

namespace UFrame
{
    public static class TextureTool
    {
        public static Texture2D GetTexByTargetWidth(Texture2D source, int targetWidth)
        {
            return ScaleTexture(source, targetWidth, (source.height * targetWidth) / source.width);
        }
        /// <summary>
        /// 图片压缩大小
        /// </summary>
        /// <returns>The texture.</returns>
        /// <param name="source">Source.</param>
        /// <param name="targetWidth">Target width.</param>
        /// <param name="targetHeight">Target height.</param>
        public static Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false);
            //float incX = (1.0f / (float)targetWidth);
            //float incY = (1.0f / (float)targetHeight);

            for (int i = 0; i < result.height; ++i)
            {
                for (int j = 0; j < result.width; ++j)
                {
                    Color newColor = source.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
                    result.SetPixel(j, i, newColor);
                }
            }
            result.Apply();
            return result;
        }

        /// 对尺寸进行缩放以实现尺寸压缩
        public static byte[] ScaleTextureToTargetLength(byte[] bytes, int targetWidth)
        {
            var source = new Texture2D(0, 0);
            source.LoadImage(bytes);
            var width = targetWidth;
            var height = Mathf.FloorToInt((source.height / (float)source.width) * width);
            var scaledTexture = ScaleTextureHigh(source, width, height);
            return scaledTexture.EncodeToJPG();
        }

        public static Texture2D ScaleTextureHigh(Texture2D source, int targetWidth, int targetHeight)
        {
            Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
            Color[] rpixels = result.GetPixels(0);
            float incX = ((float)1 / source.width) * ((float)source.width / targetWidth);
            float incY = ((float)1 / source.height) * ((float)source.height / targetHeight);
            for (int px = 0; px < rpixels.Length; px++)
            {
                rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
            }
            result.SetPixels(rpixels, 0);
            result.filterMode = FilterMode.Point;
            result.mipMapBias = 1;
            result.Apply();
            return result;
        }
    }
}