using q_common;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Unity.Mathematics;

namespace Rendering.RayTrace
{
    public partial class RayTracingRenderPipeline
    {
        public struct PfmSet
        {
            public q_pfm pfm_xp;
            public q_pfm pfm_xn;
            public q_pfm pfm_yp;
            public q_pfm pfm_yn;
            public q_pfm pfm_zp;
            public q_pfm pfm_zn;
        }

        void InitEnvmap()
        {
            string cachePath;

            if (Application.isPlaying)
                cachePath = Application.streamingAssetsPath + "//";
            else
                cachePath = "Assets/StreamingAssets/";

            Cubemap cm = new Cubemap(256, TextureFormat.RGBAFloat, 9);
            q_pf4 pf4_sat = new q_pf4();
            q_pfm pfm_lutBrdf = new q_pfm();

            pf4_sat.Load(cachePath + "satpad0.pf4");
            pfm_lutBrdf.Load(cachePath + "lutBrdf.pfm");

            PfmSet pfmSet = new PfmSet
            {
                pfm_xp = new q_pfm(cachePath + "stpeters_cross_xp.pfm"),
                pfm_xn = new q_pfm(cachePath + "stpeters_cross_xn.pfm"),
                pfm_yp = new q_pfm(cachePath + "stpeters_cross_yp.pfm"),
                pfm_yn = new q_pfm(cachePath + "stpeters_cross_yn.pfm"),
                pfm_zp = new q_pfm(cachePath + "stpeters_cross_zn.pfm"),
                pfm_zn = new q_pfm(cachePath + "stpeters_cross_zp.pfm")
            };

            pfmSet.pfm_yp.flip_horizontal();
            pfmSet.pfm_yp.transpose();

            pfmSet.pfm_yn.transpose();
            pfmSet.pfm_yn.flip_horizontal();

            cm.SetPixelData<float>(pfmSet.pfm_xp.ColorStreamRGBA, 0, CubemapFace.PositiveX);
            cm.SetPixelData<float>(pfmSet.pfm_xn.ColorStreamRGBA, 0, CubemapFace.NegativeX);
            cm.SetPixelData<float>(pfmSet.pfm_yp.ColorStreamRGBA, 0, CubemapFace.PositiveY);
            cm.SetPixelData<float>(pfmSet.pfm_yn.ColorStreamRGBA, 0, CubemapFace.NegativeY);
            cm.SetPixelData<float>(pfmSet.pfm_zp.ColorStreamRGBA, 0, CubemapFace.PositiveZ);
            cm.SetPixelData<float>(pfmSet.pfm_zn.ColorStreamRGBA, 0, CubemapFace.NegativeZ);

            cm.Apply(true);

            cubeMapSetting.isDirty = true;
            cubeMapSetting.cubemapping = cm;

            cubeMapSetting.sphericalSat = new Texture2D(pf4_sat.W, pf4_sat.H, GraphicsFormat.R32G32B32A32_SFloat, TextureCreationFlags.None);
            cubeMapSetting.sphericalSat.SetPixelData<float>(pf4_sat.ColorStreamRGBA, 0);
            cubeMapSetting.sphericalSat.Apply();

            cubeMapSetting.lutBrdf = new Texture2D(pfm_lutBrdf.W, pfm_lutBrdf.H, GraphicsFormat.R32G32B32A32_SFloat, TextureCreationFlags.None);
            cubeMapSetting.lutBrdf.SetPixelData<float>(pfm_lutBrdf.ColorStreamRGBA, 0);
            cubeMapSetting.lutBrdf.Apply();
        }
    }
}