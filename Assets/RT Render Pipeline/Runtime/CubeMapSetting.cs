using q_common;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static CubeMapSetting;

[CreateAssetMenu(menuName = "Rendering/Enviornment Map/Cube Map Setting")]
public class CubeMapSetting : ScriptableObject
{
    [System.NonSerialized]
    public Cubemap cubemapping;
    [System.NonSerialized]
    public Texture2D sphericalSat;
    [System.NonSerialized]
    public Texture2D lutBrdf;

    #region PFM cubemap
    /// <summary>
    /// whether need to reload cubemap from pfm file.
    /// </summary>
    [System.NonSerialized]
    public bool isDirty = false;

    [Serializable]
    public struct PfmFilePath
    {
        public string pfm_path;
        public string pfm_xp;
        public string pfm_xn;
        public string pfm_yp;
        public string pfm_yn;
        public string pfm_zp;
        public string pfm_zn;
    }

    [SerializeField]
    public PfmFilePath m_PfmFilePath = new PfmFilePath
    {
        pfm_path = "Assets/Resources/pfm/enviornment-map/",
        pfm_xp = "stpeters_cross.tt.xp.pfm",
        pfm_xn = "stpeters_cross.tt.xn.pfm",
        pfm_yp = "stpeters_cross.tt.yp.pfm",
        pfm_yn = "stpeters_cross.tt.yn.pfm",
        pfm_zp = "stpeters_cross.tt.zp.pfm",
        pfm_zn = "stpeters_cross.tt.zn.pfm"
    };
    public PfmFilePath M_PfmFilePath => m_PfmFilePath;
    #endregion

    #region PFM SAT
    public string pfm_sat_path = "Assets/Resources/pfm/satpad0.pf4";
    public string pfm_lutBrdf_path = "Assets/Resources/pfm/lutBrdf.pfm";
    #endregion

    #region HDR Mapping
    // Reference to set flag : https://zhuanlan.zhihu.com/p/35096536
    public enum Flag_samLinearClampHDR 
    {
        pc_default = 0,
        android_gamma_space,
        android_linear_space
    }

    [Serializable]
    public struct HDRParams
    {
        [Range(0f, 8f)]
        public float exposureToGamma;
        [ColorUsage(false, true)]
        public Color Tint;
        public Flag_samLinearClampHDR colorDecodeFlag;
    }

    [SerializeField]
    public HDRParams m_HDRParams = new HDRParams {
        exposureToGamma = 1.0f,
        Tint = new Color(.5f, .5f, .5f, .5f),
        colorDecodeFlag = 0
    };
    public HDRParams M_HDRParams => m_HDRParams;

    /// <summary>
    /// color flag setting reference: https://zhuanlan.zhihu.com/p/35096536
    /// </summary>
    /// <param name="flag"></param>
    /// <returns></returns>
    public Vector4 SetHDRDecodeFlag(Flag_samLinearClampHDR flag)
    {
        if (flag == Flag_samLinearClampHDR.pc_default)
            return new Vector4(1f, 1f, 0f, 1f);
        else if (flag == Flag_samLinearClampHDR.android_gamma_space)
            return new Vector4(2f, 1f, 0f, 0f);
        else if (flag == Flag_samLinearClampHDR.android_linear_space)
            return new Vector4(4.59f, 1f, 0f, 0f);      //return new Vector4(GammaToLinearSpace(2f), 1f, 0f, 0f);

        return new Vector4(1f, 1f, 0f, 1f);
    }

    #endregion
}