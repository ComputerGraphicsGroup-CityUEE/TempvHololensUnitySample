#ifndef CUSTOM_SATLOOKUP_INCLUDED
#define CUSTOM_SATLOOKUP_INCLUDED

float2 Float32Angle(float3 l, float drange)
{
    l.xz += .00001;
    float2 tp;
    tp.x = atan2(-l.z, l.x) / (2.0 * G_PI);  // [-pi,pi] => [-.5,.5]
    tp.x += step(tp.x, 0.0);                 //          => [0,1]
    tp.y = acos(l.y) / G_PI * drange;
    return tp;
}

// Note that, b0 should be an unit vector
float3 specboundc(float2 bound, float3 nx, float3 ny, float3 nz, float3 b0)
{
    if (dot(b0, ny) < bound.x)
    {
        b0 = float3(dot(b0, nx), bound.x, dot(b0, nz));
        b0.xz *= bound.y / (length(b0.xz) + 0.0001);
        b0 = b0.x * nx + b0.y * ny + b0.z * nz;
    }
    return b0;
}

float4 mTexture(float2 uv)
{
    float4 color = _sat.SampleLevel(sampler_sat, uv, 0);

    return color;
}

float4 Texture_LutBrdf(float2 uv)
{
    float4 color = _lutBrdf.SampleLevel(sampler_lutBrdf, uv, 0);

    return color;
}

float4 tppair(float4 cc, float dside)
{
    float d;
    d = cc.z - cc.x;
    cc.x += step(.5, d);
    cc.z += step(d, -.5);
    cc.xz = cc.xz / 1.5 + .5 / (dside * 1.5);
    return cc;
}

float4 satlookup(float4 cc, float dside, float drange)
{
    float2 A = cc.xy - cc.zw;
    float4 E =  mTexture(cc.xy) - mTexture(cc.zy);
    float4 F =  mTexture(cc.xw) - mTexture(cc.zw);
    float dl = .5;
    if (abs(A.x) > 0.0001 && abs(A.y) > 0.0001)
    {
        float2 B = Float32Angle(normalize((E.xyz - F.xyz) * sign(E.w - F.w)), drange);
        B.x = (B.x + .5 / dside) / 1.5;
        B.x += step(B.x, min(cc.x, cc.z)) / 1.5;
        B -= cc.zw;
        dl += tanh(5.87 * clamp((B.y / A.y - B.x / A.x), -1.0, 1.0)) * .5;
    }
    return mix(E, F, dl);
}

void get_coordinate_system(float3 ay, out float3 bx, out float3 by, out float3 bz)
{
    by = normalize(ay);
    bx = normalize(float3(1, 0, 0));
    float3 ref = mix(float3(by.z, 0.f, -by.x), bx, abs(by.y));
    bz = normalize(cross(ref, by));
    bx = normalize(cross(by, bz));
}


void get_coordinate_system_inverse(float3 ay, out float3 bx, out float3 by, out float3 bz)
{
    by = normalize(ay);
    bx = normalize(float3(1, 0, 0));
    float3 ref = mix(float3(by.z, 0.f, -by.x), bx, abs(by.y));
    bz = normalize(cross(ref, by));
    bz = -bz;
    bx = normalize(cross(by, bz));
}
    
void gete(out float3 v0, out float3 v1, uint ei, float3 av, float3 bx, float3 bz)
{
    int ei0 = INPUT_EDAT_EI[2 * ei + 0];
    int ei1 = INPUT_EDAT_EI[2 * ei + 1];

    if (ei0 != -1)
    {
        int ev0id = int(INPUT_EMAP[ei0].x);
        int ev1id = int(INPUT_EMAP[ei0].y);

        float3 ev0 = INPUT_VMAP_NOWELD[ev0id];
        float3 ev1 = INPUT_VMAP_NOWELD[ev1id];

        v0 = INPUT_EDAT_ES[2 * ei + 0] * (ev1 - ev0) + (ev1 + ev0) / 2.f;
    }
    else
    {
        v0 = av + (cos(INPUT_EDAT_ES[2 * ei + 0]) * bz + sin(INPUT_EDAT_ES[2 * ei + 0]) * bx) * 2;
    }

    if (ei1 != -1)
    {
        int ev0id = int(INPUT_EMAP[ei1].x);
        int ev1id = int(INPUT_EMAP[ei1].y);

        float3 ev0 = INPUT_VMAP_NOWELD[ev0id];
        float3 ev1 = INPUT_VMAP_NOWELD[ev1id];

        v1 = INPUT_EDAT_ES[2 * ei + 1] * (ev1 - ev0) + (ev1 + ev0) / 2.f;
    }
    else
    {
        v1 = av + (cos(INPUT_EDAT_ES[2 * ei + 1]) * bz + sin(INPUT_EDAT_ES[2 * ei + 1]) * bx) * 2;
    }
}

void vsatlookup(
    uint i0, uint i1, float3 cv, float3x3 mtx, out float4 ID,
    float phongbound,
    float3 ax, float3 ay, float3 az,
    float3 bx, float3 bz,
    out float4 II, out float4 JJ, out float IA
)
{
    float3 ll;
    float2 d0, d1;
    float2 a0, a1;
    float2 g0, g1, s0, s1;

    float width, height;
    width = 1536.f;
    height = 512.f;

    float cmside = width / 6.f;
    float dside = cmside * 4;
    float dend = ((dside - .5) / (dside + dside / 2.0f));
    float drange = (1.0f - .5 / cmside);

    float2 bound = float2(cos(phongbound), sin(phongbound));

    float ss;
    float lamda;
    ss = G_PI / phongbound; 
    ss = ss * ss;
    ss = min(ss, 7269.0);
    lamda = 1.209935121 / sqrt(pow(0.3, -2.0 / ss) - 1.0);
    lamda = min(lamda, 200.0) * .9;

    ID = float4(0.0f, 0.0f, 0.0f, 0.0f);
    II = float4(0.0f, 0.0f, 0.0f, 0.0f);
    IA = 0.0;

    for (uint i = i0; i < i1; i += 2)
    {
        float3 u0, u1;
        gete(u0, u1, i / 2, cv, bx, bz);

        u0 = normalize(u0 - cv);
        u1 = normalize(u1 - cv);

        float theta = acos(clamp(dot(u0, u1), -1.f, 1.f));
        uint PP = uint(ceil(theta / (2 * G_PI * 1.5 / 32)));

        for (uint pp = 0; pp < PP; pp++)
        {
            float3 v0, v1;
            v0 = normalize(mix(u0, u1, float(pp + 0) / PP));
            v1 = normalize(mix(u0, u1, float(pp + 1) / PP));

            ll = v0;
            d0 = Float32Angle(ll, drange);
            a0 = Float32Angle(specboundc(bound, ax, ay, az, normalize(ll)), drange);
            ll = float3(dot(ax, ll), dot(ay, ll), dot(az, ll));
            g0 = ll.xz / max(ll.y, .00001);
            s0 = tanh(clamp(lamda * g0, -40.0, 40.0));

            ll = v1;
            d1 = Float32Angle(ll, drange);
            a1 = Float32Angle(specboundc(bound, ax, ay, az, ll), drange);
            ll = float3(dot(ax, ll), dot(ay, ll), dot(az, ll));
            g1 = ll.xz / max(ll.y, .00001);
            s1 = tanh(clamp(lamda * g1, -40.0, 40.0));

            ID += satlookup( tppair(float4(d0, d1), dside) , dside, drange );
            II += satlookup( tppair(float4(a0, a1), dside),  dside, drange );
            IA += (s0.x - s1.x) * mix(s0.y, s1.y, tanh( lamda * 0.5 * clamp(g1.x / (g0.x - g1.x) - g1.y / (g0.y - g1.y), -1.0, 1.0) ) * .5 + .5);
        }
    }

    ID = -ID;
    II = -II;
    IA = -IA;

     
    if (ID.w < -0.01)
        ID += mTexture(float2(dend, 1.0));
    if (II.w < -1.0)
        II += mTexture(float2(dend, 1.0));
    IA /= 4.0;

    int nstep = 32;
    float cost, sint, cosp, sinp;
    float pp;
    JJ = float4(0.0f, 0.0f, 0.0f, 0.0f);
    cost = bound.x;
    sint = bound.y;
    a0 = Float32Angle(sint * ax + cost * ay, drange);
    pp = 0.0;
    for (int i = 0; i < nstep; i++)
    {
        pp += (2.0 * G_PI) / float(nstep);
        sinp = sin(pp);
        cosp = cos(pp);
        a1 = Float32Angle(sint * cosp * ax + cost * ay + sint * sinp * az, drange);
        JJ += satlookup(tppair(float4(a0, a1), dside), dside, drange);
        a0 = a1;
    }
    if (JJ.w < -0.1)
        JJ += mTexture(float2(dend, 1.0));
}

#endif