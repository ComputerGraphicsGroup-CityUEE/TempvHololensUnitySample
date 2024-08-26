Shader "RayTracing/MISBRDF"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _BaseColorMap("BaseColorMap", 2D) = "white" {}
        _UseNormalMap("UseNormalMap", Integer) = 0
        _NormalMap("NormalMap", 2D) = "white" {}
        _kDiffuse("Diffuse Coefficient", Range(0,1)) = 0.5
        _kSpecular("Specular Coefficient", Range(0,1)) = 0.5
        _Roughness("Roughness", Range(0,1)) = 0.059
        _Metallic("Metallic", Range(0,1)) = 1
    }

    SubShader
    {
        Pass
        {
            Name "RayTracing"
            Tags { "LightMode" = "RayTracing" }

            HLSLPROGRAM

            #pragma raytracing test
            #include "../../ShaderLibrary/Common.hlsl"
            #include "../../ShaderLibrary/PRNG.hlsl"
            #include "ClosestHit.hlsl"
            ENDHLSL
        }
    }
}
