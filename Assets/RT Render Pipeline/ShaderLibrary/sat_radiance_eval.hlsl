#ifndef CUSTOM_SAT_RADIANCE_EVAL_INCLUDED
#define CUSTOM_SAT_RADIANCE_EVAL_INCLUDED


float capfunc(float3 ll, float4 II, float4 JJ, float cost)
{
    //return clamp((dot(ll, II.xyz) - cost * II.w) / max(dot(ll, JJ.xyz) - cost * JJ.w, .0001), 0.0, 1.0);
    return clamp((dot(II.xyz, ll) - cost * II.w) / max(dot(JJ.xyz, ll) - cost * JJ.w, .0001), 0.0, 1.0);
}

float mixcapia(float iaval, float capval, float s)
{
    return mix(capval, iaval, clamp((G_PI / 18.0 - s) / (G_PI / 18.0), 0.0, 1.0));
}


float4 main2(float2 spos, float3 epos, float4x4 mtx,
    int ti, float3 pc, float3 av, float3 bn,
    float3 kd, float3 ks, float roughness,
    uint seed, uint vi_noweld)
{
    //////////////////////////////////////////////////////////////////
    /// <summary>
    /// From this point onward, av, bn, and epos are assumed to be represented in 
    /// right hand coordinate system instead of left hand coordinate system.
    /// </summary>
    av.z = -av.z;
    bn.z = -bn.z;
    epos.z = -epos.z;
    //////////////////////////////////////////////////////////////////

    float shininess, cone_radius;
    {
        float n;
        n = pow(2, (1 - roughness) * 6 + 2);
        shininess = float(n * n) / 4;
        cone_radius = sqrt(4.0 / 3 / (shininess)) * G_PI / 2;
    }
    float3 refdir = normalize(reflect(av - epos, bn));
    float3 eye = refdir;
    float3 ax, ay, az;
    ay = refdir;// bn;
    //ay = bn;
    ax = normalize(cross(refdir, bn));
    az = cross(ax, ay);

    float cost, sint, phi;
    cost = pow(g_rand(seed), 1.0 / (1 + shininess));
    sint = sqrt(1 - cost * cost);
    phi = g_rand(seed) * 2 * G_PI;
    ay = normalize(cost * ay + sint * (sin(phi) * ax + cos(phi) * az));
    ax = normalize(cross(ay, bn));
    az = cross(ax, ay);
    eye = ay;

    float4 ID, II, JJ;
    float IA;
    float z0 = g_rand(seed);
    float z1 = g_rand(seed);
    float z2 = g_rand(seed);
    float3 pp = pc * float3(z0, z1, z2);

    float3 bx, by, bz;
    float3 cx, cy, cz;
    uint ii0, ii1, vti, vi_0, vi_1;

    if (pp.x > pp.y && pp.x > pp.z)
        vti = 3 * ti + 0;               // index to lookup vtIdx, vtIdx is not-weld
    else if (pp.y > pp.z)
        vti = 3 * ti + 1;
    else
        vti = 3 * ti + 2;

    float3 ay0 = bn;
    get_coordinate_system(ay0, bx, by, bz);

    ii0 = _des_vinfo[_weld_vtIdx_map[vi_noweld]];
    ii1 = _des_vinfo[_weld_vtIdx_map[vi_noweld] + 1];
    
    float3x3 mtx3 = float3x3(
        1.0f, 0.0f, 0.0f,
        0.0f, 1.0f, 0.0f,
        0.0f, 0.0f, 1.0f
    );

    vsatlookup(ii0, ii1, av, mtx3, ID,
        cone_radius, ax, ay, az, bx, bz, II, JJ, IA);

    float diffuse, specular;
    diffuse = dot(bn, ID.xyz);
    specular = capfunc(eye, II, JJ, cos(cone_radius));
    specular = mixcapia(clamp(IA, 0.0, 1.0), specular, cone_radius);

    float4 dcol = _CubeTexture.SampleLevel(_SamLinearClamp, float3(bn.x, bn.y, -bn.z), 9.0);
    float4 scol = _CubeTexture.SampleLevel(_SamLinearClamp, float3(eye.x, eye.y, -eye.z), roughness * 9);

    float3 col_sat = normalize(dcol.xyz) * diffuse * kd + scol.xyz * ks * specular;

    return float4(col_sat, 1);
}
#endif
