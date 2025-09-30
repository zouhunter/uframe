//*************************************************************************************
//* 作    者： 邹杭特
//* 创建时间： 2021-10-01 10:45:28
//* 描    述：  

//* ************************************************************************************
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

public class CubeMapMaker : EditorWindow
{
    protected Camera m_camera = null;
    protected Cubemap m_cubemap = null;

    [MenuItem("Window/UFrame/CubeMap/CubeMapMaker")]
    static void Render2Cubemap()
    {
        EditorWindow.GetWindow<CubeMapMaker>("CubeMapMaker");
    }

    void OnEnable()
    {
        m_camera = Camera.main;
        m_cubemap = new Cubemap(2048,UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB,UnityEngine.Experimental.Rendering.TextureCreationFlags.None,0);
    }

    void OnGUI()
    {
        m_camera = EditorGUILayout.ObjectField("Camera", m_camera, typeof(Camera), true) as Camera;
        using (var verti = new EditorGUILayout.VerticalScope())
        {
            m_cubemap = EditorGUILayout.ObjectField("CubeMap", m_cubemap, typeof(Cubemap), false) as Cubemap;
            if (m_cubemap != null && m_camera != null && GUILayout.Button("Render2Cubemap"))
            {
                m_camera.RenderToCubemap(m_cubemap);
            }
            if (m_cubemap != null && GUILayout.Button("ExportCubemaps"))
            {
                ExportTexutre(m_cubemap);
            }
        }
    }


    public static void FlipPixels(Texture2D texture, bool flipX, bool flipY)
    {
        Color32[] originalPixels = texture.GetPixels32();

        var flippedPixels = Enumerable.Range(0, texture.width * texture.height).Select(index =>
        {
            int x = index % texture.width;
            int y = index / texture.width;
            if (flipX)
                x = texture.width - 1 - x;

            if (flipY)
                y = texture.height - 1 - y;

            return originalPixels[y * texture.width + x];
        }
        );

        texture.SetPixels32(flippedPixels.ToArray());
        texture.Apply();
    }

    public static void ExportTexutre(Cubemap cubemap)
    {
        int width = cubemap.width;
        int height = cubemap.height;
        Texture2D texture2D = new Texture2D(width, height, cubemap.graphicsFormat, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);

        var defaultPath = Application.dataPath;
        var exportPath = EditorUtility.SaveFolderPanel("Export Png Path:", defaultPath,"");

        if (string.IsNullOrEmpty(exportPath))
            return;

        for (int i = 0; i < 6; i++)
        {
            texture2D.SetPixels(cubemap.GetPixels((CubemapFace)i));
            //翻转像素，由于某种原因，导出的图片需要进行翻转
            FlipPixels(texture2D, false, true);
            var name = "";
            switch ((CubemapFace)i)
            {
                case CubemapFace.PositiveX:
                    name = "Left";
                    break;
                case CubemapFace.NegativeX:
                    name = "Right";
                    break;
                case CubemapFace.PositiveY:
                    name = "Up";
                    break;
                case CubemapFace.NegativeY:
                    name = "Down";
                    break;
                case CubemapFace.PositiveZ:
                    name = "Front";
                    break;
                case CubemapFace.NegativeZ:
                    name = "Back";
                    break;
                default:
                    break;
            }
            //此处导出为png
            File.WriteAllBytes(exportPath + "/" + name + ".png", texture2D.EncodeToPNG());
        }
        DestroyImmediate(texture2D);
    }
}