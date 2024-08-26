using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Denoising;
using UnityEngine.XR;
using q_common;
using UnityEngine.XR.Management;
using System;

namespace Rendering.RayTrace
{
    public partial class RayTracingRenderPipeline : RenderPipeline
    {
        #region Ray-Tracing-Declaration
        private readonly string cmdName = "Ray Trace Render Graph";

        private RayTracingRenderPipelineAsset asset;
        public CubeMapSetting cubeMapSetting;

        private RayTracingAccelerationStructure accelerationStructure;
        public RayTracingShader rayGenAndMissShader;

        CommandBufferDenoiser denoiser;
        Denoiser.State denoiserState;

        RenderTexture stereoRT;
        RenderTexture stereoRtL;
        RenderTexture stereoRtR;
        Texture2D dstTexture;

        Material biltStereo = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/Render/CombineTextures"));
        Material biltLeft = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/Render/SplitStereoLeft"));
        Material biltRight = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/Render/SplitStereoRight"));
        Material bilt = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/Render/BlitCopy"));

        private Matrix4x4 leftProjMatrix = new Matrix4x4();
        private Matrix4x4 rightProjMatrix = new Matrix4x4();

        private readonly Dictionary<int, ComputeBuffer> PRNGStates = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> weld_v_allPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> vtIdxPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> weld_vtIdxBufferPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> weld_vtIdx_mapBufferPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> deseiBufferPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> desesBufferPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> desVibilityBufferPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> vfEdgeMapBufferPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> ls_vBufferPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> ls_vnBufferPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, ComputeBuffer> satTexBufferPairs = new Dictionary<int, ComputeBuffer>();
        private readonly Dictionary<int, RTHandle> outputTargets = new Dictionary<int, RTHandle>();
        private readonly Dictionary<int, RTHandle> outputTargetsL = new Dictionary<int, RTHandle>();
        private readonly Dictionary<int, RTHandle> outputTargetsR = new Dictionary<int, RTHandle>();
        private readonly Dictionary<int, Vector4> outputTargetSizes = new Dictionary<int, Vector4>();

        CommandBuffer cmd;

        public int frameIndex = 0;
        #endregion

        public RayTracingRenderPipeline(RayTracingRenderPipelineAsset asset)
        {
            // Config Setting through scriptable object 
            this.asset = asset;
            this.cubeMapSetting = asset.cubeMapSetting;
            this.rayGenAndMissShader = asset.rtShader;

            leftProjMatrix.SetRow(0, new Vector4(0.87722f, 0.00000f, -0.12278f, 0.00000f));
            leftProjMatrix.SetRow(1, new Vector4(0.00000f, 0.86867f, -0.03524f, 0.00000f));
            leftProjMatrix.SetRow(2, new Vector4(0.00000f, 0.00000f, -1.00002f, -0.02000f));
            leftProjMatrix.SetRow(3, new Vector4(0.00000f, 0.00000f, -1.00000f, 0.00000f));

            rightProjMatrix.SetRow(0, new Vector4(0.87722f, 0.00000f, 0.12278f, 0.00000f));
            rightProjMatrix.SetRow(1, new Vector4(0.00000f, 0.86867f, -0.03524f, 0.00000f));
            rightProjMatrix.SetRow(2, new Vector4(0.00000f, 0.00000f, -1.00002f, -0.02000f));
            rightProjMatrix.SetRow(3, new Vector4(0.00000f, 0.00000f, -1.00000f, 0.00000f));

            /* Vive */
            //leftProjMatrix.SetRow(0, new Vector4(0.81719f, 0.00000f, -0.33962f, 0.00000f));
            //leftProjMatrix.SetRow(1, new Vector4(0.00000f, 0.89543f, 0.00429f, 0.00000f));
            //leftProjMatrix.SetRow(2, new Vector4(0.00000f, 0.00000f, -1.00060f, -0.60018f));
            //leftProjMatrix.SetRow(3, new Vector4(0.00000f, 0.00000f, -1.00000f, 0.00000f));

            //rightProjMatrix.SetRow(0, new Vector4(0.81642f, 0.00000f, 0.31889f, 0.00000f));
            //rightProjMatrix.SetRow(1, new Vector4(0.00000f, 0.89068f, -0.01039f, 0.00000f));
            //rightProjMatrix.SetRow(2, new Vector4(0.00000f, 0.00000f, -1.00060f, -0.60018f));
            //rightProjMatrix.SetRow(3, new Vector4(0.00000f, 0.00000f, -1.00000f, 0.00000f));

            InitResource();
        }

        // Only run in the first frame
        public void InitResource()
        {
            if (denoiser != null)
                DepositeDenoiser();
            InitEnvmap();
            InitDenoiser(2088, 1044);
            InitTemporalVisibility();
            accelerationStructure = new RayTracingAccelerationStructure();

            stereoRT = new RenderTexture(2088, 1044, 0, GraphicsFormat.R32G32B32A32_SFloat);
            stereoRtL = new RenderTexture(1044, 1044, 0, GraphicsFormat.R32G32B32A32_SFloat);
            stereoRtL.enableRandomWrite = true;
            stereoRtR = new RenderTexture(1044, 1044, 0, GraphicsFormat.R32G32B32A32_SFloat);
            stereoRtR.enableRandomWrite = true;
            dstTexture = new Texture2D(2088, 1044, GraphicsFormat.R32G32B32A32_SFloat, TextureCreationFlags.None);
        }

        // Only run in every frames
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {

            BeginFrameRendering(context, cameras);

            RegenVisibility();
            System.Array.Sort(cameras, (lhs, rhs) => (int)(lhs.depth - rhs.depth));
            BuildAccelerationStructure(ref q_obj);

            cmd = CommandBufferPool.Get(cmdName);

            foreach (var camera in cameras)
            {
                // Only render game and scene view camera.
                //if (camera.cameraType != CameraType.Game && camera.cameraType != CameraType.SceneView)
                if (camera.cameraType != CameraType.Game)
                    continue;

                BeginCameraRendering(context, camera);

                if (camera.stereoTargetEye == StereoTargetEyeMask.Left)
                {
                    cmd.ClearRenderTarget(RTClearFlags.All, Color.clear, 1.0f, 0);
                    context.ExecuteCommandBuffer(cmd);

                    SetupCamera(camera, leftProjMatrix);
                    stereoRtL = RenderPathTrace(camera, cmd, asset, "left");

                    if (RayTracingRenderPipelineAsset.EnableDenoise && asset.RenderMode == RayTracingRenderPipelineAsset.RTRenderSetting.RTRenderMode.MIS)
                        biltStereo.SetTexture("_LeftTex", stereoRtL);

                    context.ExecuteCommandBuffer(cmd);
                    context.Submit();
                    cmd.Clear();
                }
                else if (camera.stereoTargetEye == StereoTargetEyeMask.Right)
                {
                    cmd.ClearRenderTarget(RTClearFlags.All, Color.red, 1.0f, 0);
                    context.ExecuteCommandBuffer(cmd);

                    // get right view
                    SetupCamera(camera, rightProjMatrix);

                    stereoRtR = RenderPathTrace(camera, cmd, asset, "right");

                    if (RayTracingRenderPipelineAsset.EnableDenoise && asset.RenderMode == RayTracingRenderPipelineAsset.RTRenderSetting.RTRenderMode.MIS)
                    { 
                        biltStereo.SetTexture("_RightTex", stereoRtR);
                        // merge two view
                        cmd.Blit(null, stereoRT, biltStereo);
                        RenderDenoise(context, cmd, stereoRT, ref dstTexture);
                    }

                    context.ExecuteCommandBuffer(cmd);
                    context.Submit();
                    cmd.Clear();
                }

                // Start setting vr multi pass rendering
                if (camera.stereoTargetEye == StereoTargetEyeMask.Left)
                {
                    context.SetupCameraProperties(camera, camera.stereoEnabled, 0);
                    cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget, 0, CubemapFace.Unknown, -1);

                    if (camera.stereoEnabled)
                    {
                        context.StartMultiEye(camera);
                    }

                    cmd.ClearRenderTarget(RTClearFlags.All, Color.clear, 1.0f, 0);
                    context.ExecuteCommandBuffer(cmd);

                    if (RayTracingRenderPipelineAsset.EnableDenoise && asset.RenderMode == RayTracingRenderPipelineAsset.RTRenderSetting.RTRenderMode.MIS)
                        cmd.Blit(dstTexture, BuiltinRenderTextureType.CameraTarget, biltLeft);
                    else
                        cmd.Blit(stereoRtL, BuiltinRenderTextureType.CameraTarget);

                    context.ExecuteCommandBuffer(cmd);
                    context.Submit();
                    cmd.Clear();

                    if (camera.stereoEnabled)
                    {
                        context.StopMultiEye(camera);
                        context.StereoEndRender(camera);
                    }

                }
                else if (camera.stereoTargetEye == StereoTargetEyeMask.Right)
                {
                    context.SetupCameraProperties(camera, camera.stereoEnabled, 1);
                    cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget, 0, CubemapFace.Unknown, -1);

                    if (camera.stereoEnabled)
                    {
                        context.StartMultiEye(camera);
                    }

                    cmd.ClearRenderTarget(RTClearFlags.All, Color.clear, 1.0f, 0);
                    context.ExecuteCommandBuffer(cmd);

                    if (RayTracingRenderPipelineAsset.EnableDenoise && asset.RenderMode == RayTracingRenderPipelineAsset.RTRenderSetting.RTRenderMode.MIS)
                        cmd.Blit(dstTexture, BuiltinRenderTextureType.CameraTarget, biltRight);
                    else
                        cmd.Blit(stereoRtR, BuiltinRenderTextureType.CameraTarget);
                    
                    context.ExecuteCommandBuffer(cmd);
                    context.Submit();
                    cmd.Clear();

                    if (camera.stereoEnabled)
                    {
                        context.StopMultiEye(camera);
                        context.StereoEndRender(camera);
                    }
                }

#if UNITY_EDITOR
                if (camera.cameraType == UnityEngine.CameraType.SceneView)
                {
                    ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
                }
#endif

                if (camera.cameraType == CameraType.Game)
                    frameIndex++;

                context.Submit();
                EndCameraRendering(context, camera);

            }

            EndFrameRendering(context, cameras);
        }


        protected override void Dispose(bool disposing)
        {
            foreach (var pair in PRNGStates)
            {
                pair.Value.Release();
            }
            PRNGStates.Clear();

            if (null != accelerationStructure)
            {
                accelerationStructure.Dispose();
                accelerationStructure = null;
            }

            foreach (var pair in outputTargets)
            {
                RTHandles.Release(pair.Value);
            }
            outputTargets.Clear();


            foreach (var pair in outputTargetsL)
            {
                RTHandles.Release(pair.Value);
            }
            outputTargetsL.Clear();
            
            foreach (var pair in outputTargetsR)
            {
                RTHandles.Release(pair.Value);
            }
            outputTargetsR.Clear();

            foreach (var pair in deseiBufferPairs)
            {
                pair.Value.Release();
            }
            deseiBufferPairs.Clear();

            foreach (var pair in desesBufferPairs)
            {
                pair.Value.Release();
            }
            desesBufferPairs.Clear();

            foreach (var pair in desVibilityBufferPairs)
            {
                pair.Value.Release();
            }
            desVibilityBufferPairs.Clear();

            foreach (var pair in vfEdgeMapBufferPairs)
            {
                pair.Value.Release();
            }
            vfEdgeMapBufferPairs.Clear();

            foreach (var pair in weld_vtIdxBufferPairs)
            {
                pair.Value.Release();
            }
            weld_vtIdxBufferPairs.Clear();

            foreach (var pair in ls_vBufferPairs)
            {
                pair.Value.Release();
            }
            ls_vBufferPairs.Clear();

            foreach (var pair in ls_vnBufferPairs)
            {
                pair.Value.Release();
            }
            ls_vnBufferPairs.Clear();

            foreach (var pair in weld_v_allPairs)
            {
                pair.Value.Release();
            }
            weld_v_allPairs.Clear();

            foreach (var pair in vtIdxPairs)
            {
                pair.Value.Release();
            }
            vtIdxPairs.Clear();

            foreach (var pair in weld_vtIdx_mapBufferPairs)
            {
                pair.Value.Release();
            }
            weld_vtIdx_mapBufferPairs.Clear();

        }
    }
}