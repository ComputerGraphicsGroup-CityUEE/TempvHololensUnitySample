#ifndef CUSTOM_RT_COMMON_INCLUDED
#define CUSTOM_RT_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "UnityRaytracingMeshUtils.cginc"

#define CBUFFER_START(name) cbuffer name {
#define CBUFFER_END };

// Macro that interpolate any attribute using barycentric coordinates
#define INTERPOLATE_RAYTRACING_ATTRIBUTE(A0, A1, A2, BARYCENTRIC_COORDINATES) (A0 * BARYCENTRIC_COORDINATES.x + A1 * BARYCENTRIC_COORDINATES.y + A2 * BARYCENTRIC_COORDINATES.z)

#define TEXTURE2D(textureName) Texture2D textureName
#define SAMPLER(samplerName) SamplerState samplerName

#ifdef UNITY_COLORSPACE_GAMMA
#define unity_ColorSpaceGrey float4(0.5, 0.5, 0.5, 0.5)
#define unity_ColorSpaceDouble float4(2.0, 2.0, 2.0, 2.0)
#define unity_ColorSpaceDielectricSpec half4(0.220916301, 0.220916301, 0.220916301, 1.0 - 0.220916301)
#define unity_ColorSpaceLuminance half4(0.22, 0.707, 0.071, 0.0) // Legacy: alpha is set to 0.0 to specify gamma mode
#else // Linear values
#define unity_ColorSpaceGrey float4(0.214041144, 0.214041144, 0.214041144, 0.5)
#define unity_ColorSpaceDouble float4(4.59479380, 4.59479380, 4.59479380, 2.0)
#define unity_ColorSpaceDielectricSpec half4(0.04, 0.04, 0.04, 1.0 - 0.04) // standard dielectric reflectivity coef at incident angle (= 4%)
#define unity_ColorSpaceLuminance half4(0.0396819152, 0.458021790, 0.00609653955, 1.0) // Legacy: alpha is set to 1.0 to specify linear mode
#endif

CBUFFER_START(CameraBuffer)
float4x4 _InvCameraViewProj;
float3 _WorldSpaceCameraPos;
float _CameraFarDistance;
CBUFFER_END

#include "Ray.hlsl"


inline void GenerateCameraRay(out float3 origin, out float3 direction)
{
    // center in the middle of the pixel.
    float2 xy = DispatchRaysIndex().xy + 0.5f;

    // the position of screen coordinate in Project Space
    float2 screenPos = xy / DispatchRaysDimensions().xy * 2.0f - 1.0f;

    // the position of screen coordinate in World Space
    float4 world = mul(_InvCameraViewProj, float4(screenPos, 0, 1));

    world.xyz /= world.w;
    origin = _WorldSpaceCameraPos.xyz;
    direction = normalize(world.xyz - origin);
}

inline void GenerateCameraRayWithOffset(out float3 origin, out float3 direction, float2 offset)
{
    float2 xy = DispatchRaysIndex().xy + offset;
    float2 screenPos = xy / DispatchRaysDimensions().xy * 2.0f - 1.0f;

    // Un project the pixel coordinate into a ray.
    float4 world = mul(_InvCameraViewProj, float4(screenPos, 0, 1));

    world.xyz /= world.w;
    origin = _WorldSpaceCameraPos.xyz;
    direction = normalize(world.xyz - origin);
}

float3 remapTo0_1(float3 vec)
{
    return 0.5f * (normalize(vec) + 1.0f);
}

inline float3 DecodeHDR(float4 data, float4 decodeInstructions)
{
    // Take into account texture alpha if decodeInstructions.w is true(the alpha value affects the RGB channels)
    float alpha = decodeInstructions.w * (data.a - 1.0) + 1.0;

    return (decodeInstructions.x * alpha) * data.rgb;

}

inline half3 LinearToGammaSpace(half3 linRGB)
{
    linRGB = max(linRGB, half3(0.h, 0.h, 0.h));
    // An almost-perfect approximation from http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
    return max(1.055h * pow(linRGB, 0.416666667h) - 0.055h, 0.h);
}

float Square(float v) {
    return v * v;
}

float mix(float x, float y, float a)
{
    return x * (1 - a) + y * a;
}

float3 mix(float3 x, float3 y, float a)
{
    return x * (1 - a) + y * a;
}

float3 mix(float3 x, float3 y, float3 a)
{
    return float3 (x.x * (1 - a.x) + y.x * a.x,
                    x.y * (1 - a.y) + y.y * a.y,
                    x.z * (1 - a.z) + y.z * a.z);
}

float4 mix(float4 x, float4 y, float a)
{
    return x * (1 - a) + y * a;
}


float4 atan(float y, float x)
{
    return atan2(x, y) / 2.f;
}

float get_attenuation(float c1, float c2, float d)
{
    return 1.0f / (1 + c1 * d + c2 * d * d);
}

float3 BlendNormal(float3 n1, float3 n2){
    float3x3 nBasis = float3x3(
        float3(n1.z, n1.y, -n1.x), // +90 degree rotation around y axis
        float3(n1.x, n1.z, -n1.y), // -90 degree rotation around x axis
        float3(n1.x, n1.y, n1.z));

    float3 r = normalize(n2.x * nBasis[0] + n2.y * nBasis[1] + n2.z * nBasis[2]);
    return r * 0.5 + 0.5;
}
#endif