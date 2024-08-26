using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace Rendering.RayTrace
{
    public partial class RayTracingRenderPipeline
    {
        private static class CameraShaderParams
        {
            public static readonly int
                _WorldSpaceCameraPos = Shader.PropertyToID("_WorldSpaceCameraPos"),
                _InvCameraViewProj = Shader.PropertyToID("_InvCameraViewProj"),
                _CameraFarDistance = Shader.PropertyToID("_CameraFarDistance");
        }

        protected readonly int
            accelerationStructureShaderId = Shader.PropertyToID("_AccelerationStructure"),
            outputTargetSizeShaderId = Shader.PropertyToID("_OutputTargetSize"),
            outputTargetShaderId = Shader.PropertyToID("_OutputTarget");

        private readonly int
            // Data that may be required in raytrace.
            renderTypeFlagShaderId = Shader.PropertyToID("_RenderTypeFlag"),
            frameIndexShaderId = Shader.PropertyToID("_FrameIndex"),
            prngStatesShaderId = Shader.PropertyToID("_PRNGStates"),
            enableAccumulateShaderId = Shader.PropertyToID("_EnableAccumulate"),
            enableIndirectShaderId = Shader.PropertyToID("_EnableIndirect"),
            //indirectBounceShaderId = Shader.PropertyToID("_IndirectBounce"),
            //vsatEnableIndirectShaderId = Shader.PropertyToID("_VsatEnableIndirect"),
            vsatIndirectBounceShaderId = Shader.PropertyToID("_VsatIndirectBounce"),
            rayPerPixelShaderId = Shader.PropertyToID("_RayPerPixel"),
            isAccumulateResetId = Shader.PropertyToID("_IsAccumulateReset"),
            cubeTextureShaderId = Shader.PropertyToID("_CubeTexture"),
            HDRExposureShaderId = Shader.PropertyToID("_HDRExposure"),
            HDRTintShaderId = Shader.PropertyToID("_HDRTint"),
            samLinearClampHDRShaderId = Shader.PropertyToID("_SamLinearClamp_HDR"),
            deseiShaderId = Shader.PropertyToID("_des_ei"),
            desesShaderId = Shader.PropertyToID("_des_es"),
            desVInfoShaderId = Shader.PropertyToID("_des_vinfo"),
            vfEdgeMapShaderId = Shader.PropertyToID("_vfEdgesMap"),
            lutBrdfShaderId = Shader.PropertyToID("_lutBrdf"),
            satTexShaderId = Shader.PropertyToID("_sat"),
            weld_v_allShaderId = Shader.PropertyToID("_weld_v_all"),
            weld_vtIdx_mapShaderId = Shader.PropertyToID("_weld_vtIdx_map"),
            vtIdxShaderId = Shader.PropertyToID("_vtIdx"),
            weld_vtIdxShaderId = Shader.PropertyToID("_weld_vtIdx"),
            ls_vShaderId = Shader.PropertyToID("_ls_v"),
            ls_vnShaderId = Shader.PropertyToID("_ls_vn");
    }
}