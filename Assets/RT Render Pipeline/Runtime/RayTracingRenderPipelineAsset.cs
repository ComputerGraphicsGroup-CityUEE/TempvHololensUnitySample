using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using Rendering.RayTrace;
using UnityEngine.Rendering.Denoising;
using System;

[CreateAssetMenu(fileName = "Ray Tracing Render Pipeline Asset", menuName = "Rendering/Ray Tracing Render Pipeline Asset")]
public class RayTracingRenderPipelineAsset : RenderPipelineAsset
{
    public RayTracingShader rtShader;
    public CubeMapSetting cubeMapSetting;
    [SerializeField]
    public static bool enableDenoise;

    [Serializable]
    public struct RTRenderSetting
    {
        public bool EnableAccumulate;
        [Range(1, 100)]
        public int RayPerPixel;

        public enum RTRenderMode { MIS, VSAT }
        public RTRenderMode renderMode;
        public bool EnableIndirect;
    }

    [SerializeField]
    RTRenderSetting rtRenderSetting = new RTRenderSetting
    {
        // RTRenderSetting.EnableDenoise = EnableDenoise,
        EnableAccumulate = false,
        RayPerPixel = 1,
        renderMode = RTRenderSetting.RTRenderMode.VSAT,
        EnableIndirect = false,
    };
    public RTRenderSetting RtRenderSetting
    {
        get { return rtRenderSetting; }
        set { rtRenderSetting = value; }
    }
    public bool EnableIndirect
    {
        get { return rtRenderSetting.EnableIndirect; }
        set { rtRenderSetting.EnableIndirect = value; }
    }
    public RTRenderSetting.RTRenderMode RenderMode
    {
        get { return rtRenderSetting.renderMode; }
        set { rtRenderSetting.renderMode = value; }
    }

    public static bool EnableDenoise
    {
        get { return enableDenoise; }
        set { enableDenoise = value; }
    }


    [Serializable]
    public struct VSATSetting
    {
        [Range(1, 8)]
        public int VSATIndirectBounce;
    }

    [SerializeField]
    VSATSetting vsatSetting = new VSATSetting
    {
        VSATIndirectBounce = 1
    };
    public VSATSetting VsatSetting => vsatSetting;


    protected override RenderPipeline CreatePipeline()
    {
        return new RayTracingRenderPipeline(this);
    }

    public RenderPipeline ResetPipeline()
    {
        return new RayTracingRenderPipeline(this);
    }

    public RenderPipeline ResetPipeline(CubeMapSetting cubemapSetting)
    {
        this.cubeMapSetting = cubemapSetting;
        return new RayTracingRenderPipeline(this);
    }
}
