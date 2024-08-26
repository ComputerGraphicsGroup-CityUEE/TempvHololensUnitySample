struct IntersectionVertex
{
    // Object space normal of the vertex
    float3 normalOS;
    float2 texCoord0;
    float3 color;
    float4 tangentOS;
};

// Send data to GPU buffer
TEXTURE2D(_BaseColorMap);
SAMPLER(sampler_BaseColorMap);
TEXTURE2D(_NormalMap);
SAMPLER(sampler_NormalMap);
CBUFFER_START(UnityPerMaterial)
float4 _BaseColorMap_ST;
float4 _Color;
int _UseNormalMap;
float _kSpecular;
float _kDiffuse;
float _Metallic;
float _Roughness;
CBUFFER_END

int _RenderTypeFlag;
int _FrameIndex;
float4 _OutputTargetSize;

void FetchIntersectionVertex(uint vertexIndex, out IntersectionVertex outVertex)
{
    // Get vertex normal data in object space.
    outVertex.normalOS = UnityRayTracingFetchVertexAttribute3(vertexIndex, kVertexAttributeNormal);
    // Get the UV
    outVertex.texCoord0 = UnityRayTracingFetchVertexAttribute2(vertexIndex, kVertexAttributeTexCoord0);
    outVertex.color = UnityRayTracingFetchVertexAttribute3(vertexIndex, kVertexAttributeColor);
    outVertex.tangentOS = UnityRayTracingFetchVertexAttribute4(vertexIndex, kVertexAttributeTangent);
}

[shader("closesthit")]
void ClosestHitShader(inout RayIntersection rayIntersection : SV_RayPayload,
    AttributeData attributeData : SV_IntersectionAttributes)
{
    const uint2 dispatchIdx = DispatchRaysIndex().xy;
    const uint PRNGIndex = dispatchIdx.y * _OutputTargetSize.x + dispatchIdx.x;
    uint seed = tea(PRNGIndex, _FrameIndex * _OutputTargetSize.x);

    // Fetch the indices of the currentr triangle
    // Get the index value of the ray traced hit triangle.
    uint3 triangleIndices = UnityRayTracingFetchTriangleIndices(PrimitiveIndex());

    // Fetch the 3 vertices
    IntersectionVertex v0, v1, v2;
    FetchIntersectionVertex(triangleIndices.x, v0);
    FetchIntersectionVertex(triangleIndices.y, v1);
    FetchIntersectionVertex(triangleIndices.z, v2);

    // Compute the full barycentric coordinates
    float3 barycentricCoordinates = float3(1.0 - attributeData.barycentrics.x - attributeData.barycentrics.y, attributeData.barycentrics.x, attributeData.barycentrics.y);

    float3 pp = barycentricCoordinates;
    if (pp.x > pp.y && pp.x > pp.z)
        rayIntersection.vi_noweld = triangleIndices.x;
    else if (pp.y > pp.z)
        rayIntersection.vi_noweld = triangleIndices.y;
    else
        rayIntersection.vi_noweld = triangleIndices.z;

    //rayIntersection.vi_noweld = triangleIndices.z;

    // Interpolation to calculate the specific information of the collision point
    float3 normalOS = INTERPOLATE_RAYTRACING_ATTRIBUTE(v0.normalOS, v1.normalOS, v2.normalOS, barycentricCoordinates);
    float2 texCoord0 = INTERPOLATE_RAYTRACING_ATTRIBUTE(v0.texCoord0, v1.texCoord0, v2.texCoord0, barycentricCoordinates);
    float3 vertexColor = INTERPOLATE_RAYTRACING_ATTRIBUTE(v0.color, v1.color, v2.color, barycentricCoordinates);
    // DXR object to world transform matrix
    float3x3 objectToWorld = (float3x3)ObjectToWorld3x4();
    float4 texColor = _BaseColorMap.SampleLevel(sampler_BaseColorMap, texCoord0, 0);
    float3 normalWS = normalize(mul(objectToWorld, normalOS));

    // Get position in world space.
    float3 origin = WorldRayOrigin();
    float3 direction = WorldRayDirection();
    float t = RayTCurrent();
    float3 positionWS = origin + direction * t;

    rayIntersection.normalWS = float3(normalWS.x, normalWS.y, normalWS.z);//normalWS;
    rayIntersection.normalOS = float3(normalOS.x, normalOS.y, normalOS.z);
    rayIntersection.reflector = 1.0f;
    rayIntersection.color =  texColor * _Color;
    rayIntersection.hitT = RayTCurrent();

    // mis 
    rayIntersection.kDiffuse = _kDiffuse;
    rayIntersection.kSpecular = _kSpecular;
    rayIntersection.shininess = 80000;

    rayIntersection.color = texColor;
    rayIntersection.roughness = _Roughness;
    rayIntersection.metallic = _Metallic;
    {
        float n;
        n = pow(2, (1 - rayIntersection.roughness) * 6 + 2);
        rayIntersection.shininess = float(n * n) / 4;
    }

    // VSAT
    if(_RenderTypeFlag == 1)
    {
        rayIntersection.roughness = _Roughness;
        {
            float n;
            n = pow(2, (1 - rayIntersection.roughness) * 6 + 2);
            rayIntersection.shininess = min(rayIntersection.shininess, float(n * n) / 4);
        }

        float3 av = positionWS; // hit position
        float3 eye = -normalize(direction); 
        float4x4 mtx4 = float4x4(
            1.0f, 0.0f, 0.0f, 0.0f,
            0.0f, 1.0f, 0.0f, 0.0f,
            0.0f, 0.0f, 1.0f, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f
        );
        rayIntersection.ti = PrimitiveIndex();  // hit triangle index
        rayIntersection.barycentricCoordinates = barycentricCoordinates;
    }

}