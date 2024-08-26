using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class qObjectManager
{
    public List<Vector3> vPos = new List<Vector3>();
    public List<Vector3> vNormals = new List<Vector3>();
    public List<Vector3> vPos_world = new List<Vector3>();
    public List<Vector3> vPos_unitize = new List<Vector3>();
    public List<int> vtIdx = new List<int>();
    public List<int> tIdx = new List<int>();
    public List<int> vIdx = new List<int>();

    public int nTriangle;

    public Vector3 boundMax;
    public Vector3 boundMin;
    public Vector3 objCenter;

    public String cachePath;

    List<int> vPosLastIdxByObj = new List<int>();
    List<int> triangleLastIdxByObj = new List<int>();

    public Matrix4x4 localToWorld;
    public Matrix4x4 worldToLocal;
    Matrix4x4[] bindPoses;
    Mesh mesh;

    public qObjectManager()
    {
        vPos = new List<Vector3>();
        vNormals = new List<Vector3>();
        vPos_world = new List<Vector3>();
        vPos_unitize = new List<Vector3>();
        vtIdx = new List<int>();
        tIdx = new List<int>();
        vIdx = new List<int>();

        nTriangle = 0;

        boundMax = new Vector3();
        boundMin = new Vector3();
        objCenter = new Vector3();

        vPosLastIdxByObj.Clear();
        triangleLastIdxByObj.Clear();

        localToWorld = new Matrix4x4();
        worldToLocal = new Matrix4x4();

        mesh = new Mesh();

        if (Application.isPlaying)
            cachePath = Application.streamingAssetsPath + "//";
        else
            cachePath = "Assets/StreamingAssets/";
    }

    public void UnitizeObjects(ref GameObject[] objects,
                               ref GameObject q_obj,
                               ref List<qVtAnimData.qFrameData[]> frameDatasWS_unitized,
                               ref int planeCount,
                               List<qVtAnimData> objAnimOSList)
    {
        vPos.Clear();
        vPos_world.Clear();
        vPos_unitize.Clear();
        vNormals.Clear();
        vtIdx.Clear();
        tIdx.Clear();
        vIdx.Clear();
        vPosLastIdxByObj.Clear();
        triangleLastIdxByObj.Clear();
        nTriangle = 0;
        boundMax = new Vector3();
        boundMin = new Vector3();
        objCenter = new Vector3();
        localToWorld = new Matrix4x4();
        worldToLocal = new Matrix4x4();
        mesh = new Mesh();

        List<Vector3> vPos_plane = new List<Vector3>();
        List<Vector3> vPos_world_plane = new List<Vector3>();
        List<Vector3> vNormals_plane = new List<Vector3>();

        int objIdx = 0;
        int meshFiltersN = objects.Length;
        CombineInstance[] combine = new CombineInstance[meshFiltersN];
        List<Material> materials = new List<Material>();

        foreach (GameObject obj in objects)
        {
            if (obj.GetComponent<MeshFilter>() != null)
                mesh = obj.GetComponent<MeshFilter>().sharedMesh;
            else
                mesh = obj.GetComponent<SkinnedMeshRenderer>().sharedMesh;

            localToWorld = obj.transform.localToWorldMatrix;
            bindPoses = obj.GetComponent<MeshFilter>().sharedMesh.bindposes;
            boundMax = new Vector3(mesh.bounds.max.x, mesh.bounds.max.y, mesh.bounds.max.z);
            boundMin = new Vector3(mesh.bounds.min.x, mesh.bounds.min.y, mesh.bounds.min.z);

            int firstVertex = vPos.Count;
            int firstTriangle = tIdx.Count;

            var tempT = Enumerable.Range(0, mesh.triangles.Count() / 3).ToArray();      // tempT.count = how many triangle we have got
            nTriangle += tempT.Length;

            vPos.AddRange(mesh.vertices);
            vPos_world.AddRange(mesh.vertices.Select(vec => localToWorld.MultiplyPoint3x4(vec)));
            if (!IsAnimModel(obj))
            {
                vPos_plane.AddRange(mesh.vertices);
                vPos_world_plane.AddRange(mesh.vertices.Select(vec => localToWorld.MultiplyPoint3x4(vec)));
                vNormals_plane.AddRange(mesh.normals);
                if ( !q_common.q_IO.CheckFileExists(cachePath + "vPos_world_plane") )
                    q_common.q_IO.SaveVector3ArrayToFile(vPos_world_plane.ToArray(), cachePath + "vPos_world_plane");
                if ( !q_common.q_IO.CheckFileExists(cachePath + "vNormals_plane") )
                    q_common.q_IO.SaveVector3ArrayToFile(vNormals_plane.ToArray(), cachePath + "vNormals_plane");
            }
            vNormals.AddRange(mesh.normals);

            tIdx.AddRange(tempT.Select(index => index + firstTriangle));
            vtIdx.AddRange(mesh.triangles.Select(index => index + firstVertex));

            vPosLastIdxByObj.Add(vPos.Count);
            triangleLastIdxByObj.Add(tIdx.Count);
            objIdx++;
        }

        ////////////////// Unitize the model ////////////////////
        q_common.q_common.GetBounds(vPos_world, ref boundMax, ref boundMin, ref objCenter);
        if (!q_common.q_IO.CheckFileExists(cachePath + "bound_info"))
        {
            List<Vector3> boundsListTmp = new();
            boundsListTmp.Add(boundMax);
            boundsListTmp.Add(boundMin);
            boundsListTmp.Add(objCenter);
            q_common.q_IO.SaveVector3ArrayToFile(boundsListTmp.ToArray(), cachePath + "bound_info");
        }
        vPos_unitize.AddRange(vPos_world.Select(vec => q_common.q_common.Unitize(vec, objCenter, boundMax, boundMin)));
        ////////////////////////////////////////////////////////

        ///////////////// Save the unitized vertex os back to the mesh
        int i = 0;
        foreach (GameObject obj in objects)
        {
            if (obj.GetComponent<MeshFilter>() != null)
                mesh = obj.GetComponent<MeshFilter>().sharedMesh;
            else
                mesh = obj.GetComponent<SkinnedMeshRenderer>().sharedMesh;
            MeshRenderer renders = obj.GetComponent<MeshRenderer>();

            List<Vector3> m_vPos_unitizedOS = new List<Vector3>();
            List<Vector3> m_vPos_unitizedWS = new List<Vector3>();

            if (i == 0)
            {
                m_vPos_unitizedWS = vPos_unitize.GetRange(0, vPosLastIdxByObj[i]);
            }
            else
            {
                m_vPos_unitizedWS = vPos_unitize.GetRange(vPosLastIdxByObj[i - 1], vPosLastIdxByObj[i] - vPosLastIdxByObj[i - 1]);
            }

            worldToLocal = obj.transform.worldToLocalMatrix;
            m_vPos_unitizedOS.AddRange(m_vPos_unitizedWS.Select(vec => worldToLocal.MultiplyPoint3x4(vec)));

            mesh.vertices = m_vPos_unitizedOS.ToArray();
            mesh.RecalculateBounds();

            ///////////////////////// Combine them into a single mesh /////////////////////////////////

            combine[i].mesh = mesh;
            combine[i].transform = obj.transform.localToWorldMatrix;
            materials.Add(renders.sharedMaterial);

            i++;
        }

        /////////////// Combine obj and plane as a single mesh ///////////
        Mesh combine_mesh = new Mesh();
        combine_mesh.CombineMeshes(combine, false, true);

        MeshFilter q_filter = q_obj.GetComponent<MeshFilter>(); 
        MeshRenderer q_meshrenderer = q_obj.GetComponent<MeshRenderer>();
        q_filter.sharedMesh = combine_mesh;
        q_meshrenderer.materials = materials.ToArray();

        /////////////// Prepare Animation ////////////////////
        List<qVtAnimData.qFrameData[]> frameDatasWS = new();
        if (q_common.q_IO.CheckFileExists(cachePath + "vPos_world_plane"))
            vPos_world_plane = q_common.q_IO.LoadVector3ArrayFromFile(cachePath + "vPos_world_plane");
        if (q_common.q_IO.CheckFileExists(cachePath + "vNormals_plane"))
            vNormals_plane = q_common.q_IO.LoadVector3ArrayFromFile(cachePath + "vNormals_plane");
        planeCount = vPos_world_plane.Count;
        int animCount = objAnimOSList.Count;
        foreach (GameObject obj in objects)
        {
            if (obj.GetComponent<MeshFilter>() != null)
            {
                Debug.Log("MeshRenderer was found on " + obj.gameObject.name);
                mesh = obj.GetComponent<MeshFilter>().sharedMesh;
            }
            else
            {
                Debug.Log("No MeshRenderer was found on " + obj.gameObject.name);
                return;
            }

            localToWorld = obj.transform.localToWorldMatrix;

            if (IsAnimModel(obj))
            {
                for (int j = 0; j < animCount; j++)
                {
                    int frameLen = objAnimOSList[j].frameDatas.Length;
                    List<qVtAnimData.qFrameData> m_qFrameDataList = new();
                    for (int k = 0; k < frameLen; k++)
                    {
                        List<Vector3> temp_animVertex = new();
                        List<Vector3> temp_animVNormal = new();
                        qVtAnimData.qFrameData src_frameData = objAnimOSList[j].frameDatas[k];
                        qVtAnimData.qFrameData des_frameData = new();

                        temp_animVertex.AddRange(src_frameData.animVertex.Select(vec => localToWorld.MultiplyPoint3x4(vec)));
                        temp_animVertex.AddRange(vPos_world_plane);

                        temp_animVNormal.AddRange(src_frameData.animVNormal);
                        temp_animVNormal.AddRange(vNormals_plane);

                        des_frameData.time = src_frameData.time;
                        des_frameData.animVertex = temp_animVertex.ToArray();
                        des_frameData.animVNormal = temp_animVNormal.ToArray();

                        m_qFrameDataList.Add(des_frameData);
                    }
                    frameDatasWS.Add(m_qFrameDataList.ToArray());
                }
            }
        }

        /////////////// Unitize the Animation /////////////////
        List<Vector3> boundsList = q_common.q_IO.LoadVector3ArrayFromFile(cachePath + "bound_info");
        boundMax = boundsList[0];
        boundMin = boundsList[1];
        objCenter = boundsList[2];
        for (int j = 0; j < animCount; j++)
        {
            int frameLen = frameDatasWS[j].Length;
            List<qVtAnimData.qFrameData> m_qFrameDataList = new();
            for (int k = 0; k < frameLen; k++)
            {
                List<Vector3> temp_animVertex = new();
                List<Vector3> temp_animVNormal = new();
                qVtAnimData.qFrameData src_frameData = frameDatasWS[j][k];
                qVtAnimData.qFrameData des_frameData = new();

                temp_animVertex.AddRange(src_frameData.animVertex.Select(vec => q_common.q_common.Unitize(vec, objCenter, boundMax, boundMin)));
                temp_animVNormal.AddRange(src_frameData.animVNormal);
                des_frameData.time = src_frameData.time;
                des_frameData.animVertex = temp_animVertex.ToArray();
                des_frameData.animVNormal = temp_animVNormal.ToArray();

                m_qFrameDataList.Add(des_frameData);
            }
            frameDatasWS_unitized.Add(m_qFrameDataList.ToArray());
        }
    }

    public bool IsAnimModel(GameObject obj)
    {
        if (obj.GetComponent<qObjectAnimationAsset>() != null)
            return true;
        else
            return false;
    }

    public void ReloadModelData(ref GameObject[] objects, ref GameObject q_obj)
    {
        localToWorld = new Matrix4x4();
        vPos_unitize.Clear();
        vNormals.Clear();

        CombineInstance[] combine = new CombineInstance[objects.Length];

        int i = 0;
        foreach (GameObject obj in objects)
        {
            if (obj.GetComponent<MeshFilter>() != null)
                mesh = obj.GetComponent<MeshFilter>().sharedMesh;
            else
                return;

            localToWorld = obj.transform.localToWorldMatrix;
            int firstVertex = vPos_unitize.Count;
            int firstTriangle = tIdx.Count;

            vPos_unitize.AddRange(mesh.vertices.Select(vec => localToWorld.MultiplyPoint3x4(vec)));
            vNormals.AddRange(mesh.normals.Select(vec => localToWorld.MultiplyPoint3x4(vec)));

            combine[i].mesh = mesh;
            combine[i].transform = obj.transform.localToWorldMatrix;

            i++;
        }

        Mesh combine_mesh = new Mesh();
        combine_mesh.CombineMeshes(combine, false, true);
        q_obj.GetComponent<MeshFilter>().sharedMesh.Clear();
        q_obj.GetComponent<MeshFilter>().sharedMesh = combine_mesh;
    }

    public void LoadObjVertexAnimation(ref GameObject[] qObjects, ref List<qVtAnimData> objVtAnimDataList)
    {
        foreach (GameObject qObj in qObjects)
        {
            if (qObj.gameObject.GetComponent<qObjectAnimationAsset>() != null)
                objVtAnimDataList = qObj.gameObject.GetComponent<qObjectAnimationAsset>().vtAnimDatasList;

        }
    }

    void Clear()
    {
        vPos.Clear();
        vPos_world.Clear();
        vPos_unitize.Clear();
        vNormals.Clear();
        vtIdx.Clear();
        tIdx.Clear();
        vIdx.Clear();

        localToWorld = new Matrix4x4();
        worldToLocal = new Matrix4x4();
        boundMax = new Vector3();
        boundMin = new Vector3();
        objCenter = new Vector3();
        nTriangle = 0;

        vPosLastIdxByObj.Clear();
        triangleLastIdxByObj.Clear();
    }
}