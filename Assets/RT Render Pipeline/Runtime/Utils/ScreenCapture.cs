using q_common;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public static class ScreenCapture
{
    public static bool isScreenCapture = false;
    public static ComputeShader blitRenderTextureShader;

    public static void SaveRenderTexture(RenderTexture rt)
    {
        RenderTexture active = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D png = new Texture2D(Screen.width, Screen.height, GraphicsFormat.R32G32B32A32_SFloat, TextureCreationFlags.None);
        png.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        png.Apply();
        RenderTexture.active = active;
        byte[] bytes = png.EncodeToPNG();
        string path = string.Format("Assets/../rt_{3}{4}{5}_{0}{1}{2}.png", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        FileStream fs = File.Open(path, FileMode.Create);
        BinaryWriter writer = new BinaryWriter(fs);
        writer.Write(bytes);
        writer.Flush();
        writer.Close();
        fs.Close();
        /// Destroy(png);
        png = null;
        Debug.Log("The screen capture is saved to local！" + path);
    }
}
