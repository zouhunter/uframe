using UnityEngine;
using System.Collections;

public class ScreenshotBehaviour : MonoBehaviour
{
    public RenderTexture renderTexture;
    /// <summary>  
    /// �������ͼ��   
    /// </summary>  
    /// <returns>The screenshot2.</returns>  
    /// <param name="camera">Camera.Ҫ�����������</param>  
    /// <param name="rect">Rect.����������</param>  
    private Texture2D CaptureCamera(RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D(rt.width, rt.height,rt.graphicsFormat,0,UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
        screenShot.ReadPixels(new Rect(0,0,rt.width,rt.height), 0, 0);
        screenShot.Apply();
        RenderTexture.active = null;
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = Application.dataPath + string.Format( "/Screenshot/pic_{0}.png",System.DateTime.Now.Hour.ToString("00") + System.DateTime.Now.Minute.ToString("00") + System.DateTime.Now.Second.ToString("00"));
        System.IO.File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("������һ����Ƭ: {0}", filename));
        return screenShot;
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Screenshot"))
        {
            CaptureCamera(renderTexture);
        }
    }
}