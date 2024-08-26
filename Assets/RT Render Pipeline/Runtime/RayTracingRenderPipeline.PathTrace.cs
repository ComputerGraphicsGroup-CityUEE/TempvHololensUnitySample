using q_common;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Rendering.RayTrace
{
    public partial class RayTracingRenderPipeline
    {
        ComputeBuffer ls_vBuffer;
        ComputeBuffer ls_vnBuffer;
        ComputeBuffer vtIdxBuffer;
        ComputeBuffer weld_vtIdxBuffer;
        ComputeBuffer weld_vtIdx_mapBuffer;
        ComputeBuffer deseiBuffer;
        ComputeBuffer desesBuffer;
        ComputeBuffer vfEdgeMapBuffer;
        ComputeBuffer desVibilityBuffer;

        public RTHandle RenderPathTrace(Camera camera, CommandBuffer cmd, RayTracingRenderPipelineAsset asset, string eye)
        {
            //foreach (var pair in outputTargetsL)
            //{
            //    RTHandles.Release(pair.Value);
            //}
            //outputTargetsL.Clear();

            //foreach (var pair in outputTargetsR)
            //{
            //    RTHandles.Release(pair.Value);
            //}
            //outputTargetsR.Clear();

            var rtOutputTarget = RequireOutputTarget(camera, eye);
            var outputTargetSize = RequireOutputTargetSize(camera);
            var PRNGStates = RequirePRNGStates(camera);

            ls_vBuffer = RequireBufferData(camera, ls_v_arr, ls_vBufferPairs);
            ls_vnBuffer = RequireBufferData(camera, ls_vn_arr, ls_vnBufferPairs);
            vtIdxBuffer = RequireBufferData(camera, vtIdx, vtIdxPairs);
            weld_vtIdxBuffer = RequireBufferData(camera, weld_vtIdx, weld_vtIdxBufferPairs);
            weld_vtIdx_mapBuffer = RequireBufferData(camera, weld_vtIdx_map, weld_vtIdx_mapBufferPairs);
            vfEdgeMapBuffer = RequireBufferData(camera, vfEdgesMap, vfEdgeMapBufferPairs);
            deseiBuffer = RequireBufferData(camera, des_ei_arr, deseiBufferPairs);
            desesBuffer = RequireBufferData(camera, des_es_arr, desesBufferPairs);
            desVibilityBuffer = RequireBufferData(camera, des_vinfo_arr, desVibilityBufferPairs);

            if (isVisibilityNeedToBeBuilt)
            {
                ClearVisibilityBuffer();

                ls_vBuffer = RequireBufferData(camera, ls_v_arr, ls_vBufferPairs);
                ls_vnBuffer = RequireBufferData(camera, ls_vn_arr, ls_vnBufferPairs);
                deseiBuffer = RequireBufferData(camera, des_ei_arr, deseiBufferPairs);
                desesBuffer = RequireBufferData(camera, des_es_arr, desesBufferPairs);
                desVibilityBuffer = RequireBufferData(camera, des_vinfo_arr, desVibilityBufferPairs);

                //isVisibilityNeedToBeBuilt = false;
            }

            int rayPerPixel = asset.RtRenderSetting.RayPerPixel;
            int enableAccumulate = asset.RtRenderSetting.EnableAccumulate ? 1 : 0;
            int enableIndirect = asset.RtRenderSetting.EnableIndirect ? 1 : 0;
            //int indirectBounce = asset.RtRenderSetting.IndirectBounce;
            int renderTypeFlag = (int)asset.RtRenderSetting.renderMode;

            //int vsatEnableIndirect = asset.VsatSetting.VSATEnableIndirect ? 1 : 0;
            int vsatIndirectBounce = asset.VsatSetting.VSATIndirectBounce;

            using (new ProfilingScope(cmd, new ProfilingSampler("RayTracing")))
            {
                cmd.SetRayTracingShaderPass(rayGenAndMissShader, "RayTracing");
                cmd.SetRayTracingIntParam(rayGenAndMissShader, enableAccumulateShaderId, enableAccumulate);
                cmd.SetGlobalInteger(renderTypeFlagShaderId, renderTypeFlag);
                cmd.SetRayTracingIntParam(rayGenAndMissShader, enableIndirectShaderId, enableIndirect);
                // cmd.SetRayTracingIntParam(rayGenAndMissShader, indirectBounceShaderId, indirectBounce);
                // cmd.SetRayTracingIntParam(rayGenAndMissShader, vsatEnableIndirectShaderId, vsatEnableIndirect);
                cmd.SetRayTracingIntParam(rayGenAndMissShader, vsatIndirectBounceShaderId, vsatIndirectBounce);
                cmd.SetRayTracingIntParam(rayGenAndMissShader, frameIndexShaderId, frameIndex);
                cmd.SetRayTracingIntParam(rayGenAndMissShader, rayPerPixelShaderId, rayPerPixel);
                cmd.SetRayTracingBufferParam(rayGenAndMissShader, prngStatesShaderId, PRNGStates);

                cmd.SetRayTracingFloatParam(rayGenAndMissShader, HDRExposureShaderId, cubeMapSetting.M_HDRParams.exposureToGamma);
                cmd.SetRayTracingVectorParam(rayGenAndMissShader, HDRTintShaderId, cubeMapSetting.M_HDRParams.Tint);
                Vector4 HDRDecodeFlag = cubeMapSetting.SetHDRDecodeFlag(cubeMapSetting.M_HDRParams.colorDecodeFlag);
                cmd.SetRayTracingVectorParam(rayGenAndMissShader, samLinearClampHDRShaderId, HDRDecodeFlag);

                cmd.SetRayTracingBufferParam(rayGenAndMissShader, deseiShaderId, deseiBuffer);
                cmd.SetRayTracingBufferParam(rayGenAndMissShader, desesShaderId, desesBuffer);
                cmd.SetRayTracingBufferParam(rayGenAndMissShader, desVInfoShaderId, desVibilityBuffer);
                cmd.SetRayTracingBufferParam(rayGenAndMissShader, vfEdgeMapShaderId, vfEdgeMapBuffer);

                cmd.SetRayTracingBufferParam(rayGenAndMissShader, ls_vShaderId, ls_vBuffer);
                cmd.SetRayTracingBufferParam(rayGenAndMissShader, ls_vnShaderId, ls_vnBuffer);
                cmd.SetRayTracingBufferParam(rayGenAndMissShader, vtIdxShaderId, vtIdxBuffer);
                cmd.SetRayTracingBufferParam(rayGenAndMissShader, weld_vtIdxShaderId, weld_vtIdxBuffer);
                cmd.SetRayTracingBufferParam(rayGenAndMissShader, weld_vtIdx_mapShaderId, weld_vtIdx_mapBuffer);

                cmd.SetRayTracingTextureParam(rayGenAndMissShader, satTexShaderId, cubeMapSetting.sphericalSat);
                cmd.SetRayTracingTextureParam(rayGenAndMissShader, lutBrdfShaderId, cubeMapSetting.lutBrdf);
                cmd.SetRayTracingTextureParam(rayGenAndMissShader, cubeTextureShaderId, cubeMapSetting.cubemapping);

                cmd.SetRayTracingVectorParam(rayGenAndMissShader, outputTargetSizeShaderId, outputTargetSize);
                cmd.SetRayTracingAccelerationStructure(rayGenAndMissShader, accelerationStructureShaderId, accelerationStructure);
                cmd.SetRayTracingTextureParam(rayGenAndMissShader, outputTargetShaderId, rtOutputTarget);

                cmd.DispatchRays(rayGenAndMissShader, "MISRayGenShader", (uint)rtOutputTarget.rt.width, (uint)rtOutputTarget.rt.height, 1, camera);
            }

            return rtOutputTarget;
        }


        public RTHandle RenderPathTrace(Camera camera, CommandBuffer cmd, RayTracingRenderPipelineAsset asset)
        {

            var rtOutputTarget = RequireOutputTarget(camera);
            var outputTargetSize = RequireOutputTargetSize(camera);
            var PRNGStates = RequirePRNGStates(camera);

            ls_vBuffer = RequireBufferData(camera, ls_v, ls_vBufferPairs);
            ls_vnBuffer = RequireBufferData(camera, ls_vn, ls_vnBufferPairs);
            vtIdxBuffer = RequireBufferData(camera, vtIdx, vtIdxPairs);
            weld_vtIdxBuffer = RequireBufferData(camera, weld_vtIdx, weld_vtIdxBufferPairs);
            weld_vtIdx_mapBuffer = RequireBufferData(camera, weld_vtIdx_map, weld_vtIdx_mapBufferPairs);
            vfEdgeMapBuffer = RequireBufferData(camera, vfEdgesMap, vfEdgeMapBufferPairs);

            deseiBuffer = RequireBufferData(camera, des_ei_arr, deseiBufferPairs);
            desesBuffer = RequireBufferData(camera, des_es_arr, desesBufferPairs);
            desVibilityBuffer = RequireBufferData(camera, des_vinfo_arr, desVibilityBufferPairs);

            //if (isVisibilityNeedToBeBuilt)
            //{
            //    ClearVisibilityBuffer();

            //    ls_vBuffer = RequireBufferData(camera, ls_v_arr, ls_vBufferPairs);
            //    ls_vnBuffer = RequireBufferData(camera, ls_vn_arr, ls_vnBufferPairs);
            //    deseiBuffer = RequireBufferData(camera, des_ei_arr, deseiBufferPairs);
            //    desesBuffer = RequireBufferData(camera, des_es_arr, desesBufferPairs);
            //    desVibilityBuffer = RequireBufferData(camera, des_vinfo_arr, desVibilityBufferPairs);

            //    isVisibilityNeedToBeBuilt = false;
            //}

            int rayPerPixel = asset.RtRenderSetting.RayPerPixel;
            int enableAccumulate = asset.RtRenderSetting.EnableAccumulate ? 1 : 0;
            int enableIndirect = asset.RtRenderSetting.EnableIndirect ? 1 : 0;
            //int indirectBounce = asset.RtRenderSetting.IndirectBounce;
            int renderTypeFlag = (int)asset.RtRenderSetting.renderMode;

            //int vsatEnableIndirect = asset.VsatSetting.VSATEnableIndirect ? 1 : 0;
            int vsatIndirectBounce = asset.VsatSetting.VSATIndirectBounce;

            using (new ProfilingScope(cmd, new ProfilingSampler("RayTracing")))
            {
                cmd.SetRayTracingShaderPass(rayGenAndMissShader, "RayTracing");
                cmd.SetRayTracingIntParam(rayGenAndMissShader, enableAccumulateShaderId, enableAccumulate);
                cmd.SetGlobalInteger(renderTypeFlagShaderId, renderTypeFlag);
                cmd.SetRayTracingIntParam(rayGenAndMissShader, enableIndirectShaderId, enableIndirect);
                // cmd.SetRayTracingIntParam(rayGenAndMissShader, indirectBounceShaderId, indirectBounce);
                // cmd.SetRayTracingIntParam(rayGenAndMissShader, vsatEnableIndirectShaderId, vsatEnableIndirect);
                cmd.SetRayTracingIntParam(rayGenAndMissShader, vsatIndirectBounceShaderId, vsatIndirectBounce);
                cmd.SetRayTracingIntParam(rayGenAndMissShader, frameIndexShaderId, frameIndex);
                cmd.SetRayTracingIntParam(rayGenAndMissShader, rayPerPixelShaderId, rayPerPixel);
                cmd.SetRayTracingBufferParam(rayGenAndMissShader, prngStatesShaderId, PRNGStates);

                cmd.SetRayTracingFloatParam(rayGenAndMissShader, HDRExposureShaderId, cubeMapSetting.M_HDRParams.exposureToGamma);
                cmd.SetRayTracingVectorParam(rayGenAndMissShader, HDRTintShaderId, cubeMapSetting.M_HDRParams.Tint);
                Vector4 HDRDecodeFlag = cubeMapSetting.SetHDRDecodeFlag(cubeMapSetting.M_HDRParams.colorDecodeFlag);
                cmd.SetRayTracingVectorParam(rayGenAndMissShader, samLinearClampHDRShaderId, HDRDecodeFlag);

                cmd.SetRayTracingBufferParam(rayGenAndMissShader, deseiShaderId, deseiBuffer);
                cmd.SetRayTracingBufferParam(rayGenAndMissShader, desesShaderId, desesBuffer);
                cmd.SetRayTracingBufferParam(rayGenAndMissShader, desVInfoShaderId, desVibilityBuffer);
                cmd.SetRayTracingBufferParam(rayGenAndMissShader, vfEdgeMapShaderId, vfEdgeMapBuffer);

                cmd.SetRayTracingBufferParam(rayGenAndMissShader, ls_vShaderId, ls_vBuffer);
                cmd.SetRayTracingBufferParam(rayGenAndMissShader, ls_vnShaderId, ls_vnBuffer);
                cmd.SetRayTracingBufferParam(rayGenAndMissShader, vtIdxShaderId, vtIdxBuffer);
                cmd.SetRayTracingBufferParam(rayGenAndMissShader, weld_vtIdxShaderId, weld_vtIdxBuffer);
                cmd.SetRayTracingBufferParam(rayGenAndMissShader, weld_vtIdx_mapShaderId, weld_vtIdx_mapBuffer);
                
                cmd.SetRayTracingTextureParam(rayGenAndMissShader, satTexShaderId, cubeMapSetting.sphericalSat);
                cmd.SetRayTracingTextureParam(rayGenAndMissShader, lutBrdfShaderId, cubeMapSetting.lutBrdf);
                cmd.SetRayTracingTextureParam(rayGenAndMissShader, cubeTextureShaderId, cubeMapSetting.cubemapping);

                cmd.SetRayTracingVectorParam(rayGenAndMissShader, outputTargetSizeShaderId, outputTargetSize);
                cmd.SetRayTracingAccelerationStructure(rayGenAndMissShader, accelerationStructureShaderId, accelerationStructure);
                cmd.SetRayTracingTextureParam(rayGenAndMissShader, outputTargetShaderId, rtOutputTarget);

                cmd.DispatchRays(rayGenAndMissShader, "MISRayGenShader", (uint)rtOutputTarget.rt.width, (uint)rtOutputTarget.rt.height, 1, camera);
            }

            return rtOutputTarget;
        }

        void ClearVisibilityBuffer()
        {
            deseiBuffer?.Release();
            desesBuffer?.Release();
            desVibilityBuffer?.Release();
            ls_vBuffer?.Release();
            ls_vnBuffer?.Release();

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
        }
    }
}