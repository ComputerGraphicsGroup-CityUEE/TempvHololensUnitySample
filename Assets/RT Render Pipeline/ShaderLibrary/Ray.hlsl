#ifndef CUSTOM_RAY_INCLUDED
#define CUSTOM_RAY_INCLUDED

RaytracingAccelerationStructure _AccelerationStructure;

struct RayIntersection
{
    int remainingDepth;
    uint4 PRNGStates;
    float4 color;
    bool isFirstHit;
    float hitT;
    float3 normalWS;    // hit point normal;
    float3 normalOS;    // hit point normal
    float reflector;    // miss = 0, hit = 1;
    float3 direction;

    float kSpecular;
    float kDiffuse;
    float shininess;

    //for VSAT evaluation
    float3 barycentricCoordinates;
    uint ti;
    uint vi_noweld;
    float3 vn_noweld;
    float roughness;
    float metallic;

    float3 kd;
    float3 ks;
};

struct AttributeData
{
    float2 barycentrics;
};

#endif