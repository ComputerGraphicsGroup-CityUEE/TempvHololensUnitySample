#ifndef CUSTOM_PRNG_INCLUDED
#define CUSTOM_PRNG_INCLUDED

#define M_PI (3.14159265358979323846264338327950288)

RWStructuredBuffer<uint4> _PRNGStates;

inline float cbrt(float d)
{
    return pow(d, 1.0f / 3.0f);
}

uint TausStep(inout uint z, int S1, int S2, int S3, uint M)
{
    uint b = (((z << S1) ^ z) >> S2);
    return z = (((z & M) << S3) ^ b);
}

uint LCGStep(inout uint z)
{
    return z = (1664525 * z + 1013904223);
}

float GetRandomValueTauswortheUniform(inout uint4 states)
{
    uint taus = TausStep(states.x, 13, 19, 12, 4294967294UL) ^ TausStep(states.y, 2, 25, 4, 4294967288UL) ^ TausStep(states.z, 3, 11, 17, 4294967280UL);
    uint lcg = LCGStep(states.w);

    return 2.3283064365387e-10f * (taus ^ lcg); // taus+
}

float GetRandomValue(inout uint4 states)
{
    float rand = GetRandomValueTauswortheUniform(states);
    return rand;
}

float3 GetRandomInUnitSphere(inout uint4 states)
{
    float u = GetRandomValue(states);
    float v = GetRandomValue(states);
    float theta = u * 2.f * (float)M_PI;
    float phi = acos(2.f * v - 1.f);
    float r = cbrt(GetRandomValue(states));
    float sinTheta = sin(theta);
    float cosTheta = cos(theta);
    float sinPhi = sin(phi);
    float cosPhi = cos(phi);
    float x = r * sinPhi * cosTheta;
    float y = r * sinPhi * sinTheta;
    float z = r * cosPhi;
    return float3(x, y, z);
}

float3 GetRandomOnUnitSphere(inout uint4 states)
{
    float r1 = GetRandomValue(states);
    float r2 = GetRandomValue(states);
    float x = cos(2.0f * (float)M_PI * r1) * 2.0f * sqrt(r2 * (1.0f - r2));
    float y = sin(2.0f * (float)M_PI * r1) * 2.0f * sqrt(r2 * (1.0f - r2));
    float z = 1.0f - 2.0f * r2;
    return float3(x, y, z);
}

float2 GetRandomInUnitDisk(inout uint4 states) {
    float a = GetRandomValue(states) * 2.0f * (float)M_PI;
    float r = sqrt(GetRandomValue(states));

    return float2(r * cos(a), r * sin(a));
}

// Generate the random vector in the upper hemisphere
float3 GetRandomCosineDirection(inout uint4 states) {
    float r1 = GetRandomValue(states);
    float r2 = GetRandomValue(states);
    float z = sqrt(1.0f - r2);
    float phi = 2.0f * M_PI * r1;
    float x = cos(phi) * sqrt(r2);
    float y = sin(phi) * sqrt(r2);
    return float3(x, y, z);
}

float3 GetRandomGlossyCosineDirection(inout uint4 states, float shininess) {
    float r1 = GetRandomValue(states);
    float r2 = GetRandomValue(states);
    float z = pow((1.0f - r2), (1 / (1 + shininess)));
    float phi = 2.0f * M_PI * r1;
    float x = cos(phi) * sqrt(r2);
    float y = sin(phi) * sqrt(r2);
    return float3(x, y, z);
}

uint tea(uint val0, uint val1)
{
    uint v0 = val0;
    uint v1 = val1;
    uint s0 = 0;
    for (uint n = 0; n < 16; n++)
    {
        s0 += 0x9e3779b9;
        v0 += ((v1 << 4) + 0xa341316c) ^ (v1 + s0) ^ ((v1 >> 5) + 0xc8013ea4);
        v1 += ((v0 << 4) + 0xad90777d) ^ (v0 + s0) ^ ((v0 >> 5) + 0x7e95761e);
    }
    return v0;
}

float g_rand(inout uint seed)
{
    const uint LCG_A = 1664525u;
    const uint LCG_C = 1013904223u;
    float val = float(seed & 0x00FFFFFF) / float(0x01000000);
    seed = (LCG_A * seed + LCG_C);
    return val;
}

float3 diffuse_sample(inout uint seed, float3 vn)
{
    float3 ax, ay, az;
    float sint, cost, phi;
    ay = vn;
    if (abs(ay.x) < abs(ay.y))
        az = normalize(cross(float3(1, 0, 0), ay));
    else
        az = normalize(cross(float3(0, 1, 0), ay));
    ax = cross(ay, az);
    cost = pow(g_rand(seed), 1.0 / 2);
    sint = sqrt(1 - cost * cost);
    phi = g_rand(seed) * 2 * M_PI;
    return normalize(cost * ay + sint * (sin(phi) * ax + cos(phi) * az));
}

float3 glossy_sample(inout uint4 states, float3 eye, float3 vn, float shininess)
{
    float3 ax, ay, az;
    float sint, cost, phi;
    ay = normalize(reflect(eye, vn));
    az = normalize(cross(vn, ay));
    ax = cross(ay, az);
    cost = pow(GetRandomValue(states), 1 / (1 + shininess));
    sint = sqrt(1 - cost * cost);
    phi = GetRandomValue(states) * 2 * M_PI;
    return normalize(cost * ay + sint * (sin(phi) * ax + cos(phi) * az));
}
float3 glossy_sample0(inout uint seed, float3 eye, float3 vn, float shininess)
{
    float3 ax, ay, az;
    float sint, cost, phi;
    ay = normalize(reflect(eye, vn));
    az = normalize(cross(vn, ay));
    ax = cross(ay, az);
    cost = pow(g_rand(seed), 1 / (1 + shininess));
    sint = sqrt(1 - cost * cost);
    phi = g_rand(seed) * 2 * M_PI;
    return normalize(cost * ay + sint * (sin(phi) * ax + cos(phi) * az));
}

#endif